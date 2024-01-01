using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace Jamie {
    public class Program {

        private DiscordSocketClient _client;

        private string _token;

        public static Task Main(string[] args) => new Program().MainAsync();

        /// <summary>
        /// Program entry point. 
        /// Inits bot data, event handlers, prepares bot.
        /// </summary>
        public async Task MainAsync() {

            //Creates new client socket connection
            var config = new DiscordSocketConfig() { //TODO: update to specific intents instead of `GatewayIntents.All`
                GatewayIntents = GatewayIntents.All
            };
            _client = new DiscordSocketClient(config);

            //Reads token from source
            _token = File.ReadAllText("..\\..\\..\\token.txt");

            //Adds meta events
            _client.Log += Log;
            _client.Ready += Ready;

            //Adds command events

            //Starts bot
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            //Blocks to allow bot to stay online
            await Task.Delay(-1);
        }

        /// <summary>
        /// TODO: Implement actual logging
        /// </summary>
        private Task Log(LogMessage msg) {

            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Prepares functions for the bot, such as time and new commands (if needed).
        /// </summary>
        private Task Ready() {

            return Task.CompletedTask;
        }
    }
}
