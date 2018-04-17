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
            await Context.Channel.SendMessageAsync("https://github.com/Peeper008/mad-bot/wiki");
        }

        [Command("WhatLevelIs")]
        public async Task WhatLevelIs(uint xp)
        {
            uint level = (uint)Math.Sqrt(xp / 50);
            await Context.Channel.SendMessageAsync("The level is " + level);
        }

        [Command("react")]
        public async Task HandleReactionMessage()
        {
            RestUserMessage msg = await Context.Channel.SendMessageAsync("React to me!");
            Global.MessageIdToTrack = msg.Id;
        }

        [Command("myStats")]
        public async Task MyStats([Remainder] string arg = "")
        {
            SocketUser target = null;
            var mentionedUser = Context.Message.MentionedUsers.FirstOrDefault();

            target = mentionedUser ?? Context.User;

            var account = UserAccounts.GetAccount(target);
            await Context.Channel.SendMessageAsync($"{target.Username} has {account.XP} XP");
        }

        [Command("addXP"), RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddXP(uint xp)
        {
            var account = UserAccounts.GetAccount(Context.User);
            account.XP += xp;
            UserAccounts.SaveAccounts();
            await Context.Channel.SendMessageAsync($"You gained {xp} XP.");
        }

        [Command("echo")]
        public async Task Echo([Remainder] string message)
        {
            var embed = new EmbedBuilder();

            embed.WithTitle(Utilities.GetFormattedAlert("ECHO_&NAME", Context.User.Username))
                .WithDescription(message)
                .WithColor(0, 255, 0);

            await Context.Channel.SendMessageAsync("", false, embed);
            
        }

        [Command("pick2")]
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
