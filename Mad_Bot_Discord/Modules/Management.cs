using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Mad_Bot_Discord.Core.UserAccounts;
using Discord.WebSocket;

namespace Mad_Bot_Discord.Modules
{
    public class Management : ModuleBase<SocketCommandContext>
    {
        [Command("Warn")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task WarnUser (IGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount((SocketUser)user);
            userAccount.NumberOfWarnings++;
            UserAccounts.SaveAccounts();
           
            // punishment check
            if (userAccount.NumberOfWarnings == 3)
            {
                await Context.Channel.SendMessageAsync(Context.User.Username + " has reached 3 warnings and has been kicked.");
                await user.KickAsync("Reached 3 warnings.");
                userAccount.PunishmentsByWarnings++;

                UserAccounts.SaveAccounts();
                return;
            }
            else if (userAccount.NumberOfWarnings == 6)
            {
                await Context.Channel.SendMessageAsync(Context.User.Username + " has reached 6 warnings and has been banned.");
                await user.Guild.AddBanAsync(user, 0, "Reached 6 warnings.");
                userAccount.PunishmentsByWarnings++;

                UserAccounts.SaveAccounts();
                return;
            }
            else if (userAccount.NumberOfWarnings > 6)
            {
                await Context.Channel.SendMessageAsync(Context.User.Username + " has reached 6+ warnings and has been banned.");
                await user.Guild.AddBanAsync(user, 0, "Passed 6 warnings.");
                userAccount.PunishmentsByWarnings++;

                UserAccounts.SaveAccounts();
                return;
            }

            await Context.Channel.SendMessageAsync(Context.User.Username + " now has " + userAccount.NumberOfWarnings + " warnings.");
        }

        [Command("Kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickUser(IGuildUser user, string reason = "No reason provided.")
        {
            await user.KickAsync(reason);
        }

        [Command("Ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUser(IGuildUser user, string reason = "No reason provided.")
        {
            await user.Guild.AddBanAsync(user, 0, reason);
        }
    }
}
