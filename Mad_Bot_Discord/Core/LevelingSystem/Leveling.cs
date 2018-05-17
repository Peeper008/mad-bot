using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord.Core.LevelingSystem
{
    internal static class Leveling
    {
        internal static async void UserSentMessage(SocketGuildUser user, SocketTextChannel channel)
        {

            var userAccount = UserAccounts.UserAccounts.GetAccount(user);
            uint oldLevel = userAccount.LevelNumber;

            // if the user has a timeout, ignore them
            if ((DateTime.UtcNow - userAccount.LastMessage.ToUniversalTime()) < TimeSpan.FromMinutes(1))
            {
                return;
            }

            // name value for debug purposes
            if (user.Username != userAccount.Name)
                userAccount.Name = user.Username;
            // name value for debug purposes

            userAccount.LastMessage = DateTime.Now;
            userAccount.XP += 1;
            UserAccounts.UserAccounts.SaveAccounts();
            uint newLevel = userAccount.LevelNumber;

            if (oldLevel != newLevel)
            {
             /*
                352208507581497347 0
                363393500995387392 5
                446778429832953876 15 
                363393574060163072 30
                363393639956873217 50
                363393659930148865 100
             */

                if (newLevel == 5)
                {
                    await user.AddRoleAsync(user.Guild.GetRole(363393500995387392));

                    foreach (IRole r in user.Roles)
                    {
                        if (r.Id == 352208507581497347) await user.RemoveRoleAsync(r);
                    }



                    var embed = new EmbedBuilder();

                    embed.WithColor(67, 160, 71)
                        .WithTitle("LEVEL UP!")
                        .WithDescription(user.Username + " just leveled up and is now an `Unhappy Member`!")
                        .AddInlineField("LEVEL", newLevel)
                        .AddInlineField("XP", userAccount.XP);

                    await channel.SendMessageAsync("", false, embed);
                }
                else if (newLevel == 15)
                {
                    await user.AddRoleAsync(user.Guild.GetRole(446778429832953876));

                    foreach (IRole r in user.Roles)
                    {
                        if (r.Id == 363393500995387392) await user.RemoveRoleAsync(r);
                    }

                    var embed = new EmbedBuilder();

                    embed.WithColor(67, 160, 71)
                        .WithTitle("LEVEL UP!")
                        .WithDescription(user.Username + " just leveled up and is now a `Bothered Regular`!")
                        .AddInlineField("LEVEL", newLevel)
                        .AddInlineField("XP", userAccount.XP);

                    await channel.SendMessageAsync("", false, embed);
                }
                else if (newLevel == 30)
                {
                    await user.AddRoleAsync(user.Guild.GetRole(363393574060163072));

                    foreach (IRole r in user.Roles)
                    {
                        if (r.Id == 446778429832953876) await user.RemoveRoleAsync(r);
                    }

                    var embed = new EmbedBuilder();

                    embed.WithColor(67, 160, 71)
                        .WithTitle("LEVEL UP!")
                        .WithDescription(user.Username + " just leveled up and is now an `Annoyed Addict`!")
                        .AddInlineField("LEVEL", newLevel)
                        .AddInlineField("XP", userAccount.XP);

                    await channel.SendMessageAsync("", false, embed);
                }
                else if (newLevel == 50)
                {
                    await user.AddRoleAsync(user.Guild.GetRole(363393639956873217));

                    foreach (IRole r in user.Roles)
                    {
                        if (r.Id == 363393574060163072) await user.RemoveRoleAsync(r);
                    }

                    var embed = new EmbedBuilder();

                    embed.WithColor(67, 160, 71)
                        .WithTitle("LEVEL UP!")
                        .WithDescription(user.Username + " just leveled up and is now an `Angry Elder`!")
                        .AddInlineField("LEVEL", newLevel)
                        .AddInlineField("XP", userAccount.XP);

                    await channel.SendMessageAsync("", false, embed);
                }
                else if (newLevel == 100)
                {
                    await user.AddRoleAsync(user.Guild.GetRole(363393659930148865));

                    foreach (IRole r in user.Roles)
                    {
                        if (r.Id == 363393639956873217) await user.RemoveRoleAsync(r);
                    }

                    var embed = new EmbedBuilder();

                    embed.WithColor(67, 160, 71)
                        .WithTitle("LEVEL UP!")
                        .WithDescription(user.Username + " just leveled up and is now a `Mad God`! ***Congrats, you have reached the maximum rank!***")
                        .AddInlineField("LEVEL", newLevel)
                        .AddInlineField("XP", userAccount.XP);

                    await channel.SendMessageAsync("", false, embed);
                }
                else
                {
                    // the user leveled up
                    var embed = new EmbedBuilder();

                    embed.WithColor(67, 160, 71)
                        .WithTitle("LEVEL UP!")
                        .WithDescription(user.Username + " just leveled up!")
                        .AddInlineField("LEVEL", newLevel)
                        .AddInlineField("XP", userAccount.XP);

                    await channel.SendMessageAsync("", false, embed);
                }
            }
        }
    }
}
