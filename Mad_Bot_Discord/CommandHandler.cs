using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Mad_Bot_Discord.Core.LevelingSystem;
using Mad_Bot_Discord.Core.UserAccounts;

namespace Mad_Bot_Discord
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _service;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;

            if (msg == null) return;

            var context = new SocketCommandContext(_client, msg);
            if (context.User.IsBot) return;
            int argPos = 0;

            // Mute Check
            var userAccount = UserAccounts.GetAccount(context.User);
            if (userAccount.IsMuted)
            {
                await context.Message.DeleteAsync();
                return;
            }

            // Leveling Up
            if (!context.Message.HasStringPrefix(Config.bot.cmdPrefix, ref argPos, StringComparison.OrdinalIgnoreCase)
                && !msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                Leveling.UserSentMessage((SocketGuildUser)context.User, (SocketTextChannel)context.Channel);

            if (msg.HasStringPrefix(Config.bot.cmdPrefix, ref argPos, StringComparison.OrdinalIgnoreCase)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }

            /*
            //._. .-.
            if (!context.Message.HasStringPrefix(Config.bot.cmdPrefix, ref argPos, StringComparison.OrdinalIgnoreCase)
                && !msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                string content = msg.Content.Trim();
                content = content.Replace(" ", string.Empty);

                if (msg.Author.Id != 339071846651527169) return;

                for (int i = 0; i < msg.Content.Length; i++)
                {
                    if (content[i] == '.')
                    {
                        for (int e = 1 + i; e < msg.Content.Length; e++)
                        {
                            if (content[e] == '-') continue;

                            else if (content[e] == '_') continue;

                            else if (content[e] == '.') break;

                            else
                            {
                                return;
                            }
                        }

                        await msg.DeleteAsync();
                    }
                }
            }
            */
        }
    }
}
