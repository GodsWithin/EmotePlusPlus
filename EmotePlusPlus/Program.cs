using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EmotePlusPlus.Services;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EmotePlusPlus
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            // You should dispose a service provider created using ASP.NET
            // when you are finished using it, at the end of your app's lifetime.
            // If you use another dependency injection framework, you should inspect
            // its documentation for the best way to do this.
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                var config = services.GetRequiredService<IConfigurationRoot>();

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                client.Log += OnLog;

                // Tokens should be considered secret data and never hard-coded.
                // We can read from the config to avoid hardcoding.
                await client.LoginAsync(TokenType.Bot, config["token"]);
                await client.StartAsync();

                await Task.Delay(-1);
            }
        }

        private Task OnLog(LogMessage arg)
        {
            Console.WriteLine(arg.Message);
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json").Build();

            return new ServiceCollection()
                // Base
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 100
                }))
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Config
                .AddSingleton(config)
                // Database
                .AddSingleton(new LiteDatabase(new ConnectionString() { Filename = "bot.db", UtcDate = true }))
                .BuildServiceProvider();
        }
    }
}
