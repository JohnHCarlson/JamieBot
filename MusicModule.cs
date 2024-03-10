namespace Jamie;

using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;

public class MusicModule : InteractionModuleBase<SocketInteractionContext> {

    private readonly IAudioService _audioService;
    private readonly DiscordSocketClient _client;

    private ulong statusMessageId; //ID for the current status message ("currently playing...")

    public MusicModule(DiscordSocketClient client, IAudioService audioService) {

        ArgumentNullException.ThrowIfNull(audioService);
        ArgumentNullException.ThrowIfNull(client);

        _audioService = audioService;
        _client = client;
    }

    /// <summary>
    /// Command to play and queue new tracks.
    /// Command plays a new track, or if a track is currently playing adds it to the queue.
    /// This command is *not* a component interaction.
    /// </summary>
    /// <param name="query">Search query or link to play.</param>
    [SlashCommand("play", description: "Plays or queues the requested track.", runMode: RunMode.Async)]
    public async Task Play(string query) {

        await DeferAsync(ephemeral: true).ConfigureAwait(false); //Defers response to show the bot is working on the command (accounts for delay in youtube search/resolution)
        var player = await GetPlayerAsync(connectToVoice: true).ConfigureAwait(false); //Gets player, tells player to connect to voice if not already connected

        if (player == null) { //If player is null returns (error msg already sent in getplayer)
            return;
        }


        var track = await _audioService.Tracks //TODO search from other services (youtube music, shorts, spotify, soundcloud)
            .LoadTrackAsync(query, searchMode: Lavalink4NET.Rest.Entities.Tracks.TrackSearchMode.YouTube)
            .ConfigureAwait(false);

        if (track == null) {
            await FollowupAsync($"No result found for \"{query}\"").ConfigureAwait(false);
            return;
        }

        if (player.Queue.IsEmpty && player.CurrentItem == null) { //If queue is empty and no current item, messages that track is playing immediately
            await FollowupAsync($"Playing {track.Title}", ephemeral: true).ConfigureAwait(false);
        } else { //Otherwise, messagse that track is queued
            await FollowupAsync($"Adding {track.Title} to the queue", ephemeral: true).ConfigureAwait(false);
        }
        await player.PlayAsync(track).ConfigureAwait(false);
    }

    /// <summary>
    /// Paueses or plays the current track.
    /// Command is both a slash command and a component interaction on the track status message.
    /// </summary>
    [SlashCommand("pause", description: "Plays or resumes the player.", runMode: RunMode.Async)]
    [ComponentInteraction("playpause")]
    public async Task PausePlayComponent() {

        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        var player = await GetPlayerAsync(connectToVoice: false);

        if (player is null) {
            return;
        }

        if (player.State is PlayerState.Paused) {
            await player.ResumeAsync().ConfigureAwait(false);
            await FollowupAsync("Resumed.", ephemeral: true).ConfigureAwait(false);
            return;
        } else if (player.State is not PlayerState.Paused) {
            await player.PauseAsync().ConfigureAwait(false);
            await FollowupAsync("Paused.", ephemeral: true).ConfigureAwait(false);
            return;
        }
    }

    /// <summary>
    /// Skips the current track.
    /// Command is both a slash command and a component interaction on the track status message.
    /// </summary>
    [SlashCommand("skip", description: "Skips the current song.", runMode: RunMode.Async)]
    [ComponentInteraction("skip")]
    public async Task SkipComponent() {

        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        var player = await GetPlayerAsync(connectToVoice: false);

        if (player is null) {
            return;
        }

        if (player.CurrentTrack is null) { //If there is nothing to skip
            await FollowupAsync("Nothing playing...").ConfigureAwait(false);
            return;
        }

        await player.SkipAsync().ConfigureAwait(false); //Skips current track

        var track = player.CurrentTrack; //Gets new current track

        if (track is not null) { //If track exists, notifies
            await FollowupAsync($"Skipped. Now playing: {track.Title}", ephemeral: true).ConfigureAwait(false);
        } else { //Otherwise, queue is empty, notifies
            await FollowupAsync("Skipped. Stopped playing because the queue is now empty.", ephemeral: true).ConfigureAwait(false);
        }
    }
    /// <summary>
    /// Returns to the prior track, requeues current track.
    /// Command is both a slash command and a component interaction on the track status message.
    /// </summary>
    [SlashCommand("replay", description: "Plays the last-played song in the history.", runMode: RunMode.Async)]
    [ComponentInteraction("replay")]
    public async Task ReturnComponent() {

        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        var player = await GetPlayerAsync(connectToVoice: false);

        if (player is null) {
            return;
        }

        var lastTrack = player.Queue.History.LastOrDefault(); //First prior track from queue history

        if (lastTrack is null) {
            return;
        }

        var currentItem = player.CurrentItem; //Currently playing track
        if (currentItem is not null) {
            await player.Queue.InsertAsync(0, currentItem); //Enqueues current track at beginning of queue
        }

        await player.Queue.InsertAsync(0, lastTrack); //Enqueues last track at beginning of queue
        await player.Queue.History.RemoveAtAsync(player.Queue.History.Count - 1); //Removes prior track from history (avoids repeated replaying of the same track)

        await player.SkipAsync(); //Skips current song

        if (currentItem is not null) {
            player.Queue.History.RemoveAtAsync(player.Queue.History.Count - 1); //Removes current (now previous) track from history (avoids repeated replaying of the same track)
        }
    }


    /// <summary>
    /// Instructs the bot to leave the voice channel.
    /// Command is both a slash command and a component interaction on the track status message.
    /// </summary>
    [SlashCommand("leave", description: "Removes the bot from the voice channel.", runMode: RunMode.Async)]
    [ComponentInteraction("leave")]
    public async Task LeaveComponent() {

        await DeferAsync(ephemeral: true).ConfigureAwait(false);
        var player = await GetPlayerAsync(connectToVoice: true).ConfigureAwait(false);

        await player.DisconnectAsync();
    }

    /// <summary>
    /// Generates and replies with the current queue.
    /// Command is both a slash command and a component interaction on the track status message.
    /// </summary>
    [SlashCommand("queue", description: "Shows the current queue.", runMode: RunMode.Async)]
    [ComponentInteraction("queue")]
    public async Task QueueComponent() {

        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        var player = await GetPlayerAsync(connectToVoice: true).ConfigureAwait(false);

        List<String> tracks = new List<String>();
        ITrackQueue queue = player.Queue;
        StringBuilder queueMessage = new StringBuilder();

        foreach (TrackQueueItem item in queue) {
            tracks.Add(item.Reference.Track.Title);
        }

        queueMessage.AppendLine("Current queue:");
        for (int i = 0; i < tracks.Count; i++) {
            queueMessage.AppendLine($"{i + 1}: {tracks[i]}");
        }

        String message = queueMessage.ToString();
        await FollowupAsync(message, ephemeral: true).ConfigureAwait(false);
    }

    /// <summary>
    /// Shuffles the current queue.
    /// Command is currently not a component interaction as 5 components is the max per line.
    /// </summary>
    [SlashCommand("shuffle", description: "Shuffles the current queue.", runMode: RunMode.Async)]
    public async Task ShuffleComponent() {
        await DeferAsync(ephemeral: true).ConfigureAwait(false);

        var player = await GetPlayerAsync(connectToVoice: true).ConfigureAwait(false);

        await FollowupAsync("Shuffling the queue", ephemeral: true).ConfigureAwait(false);
        await player.Queue.ShuffleAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Fires when a new track starts.
    /// Sends a public status message with interactions showing the current track.
    /// </summary>
    internal async Task TrackStarted(object sender, TrackStartedEventArgs args) {

        var track = args.Track;
        var player = args.Player;
        var builder = GetControls();

        var channel = await _client.GetChannelAsync(1200875387824119879).ConfigureAwait(false) as IMessageChannel;

        var embed = new EmbedBuilder() {
            Title = $"Currently playing: {track.Title}",
            Timestamp = DateTime.Now,
            Url = track.Uri.ToString(),
            ImageUrl = track.ArtworkUri.ToString()
        };

        IMessage message = await channel.SendMessageAsync(embed: embed.Build(), components: builder).ConfigureAwait(false);
        statusMessageId = message.Id;
    }

    /// <summary>
    /// Fires when a track ends.
    /// Deletes the current public status message in preperation for the next track.
    /// </summary>
    internal async Task TrackEnded(object sender, TrackEndedEventArgs args) {

        var channel = await _client.GetChannelAsync(1200875387824119879).ConfigureAwait(false) as IMessageChannel;
        await channel.DeleteMessageAsync(statusMessageId).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the interaction component for the status message.
    /// </summary>
    private MessageComponent GetControls() {

        Emote.TryParse("<:pause:1207535041211797504>", out var pause);
        Emote.TryParse("<:replay:1207535060215930921>", out var replay);
        Emote.TryParse("<:skip:1207535070882172928>", out var skip);
        Emote.TryParse("<:leave:1207535024891756544>", out var leave);
        Emote.TryParse("<:queue:1207535050892247050>", out var queue);
        //Emote.TryParse("<:shuffle:1207535079228841995>", out var shuffle);

        var builder = new ComponentBuilder()
            .WithButton(customId: "playpause", emote: pause, style: ButtonStyle.Secondary, row: 0)
            .WithButton(customId: "replay", emote: replay, style: ButtonStyle.Secondary, row: 0)
            .WithButton(customId: "skip", emote: skip, style: ButtonStyle.Secondary, row: 0)
            .WithButton(customId: "leave", emote: leave, style: ButtonStyle.Secondary, row: 0)
            .WithButton(customId: "queue", emote: queue, style: ButtonStyle.Secondary, row: 0)
            //.WithButton(customId: "shuffle", emote: shuffle, style: ButtonStyle.Secondary, row: 0)
            .Build();

        return builder;
    }

    /// <summary>
    /// Used to get the lavalink player and join it to the voice channel, if desired.
    /// </summary>
    /// <param name="connectToVoice">If true, connects player to voice channel of sender.</param>
    private async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(bool connectToVoice = true) {

        var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: connectToVoice ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);
        var result = await _audioService.Players
            .RetrieveAsync(Context, playerFactory: PlayerFactory.Queued, retrieveOptions)
            .ConfigureAwait(false);

        if (!result.IsSuccess) {
            var errorMessage = result.Status switch {
                PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
                PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
                _ => "Unknown error.",
            };

            await FollowupAsync(errorMessage).ConfigureAwait(false);
            return null;
        }
        return result.Player;
    }
}

