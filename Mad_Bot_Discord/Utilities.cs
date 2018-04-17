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
        private static Random r = new Random();

        private static Dictionary<string, string> alerts;

        static Utilities()
        {
            string json = File.ReadAllText("SystemLang/alerts.json");
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            alerts = data.ToObject<Dictionary<string, string>>();
        }

        public static string GetAlert(string key)
        {
            if (alerts.ContainsKey(key)) return alerts[key];
            return "";
        }

        public static string GetFormattedAlert(string key, params object[] parameter)
        {
            if (alerts.ContainsKey(key))
            {
                return String.Format(alerts[key], parameter);
            };
            return "";
        }

        public static string GetFormattedAlert(string key, object parameter)
        {
            return GetFormattedAlert(key, new object[] { parameter });
        }

        public static Color GetColor()
        {
            return new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
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
