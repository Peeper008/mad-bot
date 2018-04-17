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

        [Command("8Ball")]
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

        [Command("Roll")]
        public async Task Roll([Remainder] string minmax = "1|6")
        {
            string[] ns = minmax.Split(new char[] { ' ', '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (!int.TryParse(ns[0], out int n1))
            {
                var embed2 = new EmbedBuilder();
                embed2.WithTitle("Failed Roll")
                    .WithColor(Utilities.GetColor())
                    .WithFooter(x =>
                    {
                        x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                        x.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithDescription($"Failed to parse {ns[0]}. Roll only supports integers, you can separate them using a space or the '|' key. " +
                    $"The max integer allowed is 2147483646.");

                await Context.Channel.SendMessageAsync("", embed: embed2);
                return;
            }
            if (!int.TryParse(ns[1], out int n2))
            {
                var embed2 = new EmbedBuilder();

                embed2.WithTitle("Failed Roll")
                    .WithColor(Utilities.GetColor())
                    .WithFooter(x =>
                    {
                        x.Text = $"{Context.User.Username}#{Context.User.Discriminator} at {Context.Message.Timestamp}";
                        x.IconUrl = Context.User.GetAvatarUrl();
                    })
                    .WithDescription($"Failed to parse {ns[1]}. Roll only supports integers, you can separate them using a space or the '|' key. " +
                    $"The max integer allowed is 2147483646.");

                await Context.Channel.SendMessageAsync("", embed: embed2);
                return;
            }

            if (n2 < n1)
            {
                var embed3 = new EmbedBuilder();
                embed3.WithTitle("Failed Roll")
                    .WithColor(Utilities.GetColor())
                    .WithDescription("The max number must be larger than the min number.")
                    .WithFooter(x =>
                    {
                        x.Text = Context.User.Username + "#" + Context.User.Discriminator + " at " + Context.Message.Timestamp;
                        x.IconUrl = Context.User.GetAvatarUrl();
                    });

                await Context.Channel.SendMessageAsync("", embed: embed3);
                return;
            }
            else
                n2++;

            int selection;

            try
            {
                selection = r.Next(n1, n2);
                
            }
            catch (ArgumentOutOfRangeException)
            {
                var embed3 = new EmbedBuilder();
                embed3.WithTitle("Failed Roll")
                    .WithColor(Utilities.GetColor())
                    .WithDescription("The max number must be larger than the min number.")
                    .WithFooter(x =>
                    {
                        x.Text = Context.User.Username + "#" + Context.User.Discriminator + " at " + Context.Message.Timestamp;
                        x.IconUrl = Context.User.GetAvatarUrl();
                    });

                await Context.Channel.SendMessageAsync("", embed: embed3);

                return;
            }
            

            var embed = new EmbedBuilder();
            embed.WithTitle("Roll for " + Context.User.Username)
                .WithColor(Utilities.GetColor())
                .WithDescription("Your two numbers were... " + ns[0] + " and " + ns[1])
                .AddInlineField("**New Number:**", selection)
                .WithFooter(x =>
                {
                    x.IconUrl = Context.User.GetAvatarUrl();
                    x.Text = Context.User.Username + "#" + Context.User.Discriminator + " at " + Context.Message.Timestamp;
                });

            await Context.Channel.SendMessageAsync("", embed: embed);
        }

    }
}
