﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        public async Task Echo([Remainder] string message)
        {
            var embed = new EmbedBuilder();

            embed.WithTitle("Message by " + Context.User.Username)
                .WithDescription(message)
                .WithColor(0, 255, 0);

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("pick")]
        public async Task Pick([Remainder] string message)
        {
            string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            Random r = new Random();
            string selection = options[r.Next(0, options.Length)];

            var embed = new EmbedBuilder();
            embed.WithTitle("Choice for " + Context.User.Username)
                .WithDescription(selection)
                .WithColor(255, 255, 0);

            await Context.Channel.SendMessageAsync("", false, embed);
        }
    }
}
