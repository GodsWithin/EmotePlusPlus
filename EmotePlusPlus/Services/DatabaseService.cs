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
        public bool CanAcceptNewUpdates { get; set; } = false;
        public ulong[] ChannelIds { get; set; } = {
            138194444645040128, //general
            168088911812362240, //fancy-you
            282688771508404225, //lane2
            198809450834952193, // games
            569901491733921793, //twicelights
            138195009580171264, //tzuyu
            138195029607841792, //nayeon
            138194941263216640, //jihyo
            138194483492683777, //dahyun
            138194636593168384, //chaeyoung
            138194610101944320, //jeongyeon
            138194569698344960, //momo
            138194552837111808, //mina
            138195097484263424, //sana
            159936670026825728  //botney
        };

        private readonly static string TABLE_EMOTE_USES = "emote_uses";
        private readonly static string TABLE_CHANNEL_UPDATES = "channel_updates";

        private readonly DiscordSocketClient _discord;
        private readonly LiteDatabase _database;
        private SocketGuild twiceCord;

        public DatabaseService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _database = services.GetRequiredService<LiteDatabase>();

            _discord.Ready += OnReady;
        }

        private async Task OnReady()
        {
            twiceCord = _discord.Guilds.FirstOrDefault(x => x.Id == 138194444645040128);

            CanAcceptNewUpdates = false;

            var channels = twiceCord.Channels.Where(x => x.GetType() == typeof(SocketTextChannel) && ChannelIds.Contains(x.Id));

            foreach (var channel in channels)
            {
                int counter = 0;
                Console.WriteLine(channel.Name);
                var lastUpdate = LastChannelUpdate(channel.Id);
                var firstMessage = (await (channel as ISocketMessageChannel).GetMessagesAsync(1).FlattenAsync()).First();

                try
                {
                    if (firstMessage.Tags.Any(x => x.Type == TagType.Emoji) && firstMessage.Timestamp > lastUpdate)
                        Update(firstMessage.Tags, firstMessage.Author.Id, firstMessage.Channel.Id, firstMessage.Timestamp, false, DateTimeOffset.UtcNow);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                var lastMessage = firstMessage;
                bool done = firstMessage.Timestamp <= lastUpdate;

                while (!done)
                {
                    Console.Write($"{string.Format("{0:n0}", counter)}, ");
                    var messages = await (channel as ISocketMessageChannel).GetMessagesAsync(lastMessage, Direction.Before, 100, CacheMode.AllowDownload).FlattenAsync();
                    foreach (var message in messages)
                    {
                        try
                        {
                            if (message.Tags.Any(x => x.Type == TagType.Emoji) && message.Timestamp > lastUpdate)
                                Update(message.Tags, message.Author.Id, message.Channel.Id, message.Timestamp, false, DateTimeOffset.UtcNow);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
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
                Console.WriteLine();
                UpdateLastChannelUpdate(channel.Id, firstMessage.Timestamp);
            }

            // CanAcceptNewUpdates = true;

            _discord.Ready -= OnReady;
        }

        public void Update(IReadOnlyCollection<ITag> tags, ulong userId, ulong channelId, DateTimeOffset messageTime, bool updateChannelTimestamp, DateTimeOffset offset)
        {
            var emojis = tags.Where(x => x.Type == TagType.Emoji).Select(x => x.Value as Emote).ToList();

            if (emojis.Any())
            {
                var emoteUses = _database.GetCollection<EmoteUse>(TABLE_EMOTE_USES);

                // Check if emoji is already present in the database, if not
                // add it.
                foreach (var emoji in emojis)
                {
                    if (twiceCord.Emotes.Any(emote => emote.Id == emoji.Id))
                    {
                        // Get the emote from the database whose data has to be updated            
                        EmoteUse emoteUse = new EmoteUse
                        {
                            EmoteId = emoji.Id,
                            EmoteName = emoji.Name,
                            Animated = emoji.Animated,
                            ChannelId = channelId,
                            UserId = userId,
                            Timestamp = messageTime.UtcDateTime
                        };

                        emoteUses.Insert(emoteUse);
                    }
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

        public List<EmoteQueryResult> GetTopAll()
        {
            return GetComplement(_database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .FindAll()
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(x => 1)
                })
                .OrderByDescending(result => result.Uses)
                .ToList());
        }
        public List<EmoteQueryResult> GetTopPeriodAll(DateTime period)
        {
            return GetComplement(_database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .FindAll()
                .Where(emoteUse => emoteUse.Timestamp.Year == period.Year && emoteUse.Timestamp.Month == period.Month)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(x => 1)
                })
                .OrderByDescending(result => result.Uses)
                .ToList());
        }
        public List<EmoteQueryResult> GetTopUserAll(ulong userId)
        {
            return GetComplement(_database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
               .Find(emoteUse => emoteUse.UserId == userId)
               .GroupBy(emoteUse => emoteUse.EmoteId)
               .Select(emoteUses => new EmoteQueryResult
               {
                   Id = emoteUses.First().EmoteId,
                   Animated = emoteUses.First().Animated,
                   Name = emoteUses.First().EmoteName,
                   Uses = emoteUses.Sum(x => 1)
               })
               .OrderByDescending(result => result.Uses)
               .ToList());
        }
        public List<EmoteQueryResult> GetTopChannelAll(ulong channelId)
        {
            return GetComplement(_database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
               .Find(emoteUse => emoteUse.ChannelId == channelId)
               .GroupBy(emoteUse => emoteUse.EmoteId)
               .Select(emoteUses => new EmoteQueryResult
               {
                   Id = emoteUses.First().EmoteId,
                   Animated = emoteUses.First().Animated,
                   Name = emoteUses.First().EmoteName,
                   Uses = emoteUses.Sum(x => 1)
               })
               .OrderByDescending(result => result.Uses)
               .ToList());
        }
        public List<EmoteQueryResult> GetTopChannelPeriodAll(DateTime period, ulong channelId)
        {
            return GetComplement(_database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
               .Find(emoteUse => emoteUse.ChannelId == channelId)
               .Where(emoteUse => emoteUse.Timestamp.Year == period.Year && emoteUse.Timestamp.Month == period.Month)
               .GroupBy(emoteUse => emoteUse.EmoteId)
               .Select(emoteUses => new EmoteQueryResult
               {
                   Id = emoteUses.First().EmoteId,
                   Animated = emoteUses.First().Animated,
                   Name = emoteUses.First().EmoteName,
                   Uses = emoteUses.Sum(x => 1)
               })
               .OrderByDescending(result => result.Uses)
               .ToList());
        }
        public List<EmoteQueryResult> GetTopUserChannelAll(ulong userId, ulong channelId)
        {
            return GetComplement(_database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
               .Find(emoteUse => emoteUse.ChannelId == channelId && emoteUse.UserId == userId)
               .GroupBy(emoteUse => emoteUse.EmoteId)
               .Select(emoteUses => new EmoteQueryResult
               {
                   Id = emoteUses.First().EmoteId,
                   Animated = emoteUses.First().Animated,
                   Name = emoteUses.First().EmoteName,
                   Uses = emoteUses.Sum(x => 1)
               })
               .OrderByDescending(result => result.Uses)
               .ToList());
        }

        public List<EmoteQueryResult> GetTop(bool animated)
        {
            return GetComplement(_database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.Animated == animated)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(x => 1)
                })
                .OrderByDescending(result => result.Uses)
                .ToList());
        }
        public List<EmoteQueryResult> GetTopPeriod(bool animated, DateTime period)
        {
            return GetComplement(_database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.Animated == animated)
                .Where(emoteUse => emoteUse.Timestamp.Year == period.Year && emoteUse.Timestamp.Month == period.Month)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(x => 1)
                })
                .OrderByDescending(result => result.Uses)
                .ToList());
        }
        public List<EmoteQueryResult> GetTopUser(bool animated, ulong userId)
        {
            return GetComplement(_database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.Animated == animated && emoteUse.UserId == userId)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(x => 1)
                })
                .OrderByDescending(result => result.Uses)
                .ToList());
        }
        public List<EmoteQueryResult> GetTopUserChannel(bool animated, ulong userId, ulong channelId)
        {
            return GetComplement(_database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.Animated == animated && emoteUse.UserId == userId && emoteUse.ChannelId == channelId)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(x => 1)
                })
                .OrderByDescending(result => result.Uses)
                .ToList());
        }

        public EmoteQueryResult GetEmoteData(Emote emote)
        {
            return _database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.EmoteId == emote.Id)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(y => 1)
                })
                .FirstOrDefault() ?? new EmoteQueryResult
                {
                    Animated = emote.Animated,
                    Id = emote.Id,
                    Name = emote.Name,
                    Uses = 0
                };
        }
        public EmoteQueryResult GetEmoteUserData(Emote emote, ulong userId)
        {
            return _database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.EmoteId == emote.Id && emoteUse.UserId == userId)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(y => 1)
                })
                .FirstOrDefault() ?? new EmoteQueryResult
                {
                    Animated = emote.Animated,
                    Id = emote.Id,
                    Name = emote.Name,
                    Uses = 0
                };
        }
        public EmoteQueryResult GetEmoteChannelData(Emote emote, ulong channelId)
        {
            return _database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.EmoteId == emote.Id && emoteUse.ChannelId == channelId)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(y => 1)
                })
                .FirstOrDefault() ?? new EmoteQueryResult
                {
                    Animated = emote.Animated,
                    Id = emote.Id,
                    Name = emote.Name,
                    Uses = 0
                };
        }
        public EmoteQueryResult GetEmoteUserChannelData(Emote emote, ulong userId, ulong channelId)
        {
            return _database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.EmoteId == emote.Id && emoteUse.UserId == userId && emoteUse.ChannelId == channelId)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(y => 1)
                })
                .FirstOrDefault() ?? new EmoteQueryResult
                {
                    Animated = emote.Animated,
                    Id = emote.Id,
                    Name = emote.Name,
                    Uses = 0
                };
        }
        public EmoteQueryResult GetEmotePeriodData(Emote emote, DateTime period)
        {
            return _database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.EmoteId == emote.Id)
                .Where(emoteUse => emoteUse.Timestamp.Year == period.Year && emoteUse.Timestamp.Month == period.Month)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(y => 1)
                })
                .FirstOrDefault() ?? new EmoteQueryResult
                {
                    Animated = emote.Animated,
                    Id = emote.Id,
                    Name = emote.Name,
                    Uses = 0
                };
        }
        public EmoteQueryResult GetEmotePeriodUserData(Emote emote, ulong userId, DateTime period)
        {
            return _database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.EmoteId == emote.Id && emoteUse.UserId == userId)
                .Where(emoteUse => emoteUse.Timestamp.Year == period.Year && emoteUse.Timestamp.Month == period.Month)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(y => 1)
                })
                .FirstOrDefault() ?? new EmoteQueryResult
                {
                    Animated = emote.Animated,
                    Id = emote.Id,
                    Name = emote.Name,
                    Uses = 0
                };
        }
        public EmoteQueryResult GetEmotePeriodChannelData(Emote emote, ulong channelId, DateTime period)
        {
            return _database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.EmoteId == emote.Id && emoteUse.ChannelId == channelId)
                .Where(emoteUse => emoteUse.Timestamp.Year == period.Year && emoteUse.Timestamp.Month == period.Month)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(y => 1)
                })
                .FirstOrDefault() ?? new EmoteQueryResult
                {
                    Animated = emote.Animated,
                    Id = emote.Id,
                    Name = emote.Name,
                    Uses = 0
                };
        }
        public EmoteQueryResult GetEmotePeriodUserChannelData(Emote emote, ulong userId, ulong channelId, DateTime period)
        {
            return _database.GetCollection<EmoteUse>(TABLE_EMOTE_USES)
                .Find(emoteUse => emoteUse.EmoteId == emote.Id && emoteUse.UserId == userId && emoteUse.ChannelId == channelId)
                .Where(emoteUse => emoteUse.Timestamp.Year == period.Year && emoteUse.Timestamp.Month == period.Month)
                .GroupBy(emoteUse => emoteUse.EmoteId)
                .Select(emoteUses => new EmoteQueryResult
                {
                    Id = emoteUses.First().EmoteId,
                    Animated = emoteUses.First().Animated,
                    Name = emoteUses.First().EmoteName,
                    Uses = emoteUses.Sum(y => 1)
                })
                .FirstOrDefault() ?? new EmoteQueryResult
                {
                    Animated = emote.Animated,
                    Id = emote.Id,
                    Name = emote.Name,
                    Uses = 0
                };
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

        private List<EmoteQueryResult> GetComplement(List<EmoteQueryResult> list)
        {
            List<EmoteQueryResult> unusedEmotes = null;
            try
            {
                unusedEmotes = twiceCord.Emotes
                   .Where(serverEmote => !list.Any(emote => emote.Id == serverEmote.Id))
                   .Select(serverEmote => new EmoteQueryResult
                   {
                       Animated = serverEmote.Animated,
                       Id = serverEmote.Id,
                       Name = serverEmote.Name,
                       Uses = 0
                   })
                   .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            list.AddRange(unusedEmotes);
            return list;
        }
    }
}
