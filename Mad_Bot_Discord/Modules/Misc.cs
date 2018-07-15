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
using Newtonsoft.Json;
using System.IO;

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

        [Command("Prefix")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Prefix(string prefix = "")
        {
            string configFolder = "Resources";
            string configFile = "config.json";

            string jsonInfo = File.ReadAllText(configFolder + "/" + configFile);
            BotConfig config = JsonConvert.DeserializeObject<BotConfig>(jsonInfo);

            if (string.IsNullOrEmpty(prefix))
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Prefix for " + Context.User.Username, $"The current prefix is: [`{config.cmdPrefix}`].", Context));
                return;
            }

            if (prefix.Length > 50)
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Prefix failed", $"The prefix must be less than 50 characters long.", Context));
                return;
            }

            string oldPrefix = config.cmdPrefix;

            config.cmdPrefix = prefix;
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFolder + "/" + configFile, json);

            Config.Refresh();
            Program.RefreshGame();

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Prefix for " + Context.User.Username, $"The prefix is now [`{config.cmdPrefix}`]. The previous prefix was [`{oldPrefix}`]", Context));
            return;
        }
    }
}
