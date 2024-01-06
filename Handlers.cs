using Discord;
using Discord.WebSocket;

namespace JamieBot {
    internal class Handlers {

        private readonly DiscordSocketClient _client;

        public Handlers(DiscordSocketClient client) {
            _client = client;
        }

        private bool nightMessage = false;
        private DateTime nightMessageTime;

        public async Task GuildMemberUpdatedHandler(Cacheable<SocketGuildUser, ulong> beforeCache, SocketGuildUser afterUser) {
            
            var beforeUser = await beforeCache.GetOrDownloadAsync(); // Downloads beforeuser from cache (if needed)
            var chnl = _client.GetChannel(1188565886425112697) as IMessageChannel; //bot test channel ID 

            SocketGuild guild = afterUser.Guild;
            var glonky = guild.GetRole(1094467468606578809);
            var notGlonky = guild.GetRole(1107826699359490120);

            if (!beforeUser.IsBot && !beforeUser.Nickname.Equals(afterUser.Nickname)) { //If the nickname has been updated

                if (!afterUser.Nickname.StartsWith("J", StringComparison.OrdinalIgnoreCase)) { //New nickname does not start with 'J'/'j'

                    await afterUser.RemoveRoleAsync(glonky);
                    await afterUser.AddRoleAsync(notGlonky);
                    
                    await chnl.SendMessageAsync($"<@{afterUser.Id}> The user formerly known as \"{beforeUser.Nickname}\" has chosen a new identiy: \"{afterUser.Nickname}\"" +
                        $" **and in doing so has broken the sacred rule of the server: their name no longer begins with the letter 'J'.** " +
                        $"Therefore they have been stripped of their *Glonky* role and now bear the title of *Not Glonky* for all eternity (or until <@&{1094413342489194537}> has mercy). " +
                        $"May they reflect on their heinous act and strive for a more alphabetically appropriate name in the future.");
                } 
            } 
        }

        public async void CheckRandomCondition(object state) {
            //TODO update to take message and time info from data.json
            DateTime nowEst = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            if(nowEst.Hour == 4 && nowEst.Minute == 0) { //Checking for daily random values at 4:00
                
                if(new Random().Next(1,11) == 1) { //Generating chance for random bot message between 11:00PM-1:30AM
                    
                    Random random = new Random();

                    int randomHours = random.Next(23, 26); //Gets a time between 11:00 and 1:30
                    int randomMinutes = (randomHours == 26) ? random.Next(0, 30) : random.Next(0, 60);
                    this.nightMessageTime = DateTime.Now.Date.AddHours(randomHours).AddMinutes(randomMinutes);
                    nightMessage = true;
                }
            }
            else if(nowEst.Hour == 3 && nowEst.Minute == 59) { //Resets all conditions at night 
                this.nightMessage = false;
            }

            if(this.nightMessage && nowEst == this.nightMessageTime) {
                var chnl = _client.GetChannel(1094701047898964078) as IMessageChannel; //general channel ID 
                await chnl.SendMessageAsync("Who up glonking they guy?"); //TODO: add other possible messages to send
            }
        }
    }
}
