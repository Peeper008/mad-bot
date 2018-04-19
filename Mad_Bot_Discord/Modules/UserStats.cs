using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Mad_Bot_Discord.Core.UserAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord.Modules
{
    public class UserStats : ModuleBase<SocketCommandContext>
    {
        [Command("Stats")]
        public async Task Stats([Remainder] string user = "optional")
        {
            SocketUser member = Context.Message.MentionedUsers.FirstOrDefault();
            SocketUser target = member ?? Context.User;
            var userAccount = UserAccounts.GetAccount(target);

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle($"{target.Username}'s Stats")
                .WithColor(Utilities.GetColor())
                .WithDescription($"{target.Username}'s stats are:")
                .AddInlineField("**XP:**", $"`{userAccount.XP}`")
                .AddInlineField("**Level:**", $"`{userAccount.LevelNumber}`")
                .AddInlineField("**# Of Warnings:**", $"`{userAccount.NumberOfWarnings}`")
                .AddInlineField("**Is Muted?:**", $"`{userAccount.IsMuted}`")
                .WithFooter(x =>
                {
                    x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithThumbnailUrl(target.GetAvatarUrl());

            await Context.Channel.SendMessageAsync("", embed: embed);
        }

        [Command("Leaderboard")]
        public async Task Leaderboard(int page = 1)
        {
            await Context.Guild.DownloadUsersAsync();

            List<UserAccount> userAccounts = new List<UserAccount>();

            foreach (SocketGuildUser u in Context.Guild.Users)
            {
                userAccounts.Add(UserAccounts.GetAccount(u));

                if (UserAccounts.GetAccount(u).Name == null)
                {
                    UserAccounts.GetAccount(u).Name = u.Username;
                    UserAccounts.SaveAccounts();
                }
            }

            page = (page > Math.Ceiling((double)userAccounts.Count / 5)) ? (int) Math.Ceiling((double)userAccounts.Count / 5) : page;

            userAccounts.Sort(delegate (UserAccount x, UserAccount y)
            {
                if (x.XP == y.XP) return 0;
                else if (x.XP > y.XP) return -1;
                else if (x.XP < y.XP) return 1;
                else return 0;
            });

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle("Current Leaderboard")
                .WithDescription($"Page {page} of {Math.Ceiling((double)userAccounts.Count / 5)}.")
                .WithColor(Utilities.GetColor())
                .WithFooter(x =>
                {
                    x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                });

            for (int i = page*5-5; i < userAccounts.Count; i++)
            {
                if (userAccounts[i].Name == Context.User.Username) userAccounts[i].Name = $"{userAccounts[i].Name} (You)";
                embed.AddField($"#{i+1} - " + userAccounts[i].Name + ":", $"**XP:** {userAccounts[i].XP} \n**Level:** {userAccounts[i].LevelNumber} \n...");

                if (i > page * 5 - 2) break;
            }

            UserAccount yourAcc = UserAccounts.GetAccount(Context.User);
            int yourPos = userAccounts.FindIndex(x => x.ID == yourAcc.ID);

            userAccounts[yourPos].Name = Context.User.Username + " (You)";

            embed.AddField($"#{yourPos + 1} - {userAccounts[yourPos].Name}:", $"**XP:** {userAccounts[yourPos].XP} \n**Level:** {userAccounts[yourPos].LevelNumber} \n...");

            await Context.Channel.SendMessageAsync("", embed: embed);
        }
    }
}
