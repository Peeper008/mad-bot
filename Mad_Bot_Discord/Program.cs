using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Mad_Bot_Discord
{
    internal class Program
    {
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        private static void Main()
        => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(Config.bot.token))
            {
                Console.WriteLine("Please enter values in the config file.");
                Console.ReadLine();
            }

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });

            _client.Ready += ClientReady;
            _client.Log += Log;
            _client.ReactionAdded += OnReactionAdded;
            await _client.LoginAsync(TokenType.Bot, Config.bot.token);
            await _client.StartAsync();
            _handler = new CommandHandler();
            Global.Client = _client;
            await _handler.InitializeAsync(_client);
            ConsoleInput();
            await Task.Delay(-1);
        }

        private async Task ConsoleInput()
        {
            var input = string.Empty;
            while(input.Trim().ToLower() != "block")
            {
                input = Console.ReadLine();
                if (input != null && input.Trim().ToLower() == "message")
                    ConsoleSendMessage();
            }
        }

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

        private SocketGuild GetSelectedGuild(IEnumerable<SocketGuild> guilds)
        {
            var socketGuilds = guilds.ToList();
            var maxIndex = socketGuilds.Count - 1;
            for (var i = 0; i <= maxIndex; i++)
            {
                Console.WriteLine($"{i}. - {socketGuilds[i].Name}");
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
                Console.WriteLine($"{i}. - {textChannels[i].Name}");
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

        private async Task ClientReady()
        {
            await _client.SetGameAsync(String.Format(Config.bot.game, Config.bot.cmdPrefix));
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.MessageId == Global.MessageIdToTrack)
            {
                if (reaction.Emote.Name == "👌")
                {
                    await channel.SendMessageAsync(reaction.User.Value.Username + " says OK.");
                }
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }
    }
}
