using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Abot.Crawler;
using Abot.Poco;
using System.Net;
using Mad_Bot_Discord.Core.UserAccounts;

namespace Mad_Bot_Discord
{
    internal class Program
    {
        static private DiscordSocketClient _client;
        private CommandHandler _handler;
        static public PoliteWebCrawler crawler = new PoliteWebCrawler();

        private static void Main()
        => new Program().StartAsync().GetAwaiter().GetResult();

        // Startup Sequence
        public async Task StartAsync()
        {


            // Makes sure the config file is filled out before continuing.
            if (string.IsNullOrEmpty(Config.bot.token) || string.IsNullOrEmpty(Config.bot.cmdPrefix))
            {
                Console.WriteLine("Please enter values in the config file.");
                Console.ReadLine();
            }


            crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

            // Sets the LogLevel to Verbose.
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            
            // Once the client is ready, it runs the ClientReady method.
            _client.Ready += ClientReady;

            // Once a log is required, it runs the Log method.
            _client.Log += Log;

            // Once a user joins any guild, it runs the UserJoined method.
            _client.UserJoined += UserJoined;

            // Once a user leaves any guild, it runs the UserLeft method.
            _client.UserLeft += UserLeft;


            // Logs in the bot with the token provided in the config file generated once the bot is initially run.
            await _client.LoginAsync(TokenType.Bot, Config.bot.token);

            // Starts the client.
            await _client.StartAsync();

            _handler = new CommandHandler();
            Global.Client = _client;
            await _handler.InitializeAsync(_client);

            // Allows console commands to be used.
            ConsoleInput();

            await Task.Delay(-1);
            
        }


        private void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }

        private void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        private void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

            if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                Console.WriteLine("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri);
            else
                Console.WriteLine("Crawl of page succeeded {0}", crawledPage.Uri.AbsoluteUri);

            if (string.IsNullOrEmpty(crawledPage.Content.Text))
                Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);

            var htmlAgilityPackDocument = crawledPage.HtmlDocument; // HTML Agility Pack Parser
            var angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument; // AngleSharp Parser
        }

        private void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            // Gets the page it's going to crawl and logs it
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("About to crawl link {0} which was found on page {1}", 
                pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }


        // Happens every time a user leaves.
        private async Task UserLeft(SocketGuildUser user)
        {
            // Gets the guild and puts it into a variable.
            SocketGuild guild = user.Guild;

            // Gets the welcome channel and puts that into a variable.
            SocketTextChannel channel = guild.GetChannel(432584892522561559) as SocketTextChannel;

            // Creates the embed for fancy text.
            EmbedBuilder embed = new EmbedBuilder().WithTitle("User Left!")
                .WithDescription($"{user.Username}#{user.Discriminator} has left the server!")
                .WithThumbnailUrl(user.GetAvatarUrl()).WithColor(Utilities.GetColor());

            // Sends the leaving message.
            await channel.SendMessageAsync("", embed: embed);
        }


        // Happens every time a user joins.
        private async Task UserJoined(SocketGuildUser u)
        {
            // Gets the starting role and puts it in a variable.
            SocketRole role = u.Guild.GetRole(352208507581497347);
            
            // Loops through all the user's roles to check if they already have the starting role, and if they do, returns.
            foreach (IRole r in u.Roles)
            {
                if (r.Id == role.Id) return;
            }

            // Adds the starting role if they don't have it already.
            await u.AddRoleAsync(role);
            
        }





        // --- Console Input ---

        private async Task ConsoleInput()
        {
            // Creates an empty string.
            var input = string.Empty;

            // Keeps looping the block of code unless the input string is "block."
            while(input.Trim().ToLower() != "block")
            {
                // Gets the user's input and puts it into the input variable.
                input = Console.ReadLine();

                // Checks if the input string isn't null and that it says "message."

                switch (input.Trim().ToLower())
                {
                    case "message":
                        ConsoleSendMessage();
                        break;
                    case "ban":
                        ConsoleBanUser();
                        break;
                    case "kick":
                        ConsoleKickUser();
                        break;
                }
            }
        }

        // --- Console Input ---


        // --- Console Commands ---

        private async void ConsoleSendMessage()
        {
            Console.WriteLine("Select the guild:");
            var guild = GetSelectedGuild(_client.Guilds);

            if (guild == null)
            {
                Console.WriteLine("Cancelled.");
                return;
            }
                

            var textChannel = GetSelectedTextChannel(guild.TextChannels);

            if (textChannel == null)
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            var msg = string.Empty;

            while (string.IsNullOrEmpty((msg)))
            {
                Console.WriteLine("Your message:");
                msg = Console.ReadLine();
            }

            try
            {
                await textChannel.SendMessageAsync(msg);
            }
            catch
            {
                Console.WriteLine("Cannot message that channel because you don't have permission.");
            }
            
        }

        private async void ConsoleBanUser()
        {
            Console.WriteLine("Select the guild:");

            // Asks the user to select from a list of available guilds.
            var guild = GetSelectedGuild(_client.Guilds);

            // Tests to see if the guild is 'null' (it is set to null if the user cancels the request mid-completion)
            if (guild == null)
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            Console.WriteLine("Select the user:");

            // Asks the user to select from a list of available users.
            var user = GetSelectedUser(guild.Users);

            // Test to see if the user is 'null.'
            if (user == null)
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            Console.WriteLine("Reason:");
            string msg = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(msg))
            {
                await user.SendMessageAsync("You have been banned from " + guild.Name + ". Reason: " + msg);
            }
            else
            {
                await user.SendMessageAsync("You have been banned from " + guild.Name + ".");
            }

            await guild.AddBanAsync(user, reason:msg);

            var embed = new EmbedBuilder();

            SocketTextChannel channel = (SocketTextChannel) guild.GetChannel(432584892522561559);

            if (channel == null) return;

            embed.WithTitle("Banned by Console")
                .WithDescription($"{user.Username}#{user.Discriminator} has been banned.")
                .WithFooter(x =>
                {
                    x.Text = $"Console at {DateTime.UtcNow}";
                    x.IconUrl = "https://cdn2.iconfinder.com/data/icons/metro-ui-dock/512/Command_Prompt.png";
                })
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithColor(Utilities.GetColor());
            
            await channel.SendMessageAsync("", embed: embed);
        }

        private async void ConsoleKickUser()
        {
            Console.WriteLine("Select the guild:");

            // Asks the user to select from a list of available guilds.
            var guild = GetSelectedGuild(_client.Guilds);

            // Tests to see if the guild is 'null' (it is set to null if the user cancels the request mid-completion)
            if (guild == null)
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            Console.WriteLine("Select the user:");

            // Asks the user to select from a list of available users.
            var user = GetSelectedUser(guild.Users);

            // Test to see if the user is 'null.'
            if (user == null)
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            Console.WriteLine("Reason:");
            string msg = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(msg))
            {
                await user.SendMessageAsync("You have been kicked from " + guild.Name + ". Reason: " + msg);
            }
            else
            {
                await user.SendMessageAsync("You have been kicked from " + guild.Name + ".");
            }

            await user.KickAsync(msg);

            var embed = new EmbedBuilder();

            SocketTextChannel channel = (SocketTextChannel)guild.GetChannel(432584892522561559);

            if (channel == null) return;

            embed.WithTitle("Kicked by Console")
                .WithDescription($"{user.Username}#{user.Discriminator} has been kicked.")
                .WithFooter(x =>
                {
                    x.Text = $"Console at {DateTime.UtcNow}";
                    x.IconUrl = "https://cdn2.iconfinder.com/data/icons/metro-ui-dock/512/Command_Prompt.png";
                })
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithColor(Utilities.GetColor());

            await channel.SendMessageAsync("", embed: embed);
        }

        // --- Console Commands ---


        // --- Console Command Requirements ---

        private SocketGuild GetSelectedGuild(IEnumerable<SocketGuild> guilds)
        {
            var socketGuilds = guilds.ToList();
            var maxIndex = socketGuilds.Count - 1;
            for (var i = 0; i <= maxIndex; i++)
            {
                Console.WriteLine($"{i} - {socketGuilds[i].Name}");
            }
            Console.WriteLine("Type 'stop' or 'break' to cancel.");

            var selectedIndex = -1;
            while(selectedIndex < 0 || selectedIndex > maxIndex)
            {
                var input = Console.ReadLine().Trim();

                if (input == "break" || input == "stop")
                    return null;

                var success = int.TryParse(input, out selectedIndex);
                if (!success)
                {
                    Console.WriteLine("That was an invalid index, try again.");
                    selectedIndex = -1;
                }
                if (selectedIndex > maxIndex) Console.WriteLine($"{selectedIndex} is too large, the max index is {maxIndex}.");
                if (selectedIndex < 0) Console.WriteLine($"{selectedIndex} must not be negative.");
            }

            return socketGuilds[selectedIndex];
        }

        private SocketTextChannel GetSelectedTextChannel(IEnumerable<SocketTextChannel> channels)
        {
            var textChannels = channels.ToList();
            var maxIndex = textChannels.Count - 1;
            for (var i = 0; i <= maxIndex; i++)
            {
                Console.WriteLine($"{i} - {textChannels[i].Name}");
            }
            Console.WriteLine("Type 'stop' or 'break' to cancel.");

            var selectedIndex = -1;
            while (selectedIndex < 0 || selectedIndex > maxIndex)
            {
                var input = Console.ReadLine().Trim();

                if (input == "break" || input == "stop")
                    return null;

                var success = int.TryParse(input, out selectedIndex);
                if (!success)
                {
                    Console.WriteLine("That was an invalid index, try again.");
                    selectedIndex = -1;
                }

                if (selectedIndex > maxIndex) Console.WriteLine($"{selectedIndex} is too large, the max index is {maxIndex}.");
                if (selectedIndex < 0) Console.WriteLine($"{selectedIndex} must not be negative.");
            }

            return textChannels[selectedIndex];
        }

        private SocketGuildUser GetSelectedUser(IEnumerable<SocketGuildUser> users)
        {
            // Puts a list of all users into a variable.
            var socketUsers = users.ToList();

            // Sets the largest number you can select.
            var maxIndex = socketUsers.Count - 1;

            // Loops through each user and creates a list of all users to select from.
            for (var i = 0; i <= maxIndex; i++)
            {
                // Prints the current line.
                Console.WriteLine($"{i} - {socketUsers[i].Username}#{socketUsers[i].Discriminator}");
            }
            Console.WriteLine("Type 'stop' or 'break' to cancel.");

            // Creates a variable to represent your selection and sets it to -1 to start.
            var selectedIndex = -1;

            while (selectedIndex < 0 || selectedIndex > maxIndex)
            {
                // Creates a variable and sets the user's input to that variable.
                var input = Console.ReadLine().Trim();

                if (input == "break" || input == "stop")
                    return null;

                // Tries to parse the input and sets the output to the success variable.
                var success = int.TryParse(input, out selectedIndex);

                if (!success)
                {
                    Console.WriteLine("That was an invalid index, try again.");
                    selectedIndex = -1;
                }
                if (selectedIndex > maxIndex) Console.WriteLine($"{selectedIndex} is too large, the max index is {maxIndex}.");
                if (selectedIndex < 0) Console.WriteLine($"{selectedIndex} must not be negative.");
            }

            // Gives the answer.
            return socketUsers[selectedIndex];
        }

        // --- Console Command Requirements ---

       


        // --- Error Logger ---

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        // --- Error Logger ---


        public static async void RefreshGame()
        {
            await _client.SetGameAsync(String.Format(Config.bot.game, Config.bot.cmdPrefix));
        }

        // Starts once the client is ready.
        private async Task ClientReady()
        {
            // Sets the bot's game.
            await _client.SetGameAsync(String.Format(Config.bot.game, Config.bot.cmdPrefix));
        }
    }
}
