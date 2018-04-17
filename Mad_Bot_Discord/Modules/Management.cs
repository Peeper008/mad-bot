﻿using Discord.Commands;
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

        [Command("Mute")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task MuteUser (IGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount((SocketUser)user);

            userAccount.IsMuted = true;

            var embed = new EmbedBuilder();

            embed.WithTitle("Mute by " + Context.User.Username)
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
                var embed = new EmbedBuilder();

                embed.WithTitle("Warn by " + Context.User.Username)
                    .WithDescription(user.Mention + " has reached 3 warnings and has been kicked.")
                    .WithFooter(x =>
                    {
                        x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                        x.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithColor(Utilities.GetColor());

                await Context.Channel.SendMessageAsync("", embed: embed);
                await user.KickAsync("Reached 3 warnings.");
                userAccount.PunishmentsByWarnings++;

                UserAccounts.SaveAccounts();
                return;
            }
            else if (userAccount.NumberOfWarnings == 6)
            {
                var embed = new EmbedBuilder();

                embed.WithTitle("Warn by " + Context.User.Username)
                    .WithDescription(user.Mention + " has reached 6 warnings and has been banned.")
                    .WithFooter(x =>
                    {
                        x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithColor(Utilities.GetColor());

                await Context.Channel.SendMessageAsync("", embed: embed);

                await user.Guild.AddBanAsync(user, 0, "Reached 6 warnings.");
                userAccount.PunishmentsByWarnings++;

                UserAccounts.SaveAccounts();
                return;
            }
            else if (userAccount.NumberOfWarnings > 6)
            {
                var embed = new EmbedBuilder();

                embed.WithTitle("Warn by " + Context.User.Username)
                    .WithDescription(user.Mention + " has reached 6+ warnings and has been banned.")
                    .WithFooter(x =>
                    {
                        x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                        x.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithColor(Utilities.GetColor());

                await Context.Channel.SendMessageAsync("", embed: embed);

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
        public async Task KickUser(IGuildUser user, [Remainder] string reason = "No reason provided.")
        {
            await user.KickAsync(reason);

            var embed = new EmbedBuilder();

            embed.WithTitle("Kick by " + Context.User.Username)
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
        public async Task BanUser(IGuildUser user, [Remainder] string reason = "No reason provided.")
        {
            await user.Guild.AddBanAsync(user, 0, reason);

            var embed = new EmbedBuilder();

            embed.WithTitle("Kick by " + Context.User.Username)
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
    }
}
