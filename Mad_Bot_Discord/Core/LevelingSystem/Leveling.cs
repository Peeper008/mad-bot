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
