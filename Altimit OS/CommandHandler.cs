using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Altimit_OS
{
    class CommandHandler
    {
        public static MainWindow _main;
        DiscordSocketClient _client;
        CommandService _service;
        public async Task InitAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            _client.JoinedGuild += JoinedGuildHandler;
            _client.UserJoined += UserJoinedHandler;
            _client.UserLeft += UserLeaveHandler;
            _client.ReactionAdded += ReactionAddedHandler;
            _client.MessageReceived += MessageReceivedHandler;
            _client.UserBanned += UserBannedHandler;
            _client.UserUpdated += UserUpdatedHandler;
        }
        private async Task JoinedGuildHandler(SocketGuild guild)
        {
            DiscordServer saved = _main.ServerList.FirstOrDefault(x => x.ServerId == guild.Id);
            if (saved != null)
                saved.Active = true;
            else
            {
                DiscordServer newServer = new DiscordServer()
                {
                    Active = true,
                    ServerName = guild.Name,
                    ServerId = guild.Id,
                    Prefix = PrefixChar.None,
                    ServerJoined = DateTime.Now.ToString()
                };
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    _main.ServerList.Add(newServer);
                });
            }
            BotFrame.SaveFile("servers");
            _main.ServerList.Clear();
            BotFrame.LoadFile("servers");
        }
        private async Task UserJoinedHandler(SocketGuildUser u)
        {
            if (u == null)
                return;
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == u.Guild.Id);
            if (u.IsBot)
            {
                BotFrame.consoleOut($"User {u} is a bot!");
                if (server.AdminRole != 0 && server.AdminChannel != 0)
                {
                    var adminRole = u.Guild.Roles.FirstOrDefault(x => x.Id == server.AdminRole);
                    BotFrame.EmbedWriter(u.Guild.Channels.FirstOrDefault(x => x.Id == server.AdminChannel) as ISocketMessageChannel, u,
                        "Altimit",
                        $"{adminRole.Mention} User {u} is a bot!",
                        time: -1);
                }
                return;
            }
            if (server.NewUserRole != 0)
            {
                try
                {
                    await u.AddRoleAsync(u.Guild.Roles.FirstOrDefault(x => x.Id == server.NewUserRole));
                }
                catch (Exception ex)
                {
                    BotFrame.consoleOut(ex.Message);
                    return;
                }
            }
            if (server.WelcomeChannel != 0 && server.UseWelcomeForDob)
                await (u.Guild.Channels.FirstOrDefault(x => x.Id == server.WelcomeChannel) as ISocketMessageChannel).SendMessageAsync($"Hey {u.Mention}, welcome to **{server.ServerName}**. Please read the rules and enter your date of birth in the {u.Guild.Channels.FirstOrDefault(y => y.Id == server.DOBChannel)}. You must be 18+");
        }
        private async Task UserUpdatedHandler(SocketUser newUser, SocketUser oldUser)
        {
            var guilds = newUser.MutualGuilds;
            if (newUser.ToString() != oldUser.ToString())
                foreach (var guild in guilds)
                {
                    var server = _main.ServerList.FirstOrDefault(x => x.ServerId == guild.Id);
                    if (server.AdminChannel != 0 && server.UserUpdate)
                    {
                        var adminChan = guild.Channels.FirstOrDefault(x => x.Id == server.AdminChannel) as ISocketMessageChannel;
                        BotFrame.EmbedWriter(adminChan, newUser,
                            "Altimit",
                            $"User:{Environment.NewLine}{oldUser}{Environment.NewLine}has changed their name to{Environment.NewLine}{newUser}", time: -1);
                    }
                }
        }
        private async Task UserLeaveHandler(SocketGuildUser u)
        {
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == u.Guild.Id);
            var chan = u.Guild.Channels.FirstOrDefault(x => x.Id == server.WelcomeChannel) as ISocketMessageChannel;
            if (server.WelcomeChannel != 0 && server.UseWelcomeForLeave)
                await BotFrame.EmbedWriter(chan, u,
                    "Altimit",
                    $"{u.Mention} has left the server",
                    time: -1);
        }
        private async Task UserBannedHandler(SocketUser user, SocketGuild guild)
        {
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == guild.Id);
            var blacklistChannel = guild.Channels.FirstOrDefault(x => x.Id == server.BlacklistChannel) as ISocketMessageChannel;
            if (blacklistChannel == null && server.UseBlacklist)
            {
                BotFrame.consoleOut("No blacklist channel set up");
                return;
            }
            else if (!server.UseBlacklist)
                return;
            var ban = guild.GetBanAsync(user).Result;
            BotFrame.EmbedWriter(blacklistChannel, user,
                "Altimit Blacklist",
                $"User {user.Mention} has been banned{Environment.NewLine}" +
                $"Reason:{Environment.NewLine}" +
                $"{ban.Reason}", time: -1);
        }
        private async Task ReactionAddedHandler(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            SocketGuildChannel guildChannel = channel as SocketGuildChannel;
            SocketGuildUser user = guildChannel.Guild.Users.FirstOrDefault(x => x.Id == reaction.UserId);
            DiscordServer server = _main.ServerList.FirstOrDefault(x => x.ServerId == guildChannel.Guild.Id);
            if (server == null || user == null)
                return;
            //-----Reaction Locks-------------------------------------------------------------------------------------------
            foreach (var reactionLock in server.ReactionLockList.Where(x => x.Channel == channel.Id && x.Message == message.Id))
            {
                if (reaction.Emote.ToString() == reactionLock.Emote)
                {
                    var guildRole = guildChannel.Guild.Roles.FirstOrDefault(x => x.Id == reactionLock.Role);
                    if (guildRole == null)
                        return;
                    if (!user.Roles.Contains(guildRole))
                    {
                        BotFrame.consoleOut($"Adding role @{guildRole} to user {user} in server {guildChannel.Guild.Name}");
                        await user.AddRoleAsync(guildRole);
                    }
                }
            }
        }
        private async Task MessageReceivedHandler(SocketMessage msg)
        {
            SocketUserMessage message = msg as SocketUserMessage;
            if (message == null)
                return;
            SocketCommandContext context = new SocketCommandContext(_client, message);
            if (context.User.IsBot)
                return;
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == context.Guild.Id);
            bool admin = false;
            var hasAdminRole = context.Guild.Users.FirstOrDefault(x => x.Id == context.User.Id).Roles.FirstOrDefault(y => y.Id == server.AdminRole);
            if (hasAdminRole != null && hasAdminRole.Id == server.AdminRole)
                admin = true;
            int argPos = 0;
            if (message.HasStringPrefix(((Char)server.Prefix).ToString(), ref argPos)
            || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                if ((server.BotChannel != 0 && msg.Channel.Id == server.BotChannel) || server.BotChannel == 0 || admin)
                {
                    var res = await _service.ExecuteAsync(context, argPos, null);
                    if (!res.IsSuccess && res.Error != CommandError.UnknownCommand)
                    {
                        BotFrame.consoleOut("Command Handler Error: " + res.ErrorReason);
                        return;
                    }
                    if (!res.IsSuccess && res.Error == CommandError.UnknownCommand)
                    {
                        await Task.Delay(200);
                        await message.DeleteAsync();
                        BotFrame.consoleOut("Unknown command! Deleted message from user: " + context.User.Username + " in channel " + context.Channel.Name + Environment.NewLine +
                            "Message: " + message.Content);
                        return;
                    }
                }
                else
                {
                    BotFrame.consoleOut($"Deleted message from user: {context.User} In channel: {context.Channel}{Environment.NewLine} Message: {context.Message.Content}");
                    return;
                }
            }
            else if (server.DOBChannel != 0 && msg.Channel.Id == server.DOBChannel)
            {
                await Task.Delay(200);
                await msg.DeleteAsync();
                try
                {
                    Modules.DateOfBirth.Submit(server, context);
                }
                catch (Exception ex)
                {
                    BotFrame.consoleOut(ex.Message);
                    return;
                }
                return;
            }
        }
    }
}
