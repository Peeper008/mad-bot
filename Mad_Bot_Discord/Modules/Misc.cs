using Discord;
using Discord.Commands;
using Discord.WebSocket;
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

            embed.WithTitle(Utilities.GetFormattedAlert("ECHO_&NAME", Context.User.Username))
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
            embed.WithTitle(Utilities.GetFormattedAlert("PICK_&NAME", Context.User.Username))
                .WithDescription(selection)
                .WithColor(255, 255, 0);

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("secret")]
        public async Task RevealSecret([Remainder] string arg = "")
        {
            EmbedBuilder embed = new EmbedBuilder();

            if (!UserIsSecretOwner((SocketGuildUser) Context.User))
            {
                embed.WithTitle("Permission Denied")
                    .WithDescription(":x: You need the SecretOwner role to do that. " + Context.User.Mention)
                    .WithColor(0, 255, 255);

                await Context.Channel.SendMessageAsync("", false, embed);
                return;
            }

            embed.WithTitle("Secret Code")
                .WithDescription(Utilities.GetAlert("SECRET"))
                .WithColor(0, 255, 255);

            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync("", false, embed);
        }

        private bool UserIsSecretOwner(SocketGuildUser user)
        {
            //user.Guild.Roles
            string targetRoleName = "SecretOwner";
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;

            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }

        [Command("data")]
        public async Task GetData()
        {
            await Context.Channel.SendMessageAsync("Data Has " + DataStorage.GetPairsCount() + " pairs.");
        }
    }
}
