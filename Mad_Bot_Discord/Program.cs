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
            await Task.Delay(-1);
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
