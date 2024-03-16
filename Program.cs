namespace Jamie;

using System.Threading;

using Discord.Interactions;
using Discord.WebSocket;
using Jamie;
using Microsoft.Extensions.Http.Logging;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;

 

public class Program {

    public static async Task Main(string[] args) {

        var builder = new HostApplicationBuilder(args);

        //Discord.net
        builder.Services.AddSingleton<DiscordSocketClient>();
        builder.Services.AddSingleton<InteractionService>();
        builder.Services.AddHostedService<DiscordClientHost>();

        String passphrase = "youshallnotpass";

#if DEBUG //uses default password if debug
#else
            try {
            passphrase = File.ReadAllText("data/passphrase.txt");

        } 
        catch (Exception ex) {
            Console.WriteLine("Unable to find passphrase at \"data/passphrase.txt\"");
            Console.WriteLine(ex.Message);
        }
#endif


        //LavaLink4Net
        builder.Services.AddLavalink();
        builder.Services.ConfigureLavalink(config => {
            config.BaseAddress = new Uri("http://localhost:2333");
            config.ReadyTimeout = TimeSpan.FromSeconds(10);
            config.Passphrase = passphrase;
        });
        builder.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace));
        builder.Services.AddHttpClient(string.Empty).AddLogger<HttpLogger>();
        builder.Services.AddSingleton<HttpLogger>();


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

file sealed class HttpLogger : IHttpClientAsyncLogger {
    public void LogRequestFailed(object? context, HttpRequestMessage request, HttpResponseMessage? response, Exception exception, TimeSpan elapsed) {
    }

    public ValueTask LogRequestFailedAsync(object? context, HttpRequestMessage request, HttpResponseMessage? response, Exception exception, TimeSpan elapsed, CancellationToken cancellationToken = default) {
        return default;
    }

    public object? LogRequestStart(HttpRequestMessage request) {
        return null;
    }

    public async ValueTask<object?> LogRequestStartAsync(HttpRequestMessage request, CancellationToken cancellationToken = default) {
        if (request.Content is not null) {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);

            Console.WriteLine($$"""
                Sending HTTP/{{request.Version}} {{request.Method}} {{request.RequestUri}}
                {{string.Join("\n", request.Headers.Select(x => $"{x.Key}: {string.Join(';', x.Value)}"))}}

                {{requestBody}}
                """);
        }

        return null;
    }

    public void LogRequestStop(object? context, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed) {
    }

    public ValueTask LogRequestStopAsync(object? context, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed, CancellationToken cancellationToken = default) {
        return default;
    }
}