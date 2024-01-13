
using Discord.WebSocket;

using Lavalink4NET;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Discord.Interactions;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Vote;
using Lavalink4NET.Rest.Entities.Tracks;


namespace JamieBot {

    class InsultData {
        public List<String> insults { get; set; }
        public List<String> other { get; set; }
    }

    internal class Commands {

        DiscordSocketClient _client;
        IAudioService _audioService;

        public Commands(DiscordSocketClient client, IAudioService service) {
            this._client = client;
            this._audioService = service;
        }

        public async Task SlashCommandHandler(SocketSlashCommand command) {
            var commandName = command.Data.Name;

            switch(commandName) {
                case "bogos-binted":
                    await BogosBinted(command);
                    break;
                case "photos-printed":
                    await PhotosPrinted(command); 
                    break;
                case "insult":
                    await Insult(command); 
                    break;
            }
        }

        private async Task BogosBinted(SocketSlashCommand command) {
            await command.RespondAsync("test response");
        }

        private async Task PhotosPrinted(SocketSlashCommand command) {
            
            await command.RespondAsync("bogos binted");
        }

        private async Task Insult(SocketSlashCommand command) {

            #if DEBUG
            String insult = LoadInsultsFromJson("..\\..\\..\\data.json");
            #else
            String insult = LoadInsultsFromJson("..\\JamieData\\data.json");
            #endif
            SocketGuildUser user = (SocketGuildUser)command.Data.Options.First().Value;
            String updatedInsult = insult.Replace("%n", $"<@{user.Id}>");

            await command.RespondAsync(updatedInsult);
        }

        private String LoadInsultsFromJson(string path) {
            try {
                string jsonString = File.ReadAllText(path); //TODO stop reading file every time insult bot is run
                InsultData jsonData = JsonConvert.DeserializeObject<InsultData>(jsonString);
                List<String> insults = jsonData.insults;

                Random random= new Random();
                int index = random.Next(0, insults.Count);
                return insults[index];

            }
            catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            return "ERROR AHH HOLY SHIT HELP ME";
        }

        private async ValueTask<VoteLavalinkPlayer?> GetPlayerAsync(bool connectToVoiceChannel = true) {
            
            
            var retrieveOptions = new PlayerRetrieveOptions(
                ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

            var result = await _audioService.Players
                .RetrieveAsync(Context, playerFactory: PlayerFactory.Vote, retrieveOptions)
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
