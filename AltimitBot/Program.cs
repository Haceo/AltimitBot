using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Discord;

namespace AltimitBot
{
    class Program
    {
        public static DiscordSocketClient _client;
        CommandHandler _handler;
        static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();
        public async Task StartAsync()
        {
            Console.Clear();
            Console.WriteLine("Welcome to Altimit Bot");
            Console.WriteLine("Written by Haceo Misaki");
            Console.WriteLine("2019-11-05");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Searching for config...");
            BotConfig.Config();
            Console.WriteLine("");
            if (BotConfig.botConfig.token == "" | BotConfig.botConfig.token == null | BotConfig.botConfig.cmdPrefix == "" | BotConfig.botConfig.cmdPrefix == null)
            {
                BotConfig.Config();
            }
            Console.WriteLine("Searching for server and user data files...");
            BotConfig.LoadInfo();
            Console.WriteLine("Starting logging...");
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, BotConfig.botConfig.token);
            await _client.StartAsync();
            _handler = new CommandHandler();
            await _handler.InitAsync(_client);
            await Task.Delay(-1);
        }
        private async Task Log(LogMessage msg)
        {
            Console.WriteLine(DateTime.Now + ": " + msg.Message);
            return;
        }
    }
}
