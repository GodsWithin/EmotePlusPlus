using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Discord;
using EmotePlusPlus.Models;
using System.Collections.Generic;

namespace EmotePlusPlus.Services
{
    public class DatabaseService
    {
        private readonly static string TABLE_EMOTES = "emotes";
        private readonly static string TABLE_EMOTE_USES = "emote_uses";

        private readonly DiscordSocketClient _discord;
        private readonly LiteDatabase _database;

        public DatabaseService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _database = services.GetRequiredService<LiteDatabase>();
        }

        public void Update(SocketMessage context)
        {
            var emojis = context.Tags.Where(x => x.Type == TagType.Emoji).Select(x => x.Value as Discord.Emote).ToList();

            if (emojis.Any())
            {
                var emotes = _database.GetCollection<Models.Emote>(TABLE_EMOTES);
                var emoteUses = _database.GetCollection<EmoteUse>(TABLE_EMOTE_USES);

                // Check if emoji is already present in the database, if not
                // add it.

                List<EmoteUse> uses = new List<EmoteUse>();

                foreach (var emoji in emojis)
                {
                    var emote = emotes.FindOne(e => e.Id == emoji.Id);

                    if (emote == null)
                    {
                        emote = new Models.Emote
                        {
                            Id = emoji.Id,
                            Name = emoji.Name,
                            CreatedAt = emoji.CreatedAt.DateTime,
                            Uses = 0,
                            Animated = emoji.Animated
                        };
                    }

                    emote.Uses++;
                    emotes.Upsert(emote);

                    EmoteUse emoteUse = new EmoteUse
                    {
                        ChannelId = context.Channel.Id,
                        Emote = emote,
                        UsedAt = DateTime.UtcNow,
                        UserId = context.Author.Id
                    };

                    uses.Add(emoteUse);
                }

                emoteUses.InsertBulk(uses);
            }
        }

        public List<Models.Emote> GetTop(int number)
        {
            if (number <= 0) number = 10;
            if (number > 25) number = 25;
            return _database.GetCollection<Models.Emote>(TABLE_EMOTES).FindAll().OrderByDescending(x => x.Uses).Take(number).ToList();
        }

        public List<Models.Emote> GetTopAnimated(int number, bool isAnimated)
        {
            if (number <= 0) number = 10;
            if (number > 25) number = 25;
            return _database.GetCollection<Models.Emote>(TABLE_EMOTES).FindAll().Where(x => x.Animated == isAnimated).OrderByDescending(x => x.Uses).Take(number).ToList();
        }

        public int EmoteCount()
        {
            return _database.GetCollection<Models.Emote>(TABLE_EMOTES).FindAll().Count();
        }
    }
}
