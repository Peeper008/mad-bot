using Discord;
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
    }
}
