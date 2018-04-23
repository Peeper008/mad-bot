using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Mad_Bot_Discord
{
    class Program
    {
        DiscordSocketClient _client;
        CommandHandler _handler;

        static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (Config.bot.token == "" || Config.bot.token == null)
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
                if (input.Trim().ToLower() == "message")
                    ConsoleSendMessage();
            }
        }

        private async void ConsoleSendMessage()
        {
            Console.WriteLine("Select the guild:");
            var guild = GetSelectedGuild(_client.Guilds);
            var textChannel = GetSelectedTextChannel(guild.TextChannels);
            var msg = string.Empty;

            while (msg.Trim() == string.Empty)
            {
                Console.WriteLine("Your message:");
                msg = Console.ReadLine();
            }

            await textChannel.SendMessageAsync(msg);
        }

        private SocketGuild GetSelectedGuild(IEnumerable<SocketGuild> guilds)
        {
            var socketGuilds = guilds.ToList();
            var maxIndex = socketGuilds.Count - 1;
            for (var i = 0; i <= maxIndex; i++)
            {
                Console.WriteLine($"{i}. - {socketGuilds[i].Name}");
            }

            var selectedIndex = -1;
            while(selectedIndex < 0 || selectedIndex > maxIndex)
            {
                var success = int.TryParse(Console.ReadLine().Trim(), out selectedIndex);
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

            var selectedIndex = -1;
            while (selectedIndex < 0 || selectedIndex > maxIndex)
            {
                var success = int.TryParse(Console.ReadLine().Trim(), out selectedIndex);
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
