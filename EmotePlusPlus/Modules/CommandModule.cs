using Discord;
using Discord.Commands;
using EmotePlusPlus.Services;
using System;
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
            await ReplyAsync(DatabaseService.EmoteCount()+"");
        }

        [Command("top")]
        [Summary("Display the top <number> emotes used in the server the command was invoked from.")]
        public async Task Top(int number, IGuildUser user, IGuildChannel channel, bool animated)
        {
            var emotes = DatabaseService.GetTop(number);
            await ReplyAsync(string.Join("\n", emotes));
        }

        [Command("ta")]
        [Summary("Display the top <number> animated emotes used in the server the command was invoked from.")]
        public async Task TopAnimated(int number = 10)
        {
            var emotes = DatabaseService.GetTopAnimated(number, true);
            await ReplyAsync(string.Join("\n", emotes));
        }

        [Command("tna")]
        [Summary("Display the top <number> non animated emotes used in the server the command was invoked from.")]
        public async Task TopNonAnimated(int number = 10)
        {
            var emotes = DatabaseService.GetTopAnimated(number, false);
            await ReplyAsync(string.Join("\n", emotes));
        }
    }
}
