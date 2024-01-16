using System;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Discord.Interactions;

using Lavalink4NET;
using Lavalink4NET.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lavalink4NET;
using Discord.Interactions;

namespace JamieBot {
    public class Program {

        private static IConfiguration _configuration;
        public static DiscordSocketClient? _client;
        public static IAudioService _audioService;
        private string? _token;
        private Timer _timer;

        private static IServiceProvider _services;

        Handlers handlers;
        Commands commands;

        public static Task Main(string[] args) => new Program().MainAsync(args);

        private static readonly DiscordSocketConfig _socketConfig = new() {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
            AlwaysDownloadUsers = true,
        };

        /// <summary>
        /// Program entry point. 
        /// Inits bot data, event handlers, prepares bot.
        /// </summary>
        public async Task MainAsync(string[] args) {

            _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

            _services = new ServiceCollection()
                .AddSingleton(_socketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<IConfiguration>(_configuration)
                .AddSingleton<InteractionService>()
                .AddSingleton<InteractionHandler>()
                .AddLavalink()
                .BuildServiceProvider();

            _client = _services.GetRequiredService<DiscordSocketClient>();
            _audioService = _services.GetRequiredService<IAudioService>();

            _client.Log += LogAsync;

            await _services.GetRequiredService<InteractionHandler>().InitalizeAsync();

            //Adds handlers
            //handlers = new Handlers(_client);
            //commands = new Commands(_client, _audioService);

            //Adds command events
            //_client.GuildMemberUpdated += handlers.GuildMemberUpdatedHandler;
            //_client.SlashCommandExecuted += commands.SlashCommandHandler;

            #if DEBUG
            _token = File.ReadAllText("..\\..\\..\\token.txt");
            #else
            _token = File.ReadAllText("..\\JamieData\token.txt");
            #endif

            //Starts bot
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            //Blocks to allow bot to stay online
            await Task.Delay(-1);
        }

        private static async Task LogAsync(LogMessage message) {
            Console.WriteLine(message.ToString());
        }
    }
}
