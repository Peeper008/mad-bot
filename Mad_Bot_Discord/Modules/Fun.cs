using Discord;
using Discord.Commands;
using Discord.WebSocket;
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

            string allOptions = "";

            foreach (string o in options)
                allOptions = allOptions + o + " ";

            var embed = new EmbedBuilder();

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Choice for " + Context.User.Username,
                $"Your choices were: `{allOptions}`", "**My Choice:**", $"I choose... `{selection}`!", Context));

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

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed($"8-Ball for {un}", $"You said: `{question}`", 
                "**Answer:**", selection, Context));
        }

        [Command("Roll")]
        public async Task Roll([Remainder] string minmax = "1|6")
        {
            string[] ns = minmax.Split(new char[] { ' ', '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (!int.TryParse(ns[0], out int n1))
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Failed Roll", $"Failed to parse `{ns[0]}`. " +
                    $"Roll only supports integers, you can separate them using a space or the '|' key. The max integer allowed is 2147483646.", Context));

                return;
            }
            if (!int.TryParse(ns[1], out int n2))
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Failed Roll", $"Failed to parse `{ns[1]}`. " +
                    $"Roll only supports integers, you can separate them using a space or the '|' key. The max integer allowed is 2147483646.", Context));

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
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed(
                    "Failed Roll", "The max number must be larger than the min number.", Context));

                return;
            }

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed(
                "Roll for " + Context.User.Username, "Your two numbers were... `" + ns[0] + "` and `" + ns[1] + "`", "**New Number:**", 
                "`" + selection.ToString() + "`", Context));
        }

        [Command("Coinflip")]
        public async Task Coinflip([Remainder] string throwaway = "")
        {
            int n = r.Next(0, 2);

            string selection = (n == 0) ? "Heads!" : "Tails!";

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed(
                "Coinflip for " + Context.User.Username, "I choose... " + "`" + selection + "`", Context));
            
        }

        [Command("Ship")]
        public async Task Ship(string memb1 = "", string memb2 = "")
        {
            await Context.Guild.DownloadUsersAsync();

            SocketGuildUser target1 = null;
            SocketGuildUser target2 = null;

            SocketUser mentionedUser1 = null;
            SocketUser mentionedUser2 = null;

            mentionedUser1 = Context.Message.MentionedUsers.ElementAtOrDefault(0);
            mentionedUser2 = Context.Message.MentionedUsers.ElementAtOrDefault(1);

            target1 = (SocketGuildUser) mentionedUser1 ?? null;
            target2 = (SocketGuildUser) mentionedUser2 ?? null;
            
           if (target1 == null )
            {
                SocketGuildUser m = Context.Guild.Users.ElementAt(r.Next(0, Context.Guild.MemberCount));
                target1 = m;
            }

           if (target2 == null)
            {
                SocketGuildUser m = Context.Guild.Users.ElementAt(r.Next(0, Context.Guild.MemberCount));
                target2 = m;
            }

            string un1 = target1.Username;
            string un2 = target2.Username;

            string newName = un1.Substring(0, (un1.Length / 2) + r.Next(-1, 2)) + un2.Substring((un2.Length / 2) + r.Next(-1, 2));

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed($"Ship for {Context.User.Username}",
                $"You shipped `{target1.Username}` and `{target2.Username}` together!", "**The Result:**", $"Their new ship name is... `{newName}`", Context));
        }

        [Command("Colorme"), Alias("Colourme")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Colorme(string hex)
        {
            bool colorFound = false;
            IRole role = null;
            SocketGuildUser user = (SocketGuildUser)Context.User;
            
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            string colour = (Context.Message.Content.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0].Substring(Config.bot.cmdPrefix.Length).ToLower() == "colorme") ? "Color" : "Colour";

            if (hex.Length < 6)
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed($"{colour}me Failed", $"{hex} is too short! Hex codes are 6 characters long (not counting the #) and {hex} is {hex.Length} characters long!", Context));
                return;
            }
            else if (hex.Length > 6)
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed($"{colour}me Failed", $"{hex} is too long! Hex codes are 6 characters long (not counting the #) and {hex} is {hex.Length} characters long!", Context));
                return;
            }

            foreach (SocketRole r in Context.Guild.Roles)
            {
                if (r.Name == "colorrole:" + hex)
                {
                    colorFound = true;
                    role = r;
                    break;
                }
            }

            Color color;

            if (hex == "000000")
            {
                hex = "000001";
            }

            try
            {
                int red = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                int green = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                int blue = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

                color = new Color(red, green, blue);
            }
            catch (System.FormatException)
            {
                return;
            }

            foreach (SocketRole r in user.Roles)
            {
                if (r.Name == "colorrole:" + hex)
                {
                    await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed($"{colour}me Failed", $"You already have the {colour.ToLower()} " + hex + "!", color, Context));
                    return;
                }
                else if (r.Name.StartsWith("colorrole:"))
                {
                    if (r.Members.Count() > 1)
                    {
                        await user.RemoveRoleAsync(r);
                    }
                    else
                    {
                        await r.DeleteAsync();
                    }
                }
            }

            if (colorFound)
            {
                await user.AddRoleAsync(role);

                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed($"{colour}me for " + Context.User.Username, "You have been " + colour.ToLower() + "ed " + hex + "!", color, Context));
            }
            else
            {
                role = await Context.Guild.CreateRoleAsync("colorrole:" + hex, color: color);
                await user.AddRoleAsync(role);
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed($"{colour}me for " + Context.User.Username, "You have been " + colour.ToLower() + "ed " + hex + "!", color, Context));
            }
        }
    }
}
