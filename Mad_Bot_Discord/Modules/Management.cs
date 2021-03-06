﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mad_Bot_Discord.Core.UserAccounts;
using Discord.WebSocket;
using System.IO;
using System.Threading;
using Discord.Rest;

namespace Mad_Bot_Discord.Modules
{
    public class Management : ModuleBase<SocketCommandContext>
    {

        [Command("Serverinfo"), Alias("si")]
        public async Task Serverinfo()
        {
            await Context.Guild.DownloadUsersAsync();

            string name = Context.Guild.Name;
            ulong id = Context.Guild.Id;

            string iconUrl = Context.Guild.IconUrl;

            DateTime createdDT = Context.Guild.CreatedAt.UtcDateTime;
            string createdS = createdDT.ToString();
            TimeSpan age = DateTime.Now.Subtract(createdDT);

            SocketGuildUser owner = Context.Guild.Owner;

            int membCount = Context.Guild.MemberCount;

            int roleCount = Context.Guild.Roles.Count;

            EmbedBuilder embed = new EmbedBuilder().
                WithColor(Utilities.GetColor()).
                WithImageUrl(iconUrl).
                WithUrl(iconUrl).
                WithTitle("Serverinfo for " + Context.User.Username).
                AddField("Name:", name, true).
                AddField("ID:", id.ToString(), true).
                AddField("Created At:", createdDT.ToString() + "\nmm/dd/yy", true).
                AddField("Age:", Math.Floor(age.TotalDays) + " days.", true).
                AddField("Owner:", owner.Username + "#" + owner.Discriminator, true).
                AddField("Member Count:", membCount.ToString(), true).
                AddField("Role Count:", roleCount.ToString(), true).
                WithFooter(x =>
                {
                    x.Text = Context.User.Username + "#" + Context.User.Discriminator + " at " + DateTime.Now.ToUniversalTime().ToString();
                    x.IconUrl = Context.User.GetAvatarUrl();
                });


            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

        [Command("Roles"), Alias("Ranks")]
        public async Task Roles()
        {
            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Role list for " + Context.User.Username, "**Roles:**\n\n**Happy Visitor** - Earned by joining the server.\n\n**Unhappy Member** - Earned by reaching level 5." +
                "\n\n**Bothered Regular** - Earned by reaching level 15.\n\n**Annoyed Addict** - Earned by reaching level 30.\n\n**Angry Elder** - Earned by reaching level 50.\n\n**Mad God** - Earned by reaching level 100.", Context));
        }

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

                await user.SendMessageAsync("You've been kicked from " + Context.Guild.Name + " by " + Context.User.Username + ".\n\nReason: " + "You've reached 3 warnings.");

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

                await user.SendMessageAsync("You've been banned from " + Context.Guild.Name + " by " + Context.User.Username + ".\n\nReason: " + "You've reached 6 warnings.");

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

                await user.SendMessageAsync("You've been banned from " + Context.Guild.Name + " by " + Context.User.Username + ".\n\nReason: " + "You've reached 6+ warnings.");

                await user.Guild.AddBanAsync(user, 0, "Passed 6 warnings.");
                userAccount.PunishmentsByWarnings++;

                UserAccounts.SaveAccounts();
                return;
            }

            var embed = new EmbedBuilder();

            await user.SendMessageAsync(Context.User.Username + " has given you a warning. You now have " + userAccount.NumberOfWarnings + " warnings.");

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


            if (reason == "No reason provided.")
                await user.SendMessageAsync("You've been kicked from " + Context.Guild.Name + " by " + Context.User.Username + ".");
            else
                await user.SendMessageAsync("You've been kicked from " + Context.Guild.Name + " by " + Context.User.Username + ".\n\nReason: " + reason);
            
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

            if (reason == "No reason provided.")
                await user.SendMessageAsync("You've been banned from " + Context.Guild.Name + " by " + Context.User.Username + ".");
            else
                await user.SendMessageAsync("You've been banned from " + Context.Guild.Name + " by " + Context.User.Username + ".\n\nReason: " + reason);

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

        [Command("ModXP"), Alias("mxp")]
        public async Task ModXP(uint xp, [Remainder] string memb1 = null)
        {
            SocketGuildUser owner = Context.Guild.GetUser(226223728076390410);

            if (Context.User.Id != 226223728076390410)
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("ModXP Failed", $"You must be {owner.Username} to use this command!", Context));
                return;
            }

            await Context.Guild.DownloadUsersAsync();

            SocketGuildUser member = null;

            if (!string.IsNullOrWhiteSpace(memb1))
            {
                member = (SocketGuildUser)Context.Message.MentionedUsers.FirstOrDefault();
            }
            else
            {
                member = (SocketGuildUser) Context.User;
            }

            UserAccount target = UserAccounts.GetAccount((SocketUser)member);
            target.LastMessage = DateTime.UtcNow - TimeSpan.FromMinutes(1);
            uint oldXP = target.XP;
            target.XP = xp;
            UserAccounts.SaveAccounts();

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("ModXP for " + Context.User.Username, $"{member.Username}'s XP is now {xp}! It was {oldXP} previously.", Context));

        }

        [Command("FixAll")]
        public async Task FixAll()
        {
            //WARING, DO NOT USE UNLESS VERY NEEDED
            /*
               352208507581497347 0
               363393500995387392 5
               446778429832953876 15 
               363393574060163072 30
               363393639956873217 50
               363393659930148865 100
            */

            await Context.Guild.DownloadUsersAsync();

            if (Context.User.Id != 226223728076390410) return;

            foreach (SocketGuildUser user in Context.Guild.Users)
            {
                foreach (IRole role in user.Roles)
                {
                    if (role.Id == 352208507581497347 || role.Id == 363393500995387392 
                        || role.Id == 446778429832953876 || role.Id ==  363393574060163072 
                        || role.Id == 363393639956873217 || role.Id == 363393659930148865) await user.RemoveRoleAsync(role);

                    UserAccount target = UserAccounts.GetAccount(user);

                    SocketGuild guild = Context.Guild;

                    if (target.LevelNumber < 1) await user.AddRoleAsync(guild.GetRole(352208507581497347));
                    if (target.LevelNumber >= 5 && target.LevelNumber < 15) await user.AddRoleAsync(guild.GetRole(363393500995387392));
                    if (target.LevelNumber >= 15 && target.LevelNumber < 30) await user.AddRoleAsync(guild.GetRole(446778429832953876));
                    if (target.LevelNumber >= 30 && target.LevelNumber < 50) await user.AddRoleAsync(guild.GetRole(363393574060163072));
                    if (target.LevelNumber >= 50 && target.LevelNumber < 100) await user.AddRoleAsync(guild.GetRole(363393639956873217));
                    if (target.LevelNumber >= 100) await user.AddRoleAsync(guild.GetRole(363393659930148865));
                    
                }
            }

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("FixAll for " + Context.User.Username, "Everyone's ranks have been fixed.", Context));
        }

        [Command("Poll")]
        public async Task Poll([Remainder] string question)
        {
            SocketGuildUser user = (SocketGuildUser) Context.Message.Author;

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("New Poll", $"Okay, the poll will be: `{question}`.\n\nHow long would" +
                $" you like the poll to last? (answer in seconds)\nAnswer within 10 seconds otherwise the poll will not be created.", Context));

            ulong messageId = Context.Message.Id;
            ulong userId = user.Id;
            SocketTextChannel channel = (SocketTextChannel) Context.Channel;
            
            bool finished = false;

            IUserMessage answer = null;

            int i = 0;
            int amount = 5;
            Timer timer = new Timer(async x =>
            {
                var messages = await channel.GetMessagesAsync(messageId, Discord.Direction.After, amount).Flatten();
                foreach (IUserMessage me in messages)
                {
                    if (me.Author.Id == userId)
                    {
                        answer = me;
                        finished = true;
                    }
                }
                i++;
                amount += 5;
            }, null, 0, 1000);

            while (true)
            {
                if (finished)
                {
                    timer.Dispose();
                    break;
                }

                else if (i > 10)
                {
                    timer.Dispose();
                    await channel.SendMessageAsync("Poll dismissed.");
                    return;
                }
            }

            if (!int.TryParse(answer.Content, out int time))
            {
                await channel.SendMessageAsync("You must send an **integer.**\nPoll dismissed.");
                return;
            }

            await channel.SendMessageAsync("", embed:Utilities.EasyEmbed("New Poll by " + Context.User.Username, "Perfect. One last thing:\nWhat " +
                "would you like the answers to be? (Answers are separated by the '|' (pipe, or line) symbol)" +
                "\nA maximum of 6 answers can be used.\n\nExample: answer 1|answer 2|answer 3. Answer within 20 seconds otherwise the poll will be dismissed.", Context));

            i = 0;
            amount = 5;
            finished = false;

            messageId = answer.Id;
            answer = null;

            timer = new Timer(async x =>
            {
                var messages = await channel.GetMessagesAsync(messageId, Discord.Direction.After, amount).Flatten();
                foreach (IUserMessage mes in messages)
                {
                    if (mes.Author.Id == userId)
                    {
                        answer = mes;
                        finished = true;
                    }
                }
                i++;
                amount += 5;
            }, null, 0, 1000);

            while (true)
            {
                if (finished)
                {
                    timer.Dispose();
                    break;
                }

                else if (i > 20)
                {
                    timer.Dispose();
                    await channel.SendMessageAsync("Poll dismissed.");
                    return;
                }
            }

            string[] finalAnswers = answer.Content.Split('|');

            if (finalAnswers.Length > 6)
            {
                await channel.SendMessageAsync("There must be less than 6 answers.\nPoll dismissed.");
                return;
            }

            string a = $"***{question}*** - {Context.User.Username}\n\n";

            int c = 0;
            foreach (string f in finalAnswers)
            {
                c++;
                a = a + c + ": " + f + "\n";
            }

            a = a + $"\n\nThis poll will end in {time} seconds. (After all reactions have been added)\nAnswer using the reactions below! (It can take up to 20 seconds for all reactions to be added. Please be patient!)";

            IUserMessage m = await channel.SendMessageAsync("", embed:Utilities.EasyEmbed("Poll By " + Context.User.Username, a, Context));

            Emoji[] numberedEmojis =
            {
                new Emoji("1\u20e3"),
                new Emoji("2\u20e3"),
                new Emoji("3\u20e3"),
                new Emoji("4\u20e3"),
                new Emoji("5\u20e3"),
                new Emoji("6\u20e3"),
            };

            int indexEnd = finalAnswers.Length;
            int index = 0;
            Timer t = new Timer(method =>
            {
                m.AddReactionAsync(numberedEmojis[index]);
                index++;
            }, null, 0, 3000);
            
            while (true)
            {
                if (index >= indexEnd)
                {
                    t.Dispose();
                    break;
                }
            }

            bool timerEnded = false;
            Timer finalTimer = new Timer(async methodddddd =>
            {

                ulong reactionMessageId = m.Id;
                IUserMessage reactionMessage = (IUserMessage)await channel.GetMessageAsync(reactionMessageId);
                List<int> reactionVotes = new List<int>();
                string endingResults = "";
                int reactionIndex = 0;

                foreach(var r in reactionMessage.Reactions)
                {
                    reactionVotes.Add(r.Value.ReactionCount - 1);
                    endingResults = endingResults + finalAnswers[reactionIndex] + ": " + reactionVotes[reactionIndex] + " votes.\n";
                    reactionIndex++;
                }

                await channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Poll Ended", $"The poll ***{question}*** by {Context.User.Username} has ended! The results:\n\n{endingResults}" +
                    $"", Context));
                timerEnded = true;
            }, null, time * 1000, Timeout.Infinite);

            while (!timerEnded)
            {

            }

            finalTimer.Dispose();
            return;



        }

        
    }
}