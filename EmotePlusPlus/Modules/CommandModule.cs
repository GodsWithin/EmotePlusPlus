using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EmotePlusPlus.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EmotePlusPlus.Modules
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        public DatabaseService DatabaseService { get; set; }

        #region Server wide commands

        [Command("top", RunMode = RunMode.Async)]
        public async Task TopAll()
        {
            var emotes = DatabaseService.GetTopAll();
            if (emotes.Any())
            {
                for (int i = 0; i < emotes.Count; i += 50)
                {
                    string result = string.Join("\n", emotes.Skip(i).Take(50));
                    await ReplyAsync(result);
                }
            }
            else
            {
                await ReplyAsync("No emotes have been used on this server.");
            }
        }

        [Command("top", RunMode = RunMode.Async)]
        public async Task TopAllPeriod([Remainder] string date)
        {
            bool dateParsed = DateTime.TryParseExact(date, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime period);
            if (dateParsed)
            {
                var emotes = DatabaseService.GetTopPeriodAll(period);
                if (emotes.Any())
                {
                    for (int i = 0; i < emotes.Count; i += 50)
                    {
                        string result = string.Join("\n", emotes.Skip(i).Take(50));
                        await ReplyAsync(result);
                    }
                }
                else
                {
                    await ReplyAsync("No emotes have been used on this server.");
                }
            }
        }

        [Command("topnormal", RunMode = RunMode.Async)]
        public async Task TopNormal()
        {
            var emotes = DatabaseService.GetTop(false);
            if (emotes.Any())
            {
                for (int i = 0; i < emotes.Count; i += 50)
                {
                    string result = string.Join("\n", emotes.Skip(i).Take(50));
                    await ReplyAsync(result);
                }
            }
            else
            {
                await ReplyAsync("No normal emotes have been used on this server.");
            }
        }

        [Command("topnormal", RunMode = RunMode.Async)]
        public async Task TopNormalPeriod([Remainder] string date)
        {
            bool dateParsed = DateTime.TryParseExact(date, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime period);

            if (dateParsed)
            {
                var emotes = DatabaseService.GetTopPeriod(false, period);
                if (emotes.Any())
                {
                    for (int i = 0; i < emotes.Count; i += 50)
                    {
                        string result = string.Join("\n", emotes.Skip(i).Take(50));
                        await ReplyAsync(result);
                    }
                }
                else
                {
                    await ReplyAsync("No normal emotes have been used on this server.");
                }
            }
        }

        [Command("topgif", RunMode = RunMode.Async)]
        public async Task TopGif()
        {
            var emotes = DatabaseService.GetTop(true);
            if (emotes.Any())
            {
                for (int i = 0; i < emotes.Count; i += 50)
                {
                    string result = string.Join("\n", emotes.Skip(i).Take(50));
                    await ReplyAsync(result);
                }
            }
            else
            {
                await ReplyAsync("No gif emotes have been used on this server.");
            }
        }

        [Command("topgif", RunMode = RunMode.Async)]
        public async Task TopGifPeriod([Remainder] string date)
        {
            bool dateParsed = DateTime.TryParseExact(date, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime period);

            if (dateParsed)
            {
                var emotes = DatabaseService.GetTopPeriod(true, period);
                if (emotes.Any())
                {
                    for (int i = 0; i < emotes.Count; i += 50)
                    {
                        string result = string.Join("\n", emotes.Skip(i).Take(50));
                        await ReplyAsync(result);
                    }
                }
                else
                {
                    await ReplyAsync("No normal emotes have been used on this server.");
                }
            }
        }

        #endregion

        #region User specific commands
        [Command("top", RunMode = RunMode.Async)]
        public async Task TopUserAll(IGuildUser user)
        {
            var emotes = DatabaseService.GetTopUserAll(user.Id);
            if (emotes.Any())
            {
                for (int i = 0; i < emotes.Count; i += 50)
                {
                    string result = string.Join("\n", emotes.Skip(i).Take(50));
                    await ReplyAsync(result);
                }
            }
        }

        [Command("topnormal", RunMode = RunMode.Async)]
        public async Task TopUserNormal(IGuildUser user)
        {
            var emotes = DatabaseService.GetTopUser(false, user.Id);
            try
            {
                if (emotes.Any())
                {
                    for (int i = 0; i < emotes.Count; i += 50)
                    {
                        string result = string.Join("\n", emotes.Skip(i).Take(50));
                        await ReplyAsync(result);
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no normal emotes.");
            }
        }

        [Command("topgif", RunMode = RunMode.Async)]
        public async Task TopUserGif(IGuildUser user)
        {
            var emotes = DatabaseService.GetTopUser(true, user.Id);
            try
            {
                if (emotes.Any())
                {
                    for (int i = 0; i < emotes.Count; i += 50)
                    {
                        string result = string.Join("\n", emotes.Skip(i).Take(50));
                        await ReplyAsync(result);
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no gif emotes.");
            }
        }

        #endregion

        #region Channel specific commands

        [Command("top", RunMode = RunMode.Async)]
        public async Task TopChannelAll(SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTopChannelAll(channel.Id);
            if (emotes.Any())
            {
                for (int i = 0; i < emotes.Count; i += 50)
                {
                    string result = string.Join("\n", emotes.Skip(i).Take(50));
                    await ReplyAsync(result);
                }
            }
            else
            {
                await ReplyAsync("There haven't been any emotes used in that channel.");
            }
        }

        [Command("top", RunMode = RunMode.Async)]
        public async Task TopChannelPeriodAll(SocketTextChannel channel, [Remainder] string date)
        {
            bool dateParsed = DateTime.TryParseExact(date, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime period);

            if (dateParsed)
            {
                var emotes = DatabaseService.GetTopChannelPeriodAll(period, channel.Id);
                if (emotes.Any())
                {
                    for (int i = 0; i < emotes.Count; i += 50)
                    {
                        string result = string.Join("\n", emotes.Skip(i).Take(50));
                        await ReplyAsync(result);
                    }
                }
                else
                {
                    await ReplyAsync("There haven't been any emotes used in that channel.");
                }
            }
        }

        #endregion

        #region User and channel specific commands

        [Command("top", RunMode = RunMode.Async)]
        public async Task TopUserChannelAll(IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTopUserChannelAll(user.Id, channel.Id);
            try
            {
                if (emotes.Any())
                {
                    for (int i = 0; i < emotes.Count; i += 50)
                    {
                        string result = string.Join("\n", emotes.Skip(i).Take(50));
                        await ReplyAsync(result);
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("There haven't been any emotes used in that channel.");
            }
        }

        [Command("topnormal", RunMode = RunMode.Async)]
        public async Task TopUserChannelNormal(IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTopUserChannel(false, user.Id, channel.Id);
            try
            {
                if (emotes.Any())
                {
                    for (int i = 0; i < emotes.Count; i += 50)
                    {
                        string result = string.Join("\n", emotes.Skip(i).Take(50));
                        await ReplyAsync(result);
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no normal emotes in that channel.");
            }
        }

        [Command("topgif", RunMode = RunMode.Async)]
        public async Task TopUserChannelGif(IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTopUserChannel(true, user.Id, channel.Id);
            try
            {
                if (emotes.Any())
                {
                    for (int i = 0; i < emotes.Count; i += 50)
                    {
                        string result = string.Join("\n", emotes.Skip(i).Take(50));
                        await ReplyAsync(result);
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no gif emotes in that channel.");
            }
        }

        #endregion

        #region Emote specific commands

        [Command("check", RunMode = RunMode.Async)]
        public async Task CheckEmote(string emoteString)
        {
            bool success = Emote.TryParse(emoteString, out Emote emote);
            if (success)
            {
                var result = DatabaseService.GetEmoteData(emote);
                if (result != null)
                {
                    await ReplyAsync(result.ToString());
                }
            }
        }

        [Command("check", RunMode = RunMode.Async)]
        public async Task CheckEmoteUser(string emoteString, IGuildUser user)
        {
            bool success = Emote.TryParse(emoteString, out Emote emote);
            if (success)
            {
                var result = DatabaseService.GetEmoteUserData(emote, user.Id);
                if (result != null)
                {
                    await ReplyAsync(result.ToString());
                }
            }
        }

        [Command("check", RunMode = RunMode.Async)]
        public async Task CheckEmoteChannel(string emoteString, SocketTextChannel channel)
        {
            bool success = Emote.TryParse(emoteString, out Emote emote);
            if (success)
            {
                var result = DatabaseService.GetEmoteChannelData(emote, channel.Id);
                if (result != null)
                {
                    await ReplyAsync(result.ToString());
                }
            }
        }

        [Command("check", RunMode = RunMode.Async)]
        public async Task CheckEmoteDate(string emoteString, [Remainder] string date)
        {
            bool success = Emote.TryParse(emoteString, out Emote emote);
            bool dateParsed = DateTime.TryParseExact(date, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime period);
            if (success && dateParsed)
            {
                var result = DatabaseService.GetEmotePeriodData(emote, period);
                if (result != null)
                {
                    await ReplyAsync(result.ToString());
                }
            }
        }

        [Command("check", RunMode = RunMode.Async)]
        public async Task CheckEmoteDateChannel(string emoteString, IGuildUser user, [Remainder] string date)
        {
            bool success = Emote.TryParse(emoteString, out Emote emote);
            bool dateParsed = DateTime.TryParseExact(date, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime period);
            if (success && dateParsed)
            {
                var result = DatabaseService.GetEmotePeriodUserData(emote, user.Id, period);
                if (result != null)
                {
                    await ReplyAsync(result.ToString());
                }
            }
        }

        [Command("check", RunMode = RunMode.Async)]
        public async Task CheckEmoteDateChannel(string emoteString, SocketTextChannel channel, [Remainder] string date)
        {
            bool success = Emote.TryParse(emoteString, out Emote emote);
            bool dateParsed = DateTime.TryParseExact(date, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime period);
            if (success && dateParsed)
            {
                var result = DatabaseService.GetEmotePeriodChannelData(emote, channel.Id, period);
                if (result != null)
                {
                    await ReplyAsync(result.ToString());
                }
            }
        }

        [Command("check", RunMode = RunMode.Async)]
        public async Task CheckEmoteDateUserChannel(string emoteString, IGuildUser user, SocketTextChannel channel)
        {
            bool success = Emote.TryParse(emoteString, out Emote emote);
            if (success)
            {
                var result = DatabaseService.GetEmoteUserChannelData(emote, user.Id, channel.Id);
                if (result != null)
                {
                    await ReplyAsync(result.ToString());
                }
            }
        }

        [Command("check", RunMode = RunMode.Async)]
        public async Task CheckEmoteDateUserChannel(string emoteString, IGuildUser user, SocketTextChannel channel, [Remainder] string date)
        {
            bool success = Emote.TryParse(emoteString, out Emote emote);
            bool dateParsed = DateTime.TryParseExact(date, "MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime period);
            if (success && dateParsed)
            {
                var result = DatabaseService.GetEmotePeriodUserChannelData(emote, user.Id, channel.Id, period);
                if (result != null)
                {
                    await ReplyAsync(result.ToString());
                }
            }
        }

        #endregion

    }
}
