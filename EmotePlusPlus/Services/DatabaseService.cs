using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Discord;
using EmotePlusPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmotePlusPlus.Services
{
    public class DatabaseService
    {
        public bool CanAcceptNewUpdates { get; set; } = true;

        private readonly static string TABLE_EMOTES = "emotes";
        private readonly static string TABLE_USERS = "users";
        private readonly static string TABLE_CHANNEL_UPDATES = "channel_updates";

        private readonly DiscordSocketClient _discord;
        private readonly LiteDatabase _database;

        public DatabaseService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _database = services.GetRequiredService<LiteDatabase>();

            _discord.Ready += OnReady;
        }

        private async Task OnReady()
        {
            await _discord.SetGameAsync($"Scanning channels... beep boop");

            CanAcceptNewUpdates = false;
            var channels = _discord.Guilds.First().Channels.Where(x => x.GetType() == typeof(SocketTextChannel));

            foreach (var channel in channels)
            {
                var lastUpdate = LastChannelUpdate(channel.Id);

                var firstMessage = (await(channel as ISocketMessageChannel).GetMessagesAsync(1).FlattenAsync()).First();
                if (firstMessage.Tags.Any() && firstMessage.Timestamp > lastUpdate)
                    Update(firstMessage.Tags, firstMessage.Author.Id, firstMessage.Channel.Id, false, DateTimeOffset.UtcNow);

                var lastMessage = firstMessage;
                bool done = firstMessage.Timestamp <= lastUpdate;

                while (!done)
                {
                    var messages = await(channel as ISocketMessageChannel).GetMessagesAsync(lastMessage, Direction.Before, 100, CacheMode.AllowDownload).FlattenAsync();
                    foreach (var message in messages)
                    {
                        if (message.Tags.Any() && message.Timestamp > lastUpdate)
                            Update(message.Tags, message.Author.Id, message.Channel.Id, false, DateTimeOffset.UtcNow);
                    }

                    if (!messages.Any())
                    {
                        done = true;
                    }
                    else
                    {
                        lastMessage = messages.Last();
                        if (lastMessage.Timestamp < lastUpdate)
                        {
                            done = true;
                        }
                    }
                }

                UpdateLastChannelUpdate(channel.Id, firstMessage.Timestamp);
            }
            CanAcceptNewUpdates = true;

            await _discord.SetGameAsync("emote catcher");

            _discord.Ready -= OnReady;
        }

        public void Update(IReadOnlyCollection<ITag> tags, ulong userId, ulong channelId, bool updateChannelTimestamp, DateTimeOffset offset)
        {
            var emojis = tags.Where(x => x.Type == TagType.Emoji).Select(x => x.Value as Discord.Emote).ToList();

            if (emojis.Any())
            {
                var emotes = _database.GetCollection<Models.Emote>(TABLE_EMOTES);

                var userCollection = _database.GetCollection<User>(TABLE_USERS);
                var user = userCollection.FindOne(x => x.Id == userId) ?? new User
                {
                    Id = userId,
                    UsedEmotes = new List<EmoteData>()
                };

                // Check if emoji is already present in the database, if not
                // add it.
                foreach (var emoji in emojis)
                {
                    // Get the emote from the database whose data has to be updated            
                    var emote = emotes.FindOne(e => e.Id == emoji.Id) ?? new Models.Emote
                    {
                        Id = emoji.Id,
                        Name = emoji.Name,
                        CreatedAt = emoji.CreatedAt.DateTime,
                        Uses = 0,
                        Animated = emoji.Animated
                    };

                    emote.Uses++;
                    emotes.Upsert(emote);

                    // Get the usage data of the used emote from the user
                    EmoteData emoteData = user.UsedEmotes.FirstOrDefault(x => x.Id == emote.Id && x.ChannelId == channelId) ?? new EmoteData
                    {
                        Id = emote.Id,
                        Animated = emote.Animated,
                        ChannelId = channelId,
                        Name = emote.Name,
                        Uses = 0
                    };

                    if (emoteData.Uses == 0)
                    {
                        user.UsedEmotes.Add(emoteData);
                    }

                    emoteData.Uses++;
                    userCollection.Upsert(user);
                }

                if (updateChannelTimestamp)
                {
                    var channelUpdateCollection = _database.GetCollection<ChannelUpdate>(TABLE_CHANNEL_UPDATES);
                    var channelUpdate = channelUpdateCollection.FindOne(x => x.ChannelId == channelId);

                    if (channelUpdate == null)
                    {
                        channelUpdate = new ChannelUpdate
                        {
                            ChannelId = channelId,
                            LastUpdate = DateTime.UnixEpoch
                        };
                    }

                    channelUpdate.LastUpdate = offset.UtcDateTime;
                    channelUpdateCollection.Upsert(channelUpdate);
                }

            }
        }

        public List<EmoteQueryResult> GetTopAll(int number)
        {
            if (number <= 0) number = 10;
            if (number > 25) number = 25;
            return _database.GetCollection<Models.Emote>(TABLE_EMOTES)
                .FindAll()
                .OrderByDescending(emote => emote.Uses)
                .Take(number)
                .Select(emote => new EmoteQueryResult
                {
                    Id = emote.Id,
                    Name = emote.Name,
                    Uses = emote.Uses
                })
                .ToList();
        }
        public List<EmoteQueryResult> GetTopUserAll(int number, ulong userId)
        {
            if (number <= 0) number = 10;
            if (number > 25) number = 25;

            var user = _database.GetCollection<User>(TABLE_USERS).FindOne(x => x.Id == userId);
            if (user == null) return null;

            var result = user.UsedEmotes
                .GroupBy(emote => emote.Id)
                .Select(emotes => new EmoteQueryResult
                {
                    Id = emotes.First().Id,
                    Name = emotes.First().Name,
                    Uses = emotes.Sum(emote => emote.Uses),
                    Animated = emotes.First().Animated
                })
                .OrderByDescending(emote => emote.Uses)
                .Take(number)
                .ToList();

            return result;
        }
        public List<EmoteQueryResult> GetTopChannelAll(int number, ulong channelId)
        {
            if (number <= 0) number = 10;
            if (number > 25) number = 25;

            var users = _database.GetCollection<User>(TABLE_USERS).FindAll();

            return users.SelectMany(x => x.UsedEmotes.Where(y => y.ChannelId == channelId))
                .GroupBy(x => x.Id)
                .Select(x => new EmoteQueryResult
                {
                    Id = x.First().Id,
                    Animated = x.First().Animated,
                    Name = x.First().Name,
                    Uses = x.Sum(y => y.Uses)
                })
                .Take(number)
                .ToList();
        }
        public List<EmoteQueryResult> GetTopUserChannelAll(int number, ulong userId, ulong channelId)
        {
            if (number <= 0) number = 10;
            if (number > 25) number = 25;

            var user = _database.GetCollection<User>(TABLE_USERS).FindOne(x => x.Id == userId);
            if (user == null) return null;

            var result = user.UsedEmotes
                .Where(emote => emote.ChannelId == channelId)
                .GroupBy(emote => emote.Id)
                .Select(emotes => new EmoteQueryResult
                {
                    Id = emotes.First().Id,
                    Name = emotes.First().Name,
                    Uses = emotes.Sum(emote => emote.Uses),
                    Animated = emotes.First().Animated
                })
                .OrderByDescending(emote => emote.Uses)
                .Take(number)
                .ToList();

            return result;
        }
        
        public List<EmoteQueryResult> GetTop(int number, bool animated)
        {
            if (number <= 0) number = 10;
            if (number > 25) number = 25;
            return _database.GetCollection<Models.Emote>(TABLE_EMOTES)
                .Find(emote => emote.Animated == animated)
                .OrderByDescending(emote => emote.Uses)
                .Take(number)
                .Select(emote => new EmoteQueryResult
                {
                    Id = emote.Id,
                    Name = emote.Name,
                    Uses = emote.Uses
                })
                .ToList();
        }
        public List<EmoteQueryResult> GetTop(int number, bool animated, ulong userId)
        {
            if (number <= 0) number = 10;
            if (number > 25) number = 25;

            var user = _database.GetCollection<User>(TABLE_USERS).FindOne(x => x.Id == userId);
            if (user == null) return null;

            var result = user.UsedEmotes
                .Where(emote => emote.Animated == animated)
                .GroupBy(emote => emote.Id)
                .Select(emotes => new EmoteQueryResult
                {
                    Id = emotes.First().Id,
                    Name = emotes.First().Name,
                    Uses = emotes.Sum(emote => emote.Uses),
                    Animated = emotes.First().Animated
                })
                .OrderByDescending(emote => emote.Uses)
                .Take(number)
                .ToList();

            return result;
        }
        public List<EmoteQueryResult> GetTop(int number, bool animated, ulong userId, ulong channelId)
        {
            if (number <= 0) number = 10;
            if (number > 25) number = 25;

            var user = _database.GetCollection<User>(TABLE_USERS).FindOne(x => x.Id == userId);
            if (user == null) return null;

            var result = user.UsedEmotes
                .Where(emote => emote.Animated == animated && emote.ChannelId == channelId)
                .GroupBy(emote => emote.Id)
                .Select(emotes => new EmoteQueryResult
                {
                    Id = emotes.First().Id,
                    Name = emotes.First().Name,
                    Uses = emotes.Sum(emote => emote.Uses),
                    Animated = emotes.First().Animated
                })
                .OrderByDescending(emote => emote.Uses)
                .Take(number)
                .ToList();

            return result;
        }

        public EmoteQueryResult GetEmoteData(Discord.Emote emote)
        {
            return _database.GetCollection<Models.Emote>(TABLE_EMOTES)
                .Find(x => x.Id == emote.Id)
                .Select(x => new EmoteQueryResult
                {
                    Id = x.Id,
                    Animated = x.Animated,
                    Name = x.Name,
                    Uses = x.Uses
                })
                .FirstOrDefault();
        }
        public EmoteQueryResult GetEmoteUserData(Discord.Emote emote, ulong userId)
        {
            var user = _database.GetCollection<User>(TABLE_USERS).FindOne(x => x.Id == userId);
            if (user == null) return null;

            return user.UsedEmotes
                .Where(x => x.Id == emote.Id)
                .GroupBy(x => x.Id)
                .Select(emotes => new EmoteQueryResult
                {
                    Id = emotes.First().Id,
                    Name = emotes.First().Name,
                    Uses = emotes.Sum(x => x.Uses),
                    Animated = emotes.First().Animated
                })
                .OrderByDescending(x => x.Uses)
                .FirstOrDefault();
        }
        public EmoteQueryResult GetEmoteChannelData(Discord.Emote emote, ulong channelId)
        {
            var users = _database.GetCollection<User>(TABLE_USERS).FindAll();

            return users.SelectMany(x => x.UsedEmotes.Where(y => y.Id == emote.Id && y.ChannelId == channelId))
                .GroupBy(x => x.Id)
                .Select(x => new EmoteQueryResult
                {
                    Id = x.First().Id,
                    Animated = x.First().Animated,
                    Name = x.First().Name,
                    Uses = x.Sum(y => y.Uses)
                })
                .FirstOrDefault();
        }

        public DateTimeOffset LastChannelUpdate(ulong channelId)
        {
            var channelUpdateCollection = _database.GetCollection<ChannelUpdate>(TABLE_CHANNEL_UPDATES);
            var channelUpdate = channelUpdateCollection.FindOne(x => x.ChannelId == channelId);

            if (channelUpdate == null)
            {
                channelUpdate = new ChannelUpdate
                {
                    ChannelId = channelId,
                    LastUpdate = DateTime.UnixEpoch
                };

                channelUpdateCollection.Insert(channelUpdate);
            }

            return channelUpdate.LastUpdate;
        }

        public void UpdateLastChannelUpdate(ulong channelId, DateTimeOffset offset)
        {
            var channelUpdateCollection = _database.GetCollection<ChannelUpdate>(TABLE_CHANNEL_UPDATES);
            var channelUpdate = channelUpdateCollection.FindOne(x => x.ChannelId == channelId);
            channelUpdate.LastUpdate = offset.UtcDateTime;
            channelUpdateCollection.Update(channelUpdate);
        }

        public int EmoteCount()
        {
            return _database.GetCollection<Models.Emote>(TABLE_EMOTES).FindAll().Count();
        }
    }
}
