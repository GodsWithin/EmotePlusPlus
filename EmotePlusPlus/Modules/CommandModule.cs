using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EmotePlusPlus.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EmotePlusPlus.Modules
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        public DatabaseService DatabaseService { get; set; }

        #region Server wide commands

        [Command("top")]
        public async Task TopAll(int number = 10)
        {
            var emotes = DatabaseService.GetTopAll(number);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
            else
            {
                await ReplyAsync("No emotes have been used on this server.");
            }
        }

        [Command("top normal")]
        public async Task TopNormal(int number = 10)
        {
            var emotes = DatabaseService.GetTop(number, false);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
            else
            {
                await ReplyAsync("No normal emotes have been used on this server.");
            }
        }

        [Command("top gif")]
        public async Task TopGif(int number = 10)
        {
            var emotes = DatabaseService.GetTop(number, true);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
            else
            {
                await ReplyAsync("No gif emotes have been used on this server.");
            }
        }

        #endregion

        #region User specific commands
        [Command("top")]
        public async Task TopUserAll(int number, IGuildUser user)
        {
            var emotes = DatabaseService.GetTopUserAll(number, user.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        [Command("top")]
        public async Task TopUserAll(IGuildUser user)
        {
            var emotes = DatabaseService.GetTopUserAll(10, user.Id);
            try
            {
                if (emotes.Any())
                {
                    await ReplyAsync(string.Join("\n", emotes));
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no emotes.");
            }
        }

        [Command("top normal")]
        public async Task TopUserNormal(int number, IGuildUser user)
        {
            var emotes = DatabaseService.GetTop(number, false, user.Id);
            try
            {
                if (emotes.Any())
                {
                    await ReplyAsync(string.Join("\n", emotes));
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no normal emotes.");
            }
        }

        [Command("top normal")]
        public async Task TopUserNormal(IGuildUser user)
        {
            var emotes = DatabaseService.GetTop(10, false, user.Id);
            try
            {
                if (emotes.Any())
                {
                    await ReplyAsync(string.Join("\n", emotes));
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no normal emotes.");
            }
        }

        [Command("top gif")]
        public async Task TopUserGif(int number, IGuildUser user)
        {
            var emotes = DatabaseService.GetTop(number, true, user.Id);
            try
            {
                if (emotes.Any())
                {
                    await ReplyAsync(string.Join("\n", emotes));
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no gif emotes.");
            }
        }

        [Command("top gif")]
        public async Task TopUserGif(IGuildUser user)
        {
            var emotes = DatabaseService.GetTop(10, true, user.Id);
            try
            {
                if (emotes.Any())
                {
                    await ReplyAsync(string.Join("\n", emotes));
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no gif emotes.");
            }
        }

        #endregion

        #region Channel specific commands

        [Command("top")]
        public async Task TopChannelAll(int number, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTopChannelAll(number, channel.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
            else
            {
                await ReplyAsync("There haven't been any emotes used in that channel.");
            }
        }

        [Command("top")]
        public async Task TopChannelAll(SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTopChannelAll(10, channel.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
            else
            {
                await ReplyAsync("There haven't been any emotes used in that channel.");
            }
        }

        #endregion

        #region User and channel specific commands

        [Command("top")]
        public async Task TopUserChannelAll(int number, IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTopUserChannelAll(number, user.Id, channel.Id);
            try
            {
                if (emotes.Any())
                {
                    await ReplyAsync(string.Join("\n", emotes));
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("There haven't been any emotes used in that channel.");
            }
        }

        [Command("top")]
        public async Task TopUserChannelAll(IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTopUserChannelAll(10, user.Id, channel.Id);
            try
            {
                if (emotes.Any())
                {
                    await ReplyAsync(string.Join("\n", emotes));
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no emotes in that channel.");
            }
        }

        [Command("top normal")]
        public async Task TopUserChannelNormal(int number, IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTop(number, false, user.Id, channel.Id);
            try
            {
                if (emotes.Any())
                {
                    await ReplyAsync(string.Join("\n", emotes));
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no normal emotes in that channel.");
            }
        }

        [Command("top normal")]
        public async Task TopUserChannelNormal(IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTop(10, false, user.Id, channel.Id);
            try
            {
                if (emotes.Any())
                {
                    await ReplyAsync(string.Join("\n", emotes));
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no normal emotes in that channel.");
            }
        }

        [Command("top gif")]
        public async Task TopUserChannelGif(int number, IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTop(number, true, user.Id, channel.Id);
            try
            {
                if (emotes.Any())
                {
                    await ReplyAsync(string.Join("\n", emotes));
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no gif emotes in that channel.");
            }
        }

        [Command("top gif")]
        public async Task TopUserChannelGif(IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTop(10, true, user.Id, channel.Id);
            try
            {
                if (emotes.Any())
                {
                    await ReplyAsync(string.Join("\n", emotes));
                }
            }
            catch (ArgumentNullException ex)
            {
                await ReplyAsync("That user has used no gif emotes in that channel.");
            }
        }

        #endregion

        #region Emote specific commands

        [Command("check")]
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

        [Command("check")]
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

        [Command("check")]
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

        #endregion

    }
}
