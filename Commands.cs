
using Discord.WebSocket;
using Newtonsoft.Json;


namespace JamieBot {

    class InsultData {
        public List<String> insults { get; set; }
        public List<String> other { get; set; }
    }

    internal class Commands {

        DiscordSocketClient _client;

        public Commands(DiscordSocketClient client) {
            this._client = client;
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
            await command.RespondAsync("\u1f47D");
        }

        private async Task PhotosPrinted(SocketSlashCommand command) {
            await command.RespondAsync("bogos binted");
        }

        private async Task Insult(SocketSlashCommand command) {

            String insult = LoadInsultsFromJson("..\\..\\..\\data.json");

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
    }
}
