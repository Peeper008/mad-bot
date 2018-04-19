using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mad_Bot_Discord.Core.UserAccounts;
using Discord.WebSocket;

namespace Mad_Bot_Discord.Modules
{
    public class Management : ModuleBase<SocketCommandContext>
    {

        [Command("Mute")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task MuteUser(string memb1 = "")
        {
            await Context.Guild.DownloadUsersAsync();

            SocketGuildUser user = (SocketGuildUser)Context.Message.MentionedUsers.FirstOrDefault();

            var userAccount = UserAccounts.GetAccount((SocketUser)user);

            userAccount.IsMuted = true;

            var embed = new EmbedBuilder();

            embed.WithTitle("Muted by " + Context.User.Username)
                .WithDescription(user.Mention + " is now muted.")
                .WithFooter(x =>
                {
                    x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithColor(Utilities.GetColor());

            await Context.Channel.SendMessageAsync("", embed: embed);

        }

        [Command("Unmute")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task UnmuteUser(string memb1 = "")
        {
            await Context.Guild.DownloadUsersAsync();

            SocketGuildUser user = (SocketGuildUser) Context.Message.MentionedUsers.FirstOrDefault();

            var userAccount = UserAccounts.GetAccount((SocketUser)user);

            userAccount.IsMuted = false;

            var embed = new EmbedBuilder();

            embed.WithTitle("Unmuted by " + Context.User.Username)
                .WithDescription(user.Mention + " is now unmuted.")
                .WithFooter(x =>
                {
                    x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithColor(Utilities.GetColor());

            await Context.Channel.SendMessageAsync("", embed: embed);
        }

        [Command("Warn")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task WarnUser(string memb1 = "")
        {
            await Context.Guild.DownloadUsersAsync();

            SocketGuildUser user = (SocketGuildUser) Context.Message.MentionedUsers.FirstOrDefault();

            Console.WriteLine("HELP");

            var userAccount = UserAccounts.GetAccount((SocketUser)user);
            userAccount.NumberOfWarnings++;
            UserAccounts.SaveAccounts();

            // punishment check
            if (userAccount.NumberOfWarnings == 3)
            {
                var embed4 = new EmbedBuilder();

                embed4.WithTitle("Warning by " + Context.User.Username)
                    .WithDescription(user.Mention + " has reached 3 warnings and has been kicked.")
                    .WithFooter(x =>
                    {
                        x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                        x.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithColor(Utilities.GetColor());

                await Context.Channel.SendMessageAsync("", embed: embed4);
                await user.KickAsync("Reached 3 warnings.");
                userAccount.PunishmentsByWarnings++;

                UserAccounts.SaveAccounts();
                return;
            }
            else if (userAccount.NumberOfWarnings == 6)
            {
                var embed3 = new EmbedBuilder();

                embed3.WithTitle("Warning by " + Context.User.Username)
                    .WithDescription(user.Mention + " has reached 6 warnings and has been banned.")
                    .WithFooter(x =>
                    {
                        x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                        x.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithColor(Utilities.GetColor());

                await Context.Channel.SendMessageAsync("", embed: embed3);

                await user.Guild.AddBanAsync(user, 0, "Reached 6 warnings.");
                userAccount.PunishmentsByWarnings++;

                UserAccounts.SaveAccounts();
                return;
            }
            else if (userAccount.NumberOfWarnings > 6)
            {
                var embed2 = new EmbedBuilder();

                embed2.WithTitle("Warning by " + Context.User.Username)
                    .WithDescription(user.Mention + " has reached 6+ warnings and has been banned.")
                    .WithFooter(x =>
                    {
                        x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                        x.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithColor(Utilities.GetColor());

                await Context.Channel.SendMessageAsync("", embed: embed2);

                await user.Guild.AddBanAsync(user, 0, "Passed 6 warnings.");
                userAccount.PunishmentsByWarnings++;

                UserAccounts.SaveAccounts();
                return;
            }

            var embed = new EmbedBuilder();

            embed.WithTitle("Warning by " + Context.User.Username)
                .WithDescription(user.Mention + " now has " + userAccount.NumberOfWarnings + " warnings.")
                .WithFooter(x =>
                {
                    x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithColor(Utilities.GetColor());

            await Context.Channel.SendMessageAsync("", embed: embed);
        }

        [Command("Kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickUser(string memb1 = "", [Remainder] string reason = "No reason provided.")
        {
            await Context.Guild.DownloadUsersAsync();

            SocketGuildUser user = (SocketGuildUser)Context.Message.MentionedUsers.FirstOrDefault();

            await user.KickAsync(reason);

            var embed = new EmbedBuilder();

            embed.WithTitle("Kicked by " + Context.User.Username)
                .WithDescription($"{user.Mention} has been kicked.")
                .WithFooter(x =>
                {
                    x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithColor(Utilities.GetColor());

            await Context.Channel.SendMessageAsync("", embed: embed);
        }

        [Command("Ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUser(string memb1 = "", [Remainder] string reason = "No reason provided.")
        {
            await Context.Guild.DownloadUsersAsync();

            SocketGuildUser user = (SocketGuildUser)Context.Message.MentionedUsers.FirstOrDefault();

            await user.Guild.AddBanAsync(user, 0, reason);

            var embed = new EmbedBuilder();

            embed.WithTitle("Banned by " + Context.User.Username)
                .WithDescription($"{user.Mention} has been banned.")
                .WithFooter(x =>
                {
                    x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithColor(Utilities.GetColor());

            await Context.Channel.SendMessageAsync("", embed: embed);
        }

        [Command("ModXP")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task ModXP(string memb1, uint xp)
        {
            await Context.Guild.DownloadUsersAsync();

            SocketGuildUser member = (SocketGuildUser)Context.Message.MentionedUsers.FirstOrDefault();

            UserAccount target = UserAccounts.GetAccount((SocketUser)member);
            target.XP = xp;
            UserAccounts.SaveAccounts();

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("ModXP for " + Context.User.Username, $"{member.Username}'s XP is now {xp}!", Context));

        }
    }
}