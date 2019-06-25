using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmotePlusPlus.Services
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly DatabaseService _database;

        private readonly IServiceProvider _services;

        public CommandHandlingService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _commands = services.GetRequiredService<CommandService>();
            _database = services.GetRequiredService<DatabaseService>();
            _services = services;

            // Hook CommandExecuted to handle post-command-execution logic.
            _commands.CommandExecuted += CommandExecutedAsync;

            // Hook MessageReceived so we can process each message to see
            // if it qualifies as a command.
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            // Add additional initialization code here...
        }

        public async Task MessageReceivedAsync(SocketMessage msg)
        {
            // Ignore system messages and messages from bots.
            
            if (!(msg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var context = new SocketCommandContext(_discord, message);

            if (msg.Tags.Any() && _database.CanAcceptNewUpdates)
                _database.Update(msg.Tags, msg.Author.Id, msg.Channel.Id, true, msg.Timestamp);

            int argPos = 0;
            if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos)
                && !message.HasCharPrefix('+', ref argPos)) return;

            // Perform the execution of the command. In this method,
            // the command service will perform precondition and parsing check
            // then execute the command if one is matched.
            await _commands.ExecuteAsync(context, argPos, _services);
            // Note that normally a result will be returned by this format, but here
            // we will handle the result in CommandExecutedAsync.
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // Command is unspecified when there was a search failure (command not found); we don't care about these errors.
            if (!command.IsSpecified)
                return;

            // The command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;

            // The command failed, let's notify the user that something happened.

            if (result.Error == CommandError.Exception)
                await context.Channel.SendMessageAsync("Something broke!");
            else
            {
                await context.Message.AddReactionAsync(new Emoji("❌"));
            }
        }
    }
}
