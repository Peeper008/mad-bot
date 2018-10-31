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

        [Command("Remindme")]
        public async Task Remindme(string amount = "5", string timeType = "minutes", [Remainder] string message = "")
        {
            if (int.TryParse(amount, out int amountOfTime))
            {
                int multiplier = 1000;

                switch (timeType)
                {
                    case "millisecond":
                    case "milliseconds":
                    case "millis":
                    case "milli":
                        break;

                    case "second":
                    case "seconds":
                    case "sec":
                    case "secs":
                        multiplier = 1000;
                        break;

                    case "minute":
                    case "minutes":
                    case "min":
                    case "mins":
                        multiplier = 60000;
                        break;
                }

                if (message == "") message = "Reminder for " + Context.User.Username;

                var timer = new System.Threading.Timer(async (e) =>
                {
                    await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed(
                        "RemindMe for " + Context.User.Username, message + "\n" + Context.User.Mention, Context));
                    return;

                }, null, amountOfTime * multiplier, System.Threading.Timeout.Infinite);

                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("RemindMe for " + Context.User.Username, $"Reminder set. It will go off in `{amountOfTime} {timeType.Substring(0, 3)}`.", Context));
                return;
            }

            await Context.Channel.SendMessageAsync("'", embed: Utilities.EasyEmbed(
                "RemindMe Failed", "The amount of time needs to be a number!", Context));
            return;
        }
    }
}
