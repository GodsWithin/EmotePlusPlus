using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EmotePlusPlus.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotePlusPlus.Modules
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        public DatabaseService DatabaseService { get; set; }

        [Command("test")]
        public async Task Test()
        {
            await ReplyAsync(DatabaseService.EmoteCount() + "");
        }

        /*
         * 1) Top, global for the server --
         * 2) Top of a user, global for the server --
         * 3) Top of a user, for a specific channel --
         * 4) ALL OF THE ABOVE WITH ANIMATED FLAG --
         */

        // 1 both
        [Command("top")]
        public async Task TopAll(int number = 10)
        {
            var emotes = DatabaseService.GetTopAll(number);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 1 normal
        [Command("top normal")]
        public async Task TopNormal(int number = 10)
        {
            var emotes = DatabaseService.GetTop(number, false);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 1 animated
        [Command("top gif")]
        public async Task TopGif(int number = 10)
        {
            var emotes = DatabaseService.GetTop(number, true);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 2 both
        [Command("top")]
        public async Task TopUser(int number, IGuildUser user)
        {
            var emotes = DatabaseService.GetTopAll(number, user.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 2 both
        [Command("top")]
        public async Task TopUserAll(IGuildUser user)
        {
            var emotes = DatabaseService.GetTopAll(10, user.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 2 normal
        [Command("top normal")]
        public async Task TopUserNormal(int number, IGuildUser user)
        {
            var emotes = DatabaseService.GetTop(number, false, user.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 2 normal
        [Command("top normal")]
        public async Task TopUserNormal(IGuildUser user)
        {
            var emotes = DatabaseService.GetTop(10, false, user.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 2 animated
        [Command("top gif")]
        public async Task TopUserGif(int number, IGuildUser user)
        {
            var emotes = DatabaseService.GetTop(number, true, user.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 2 animated
        [Command("top gif")]
        public async Task TopUserGif(IGuildUser user)
        {
            var emotes = DatabaseService.GetTop(10, true, user.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 3 both
        [Command("top")]
        public async Task TopUserChannel(int number, IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTopAll(number, user.Id, channel.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 3 both
        [Command("top")]
        public async Task TopUserChannel(IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTopAll(10, user.Id, channel.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 3 normal
        [Command("top normal")]
        public async Task TopUserChannelNormal(int number, IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTop(number, false, user.Id, channel.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 3 normal
        [Command("top normal")]
        public async Task TopUserChannelNormal(IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTop(10, false, user.Id, channel.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 3 animated
        [Command("top gif")]
        public async Task TopUserChannelGif(int number, IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTop(number, true, user.Id, channel.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }

        // 3 animated
        [Command("top gif")]
        public async Task TopUserChannelGif(IGuildUser user, SocketTextChannel channel)
        {
            var emotes = DatabaseService.GetTop(10, true, user.Id, channel.Id);
            if (emotes.Any())
            {
                await ReplyAsync(string.Join("\n", emotes));
            }
        }
    }
}
