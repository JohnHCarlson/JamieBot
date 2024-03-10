namespace Jamie;

using System.Threading;

using Discord.Interactions;
using Discord.WebSocket;
using Jamie;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

public class Program {
    public static async Task Main(string[] args) {

        var builder = new HostApplicationBuilder(args);

        //Discord.net
        builder.Services.AddSingleton<DiscordSocketClient>();
        builder.Services.AddSingleton<InteractionService>();
        builder.Services.AddHostedService<DiscordClientHost>();

#if DEBUG
        String passphrase = File.ReadAllText("..\\..\\..\\token.txt");
#else
        String passphrase = File.ReadAllText("../JamieData/token.txt");
#endif

        //LavaLink4Net
        builder.Services.AddLavalink();
        builder.Services.ConfigureLavalink(config => {
            config.ReadyTimeout = TimeSpan.FromSeconds(10);
            config.Passphrase = passphrase;
        });
        builder.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace));

        //Adding modules
        builder.Services.AddSingleton<MusicModule>();

        //Subscribe to specific module events
        var app = builder.Build();

        var musicModule = app.Services.GetRequiredService<MusicModule>();
        app.Services.GetRequiredService<IAudioService>().TrackStarted += musicModule.TrackStarted;
        app.Services.GetRequiredService<IAudioService>().TrackEnded += musicModule.TrackEnded;

        app.Run();
    }
}