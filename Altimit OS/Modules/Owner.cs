using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Altimit_OS.Modules
{
    class Owner : ModuleBase<SocketCommandContext>
    {
        public static MainWindow _main;
        [Command("leave", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task leave(ulong serverId, bool full = true)
        {
            SocketGuild guild;
            guild = Context.Client.Guilds.FirstOrDefault(x => x.Id == serverId);
            DiscordServer server = _main.ServerList.FirstOrDefault(x => x.ServerId == guild.Id);
            if (full)
            {
                if (server.DOBChannel != 0)
                    await guild.Channels.FirstOrDefault(x => x.Id == server.DOBChannel).DeleteAsync();
                if (server.MemberRole != 0)
                    await guild.Roles.FirstOrDefault(x => x.Id == server.MemberRole).DeleteAsync();
                if (server.UnderageRole != 0)
                    await guild.Roles.FirstOrDefault(x => x.Id == server.UnderageRole).DeleteAsync();
                if (server.NewUserRole != 0)
                    await guild.Roles.FirstOrDefault(x => x.Id == server.NewUserRole).DeleteAsync();
                if (server.StreamingRole != 0)
                    await guild.Roles.FirstOrDefault(x => x.Id == server.StreamingRole).DeleteAsync();
                await guild.LeaveAsync();
                _main.ServerList.Remove(server);
            }
            else
                await guild.LeaveAsync();
            BotFrame.SaveFile("servers");
        }
    }
}
