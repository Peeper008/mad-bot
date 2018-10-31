using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Discord;
using Discord.Commands;

namespace Mad_Bot_Discord
{
    class Utilities
    {
        // Creates the random class variable.
        private static Random r = new Random();

        public static Color GetColor()
        {
            return new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
        }

        public static double GetRandomNumber(double minimum, double maximum)
        {
            return r.NextDouble() * (maximum - minimum) + minimum;
        }

        public static Embed EasyEmbed(string title, string description, SocketCommandContext context)
        {
            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle(title)
                .WithDescription(description)
                .WithFooter(x =>
                {
                    x.Text = $"{context.User.Username}#{context.User.Discriminator} at {context.Message.Timestamp}";
                    x.IconUrl = context.User.GetAvatarUrl();
                })
                .WithColor(GetColor());

            return embed.Build();
        }

        public static Embed EasyEmbed(string title, string description, Color color, SocketCommandContext context)
        {
            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle(title)
                .WithDescription(description)
                .WithFooter(x =>
                {
                    x.Text = $"{context.User.Username}#{context.User.Discriminator} at {context.Message.Timestamp}";
                    x.IconUrl = context.User.GetAvatarUrl();
                })
                .WithColor(color);

            return embed.Build();
        }

        public static Embed EasyEmbed(string title, string description, string inlineTitle, string inlineValue, SocketCommandContext context)
        {
            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle(title)
                .WithDescription(description)
                .WithFooter(x =>
                {
                    x.Text = $"{context.User.Username}#{context.User.Discriminator} at {context.Message.Timestamp}";
                    x.IconUrl = context.User.GetAvatarUrl();
                })
                .WithColor(GetColor())
                .AddInlineField(inlineTitle, inlineValue);

            return embed.Build();
        }
    }
}
