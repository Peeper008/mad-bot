using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Mad_Bot_Discord.Core.UserAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("Help")]
        public async Task Help()
        {
            // Sends a link to the wiki page.
            await Context.Channel.SendMessageAsync("https://github.com/Peeper008/mad-bot/wiki");
        }
    }
}
