using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;

namespace AltimitBot2._0
{
    class CommandHandler
    {
        public static MainWindow _this;
        DiscordSocketClient _client;
        CommandService _service;

        public async Task InitAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), services: null);
            _client.MessageReceived += HandleCommandAsync;
            _client.UserJoined += JoinHandlerAsync;
            _client.JoinedGuild += JoinGuild;
            _client.UserLeft += LeaveHandlerAsync;
            _client.ReactionAdded += ReactionAdded;
        }
        private async Task HandleCommandAsync(SocketMessage s)
        {
            SocketUserMessage msg = s as SocketUserMessage;
            if (msg == null)
                return;
            SocketCommandContext context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            var dobchanList = new List<string>();
            foreach (var server in BotConfig.serverData)
            {
                dobchanList.Add(server.dobChannel);
            }
            if (msg.HasStringPrefix(BotConfig.botConfig.cmdPrefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var res = await _service.ExecuteAsync(context, argPos, services: null);
                if (!res.IsSuccess && res.Error != CommandError.UnknownCommand)
                {
                    consoleOut("Error!!: " + res.ErrorReason);
                    return;
                }
                if (!res.IsSuccess && res.Error == CommandError.UnknownCommand)
                {
                    await msg.DeleteAsync();
                    consoleOut("Deleted message from user: " + context.User.Username + " within server: " + context.Guild.Name + " in channel: " + context.Channel.Name);
                    consoleOut("Message: " + msg.Content);
                    return;
                }
            }
            else if (dobchanList.Contains(msg.Channel.Name) && !context.User.IsBot)
            {
                await msg.DeleteAsync();
                consoleOut("Deleted message from user: " + context.User.Username + " within server: " + context.Guild.Name + " in channel: " + context.Channel.Name);
                consoleOut("Message: " + msg.Content);
                return;
            }
            else if (msg.Channel.Name == "owoified" && !context.User.IsBot)
            {
                await Task.Delay(200);
                await msg.DeleteAsync();
                Modules.OwOify.owoify(msg.Channel, context.User, msg.Content);
                return;
            }
        }

        private Task ReactionAdded(Discord.Cacheable<Discord.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            Task.Run(async () =>
            {
                await Reaction(arg1, arg2, arg3);
            });
            return Task.CompletedTask;
        }
        private async Task Reaction(Discord.Cacheable<Discord.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var gChan = (arg2 as SocketGuildChannel);
            foreach (var channel in BotConfig.LockList)
            {
                if (channel.Message == arg3.MessageId)
                {
                    if (arg3.Emote.ToString() == channel.Emote)
                    {
                        consoleOut("Adding role @" + channel.Role + " to user " + gChan.Guild.Users.FirstOrDefault(x => x.Id == arg3.UserId) + " in server " + gChan.Guild.Name);
                        await gChan.Guild.Users.FirstOrDefault(x => x.Id == arg3.UserId).AddRoleAsync(gChan.Guild.Roles.FirstOrDefault(y => y.Name == channel.Role));
                    }
                }
            }
        }
        private Task JoinHandlerAsync(SocketGuildUser u)
        {
            Task.Run(async () =>
            {
                await AddUser(u);
            });
            return Task.CompletedTask;
        }
        private async Task AddUser(SocketGuildUser u)
        {
            if (u == null)
                return;
            var role = BotConfig.serverData.FirstOrDefault(x => x.ServerId == u.Guild.Id).dobRole ?? null;
            if (role != null | role != "")
            {
                await u.AddRoleAsync(u.Guild.Roles.FirstOrDefault(x => x.Name == "Unverified"));
            }
        }
        private Task LeaveHandlerAsync(SocketGuildUser u)
        {
            Task.Run(async () =>
            {
                await UserLeave(u);
            });
            return Task.CompletedTask;
        }
        private async Task UserLeave(SocketGuildUser u)
        {
            if (u == null)
                return;
            var server = u.Guild;
            var channel = (server.Channels.FirstOrDefault(x => x.Name == "welcome") as ISocketMessageChannel);
            await Modules.Misc.EmbedWriter(channel, u, "User left", u.Username + " has left the server" + Environment.NewLine + Environment.NewLine + u.Id.ToString(), time: -1);
        }
        private Task JoinGuild(SocketGuild g)
        {
            Task.Run(async () =>
            {
                await AddGuild(g);
            });
            return Task.CompletedTask;
        }
        private async Task AddGuild(SocketGuild g)
        {
            if (g == null)
                return;
            var server = BotConfig.serverData.FirstOrDefault(x => x.ServerId == g.Id) ?? null;
            if (server == null)
            {
                ServerInfo newServer = new ServerInfo();
                newServer.ServerName = g.Name;
                newServer.ServerId = g.Id;
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    BotConfig.serverData.Add(newServer);
                    BotConfig.SaveServerData();
                });
            }
        }

        //VVV----copy to all modules----VVV
        public static void consoleOut(string msg)
        {

            _this.ConsoleString = _this.ConsoleString + DateTime.Now + ": " + msg + Environment.NewLine;
        }
    }
}
