using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamieBot {
    internal class Handlers {

        private readonly DiscordSocketClient _client;

        public Handlers(DiscordSocketClient client) {
            _client = client;
        }

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
    }
}
