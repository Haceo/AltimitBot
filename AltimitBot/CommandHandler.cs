using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;

namespace AltimitBot
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _service;

        public async Task InitAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), services: null);
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            SocketUserMessage msg = s as SocketUserMessage;
            if (msg == null)
                return;
            SocketCommandContext context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            if (msg.HasStringPrefix(BotConfig.botConfig.cmdPrefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var res = await _service.ExecuteAsync(context, argPos, services: null);
                if (!res.IsSuccess && res.Error != CommandError.UnknownCommand)
                {
                    Console.WriteLine("Error!!: " + res.ErrorReason);
                }
            }
        }
    }
}
