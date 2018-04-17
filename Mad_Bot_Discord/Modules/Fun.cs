using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord.Modules
{
    public class Fun : ModuleBase<SocketCommandContext>
    {

        private Random r = new Random();

        [Command("Pick")]
        public async Task Pick([Remainder] string msg)
        {
            string[] options = msg.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string selection = options[r.Next(0, options.Length)];

            var embed = new EmbedBuilder();

            embed.WithTitle("Choice for " + Context.User.Username)
                .WithDescription("I choose... " + selection + "!")
                .WithColor(Utilities.GetColor())
                .WithFooter(x =>
                {
                    x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                });

            await Context.Channel.SendMessageAsync("", embed: embed);

        }

        [Command("8ball")]
        public async Task EightBall([Remainder] string question = "[No Question]")
        {
            string json = File.ReadAllText("SystemLang/8ballAnswers.json");
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            var answers = new List<string>();
            answers = data.ToObject<List<string>>();
            string selection = answers[r.Next(0, answers.Count)];
            string un = Context.User.Username;

            var embed = new EmbedBuilder();
            embed.WithTitle($"8Ball for {un}")
                .WithDescription($"You said {question}...")
                .AddInlineField("**Answer:**", selection)
                .WithColor(Utilities.GetColor())
                .WithFooter(x =>
                {
                    x.IconUrl = Context.User.GetAvatarUrl();
                    x.Text = un + Context.User.Discriminator + " at " + Context.Message.Timestamp;
                });

            await Context.Channel.SendMessageAsync("", embed: embed);
        }

    }
}
