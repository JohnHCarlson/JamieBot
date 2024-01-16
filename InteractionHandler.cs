using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JamieBot {
    public class InteractionHandler {

        private readonly DiscordSocketClient _client;
        private readonly InteractionService _handler;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider service, IConfiguration config) {
            _client = client;
            _handler = handler;
            _services = service;
            _configuration = config;
        }

        public async Task InitalizeAsync() {

            _client.Ready += ReadyAsync;
            _client.Log += LogAsync;

            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // Also process the result of the command execution.
            _handler.InteractionExecuted += HandleInteractionExecute;
        }

        private async Task LogAsync(LogMessage log) {
            Console.WriteLine(log);
        }

        private async Task ReadyAsync() {

            var guild = _client.GetGuild(1094410629487546410);
            await guild.DeleteApplicationCommandsAsync();
            IReadOnlyCollection <SocketApplicationCommand> cmds = await _client.GetGlobalApplicationCommandsAsync();
            foreach (SocketApplicationCommand cmd in cmds){
                await cmd.DeleteAsync();
            }

            await _handler.RegisterCommandsGloballyAsync();
        }

        private async Task HandleInteraction(SocketInteraction interaction) {

            try {
                //Creates execution context
                var context = new SocketInteractionContext(_client, interaction);
                
                //Executes command
                var result = await _handler.ExecuteCommandAsync(context, _services);


                if (!result.IsSuccess) {
                    switch(result.Error) {
                        case InteractionCommandError.UnmetPrecondition:
                            Console.WriteLine(result.ToString());
                            break;
                    }
                }
            } catch {
                if (interaction.Type is InteractionType.ApplicationCommand) {
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }
            }

        }

        private async Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result) {
            if(!result.IsSuccess) {
                switch (result.Error) {
                    case InteractionCommandError.UnmetPrecondition:
                        Console.WriteLine(result.ToString());
                        break;
                }
            }
        }
    }
}
