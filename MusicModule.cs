using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lavalink4NET;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Players;
using Lavalink4NET.DiscordNet;

namespace JamieBot {
    public class MusicModule : InteractionModuleBase<SocketInteractionContext>{

        private DiscordSocketClient _client;
        private IAudioService _audioService;

        public MusicModule(DiscordSocketClient client, IAudioService service) {
            this._client = client;
            this._audioService= service;
        }

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
}
