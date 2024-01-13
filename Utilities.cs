using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;

namespace JamieBot {
    internal class Utilities {

        public static async Task CreateCommands(DiscordSocketClient client) {

            var bogosBintedCommand = new SlashCommandBuilder();
            bogosBintedCommand.WithName("bogos-binted");
            bogosBintedCommand.WithDescription("bwaaaaaaa");
                     
            var photosPrintedCommand = new SlashCommandBuilder();
            photosPrintedCommand.WithName("photos-printed");
            photosPrintedCommand.WithDescription("Hey, just wondering if you got your photos printed?");
            
            var insultCommand = new SlashCommandBuilder();
            insultCommand.WithName("insult");
            insultCommand.WithDescription("Insult the user of your choice.");
            insultCommand.AddOption("user", ApplicationCommandOptionType.User, "The user you wish to insult", isRequired: true);
            try {
                await client.CreateGlobalApplicationCommandAsync(bogosBintedCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(photosPrintedCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(insultCommand.Build());

                var guild = client.GetGuild(1094410629487546410);
                await guild.CreateApplicationCommandAsync(insultCommand.Build());
            }
            catch(ApplicationCommandException ex) {
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }
    }
}
