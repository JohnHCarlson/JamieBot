namespace Jamie;

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class DiscordClientHost : IHostedService {

    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;

    public DiscordClientHost(DiscordSocketClient client, InteractionService service, IServiceProvider serviceProvider) {

        _client = client;
        _interactionService = service;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        _client.InteractionCreated += InteractionCreated;
        _client.Ready += ClientReady;

        await _client
            .LoginAsync(TokenType.Bot, getToken())
            .ConfigureAwait(false);

        await _client
            .StartAsync()
            .ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        _client.InteractionCreated -= InteractionCreated;
        _client.Ready -= ClientReady;

        await _client
            .StopAsync()
            .ConfigureAwait(false);
    }

    private Task InteractionCreated(SocketInteraction interaction) {
        var interactionContext = new SocketInteractionContext(_client, interaction);
        return _interactionService!.ExecuteCommandAsync(interactionContext, _serviceProvider);
    }

    private async Task ClientReady() {
        await _interactionService
            .AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider)
            .ConfigureAwait(false);

        //REGISTER COMMANDS HERE
        await _interactionService
            .RegisterCommandsGloballyAsync(true)
            .ConfigureAwait(false);
    }

    private string getToken() {
#if DEBUG
        return File.ReadAllText("..\\..\\..\\token.txt");
#else
        return File.ReadAllText("data/token.txt");
#endif
    }
}
