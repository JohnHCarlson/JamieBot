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
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Runtime.InteropServices;



public class Program {

    public static async Task Main(string[] args) {

        var builder = new HostApplicationBuilder(args);

        //Discord.net
        builder.Services.AddSingleton<DiscordSocketClient>();
        builder.Services.AddSingleton<InteractionService>();
        builder.Services.AddHostedService<DiscordClientHost>();

        Console.WriteLine(Directory.GetCurrentDirectory());
#if DEBUG
        String passphrase = File.ReadAllText("..\\..\\..\\passphrase.txt");
#else
        String passphrase = File.ReadAllText("data/passphrase.txt");
#endif


        //LavaLink4Net
        builder.Services.AddLavalink();
        builder.Services.ConfigureLavalink(config => {
            config.BaseAddress = new Uri("http://localhost:2333");
            config.ReadyTimeout = TimeSpan.FromSeconds(10);
            config.Passphrase = passphrase;
        });
        builder.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace));
        
        builder.Services.Replace(ServiceDescriptor.Singleton<IHostLifetime, EmptyLifetime>());

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

file sealed class EmptyLifetime : IHostLifetime {
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task WaitForStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}