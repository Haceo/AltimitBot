using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace AltimitBot2._0.Modules
{
    public class ChannelLock : ModuleBase<SocketCommandContext>
    {
        [Command("lock", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task LockChannel(string emote, string role, string msg)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            LockChannel newLock = new LockChannel();
            newLock.Channel = Context.Channel.Id;
            newLock.Emote = emote.ToString();
            newLock.Server = Context.Guild.Id;
            newLock.Role = role;
            ulong message = await Misc.EmbedWriter(Context.Channel, Context.User,
                "Lockout",
                msg, time: -1);
            newLock.Message = message;
            BotConfig.LockList.Add(newLock);
            BotConfig.SaveLocks();
        }
        [Command("unlock", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task UnlockChannel()
        {
            var message = BotConfig.LockList.FirstOrDefault(x => x.Channel == Context.Channel.Id && x.Server == Context.Guild.Id).Message;
            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Context.Channel.DeleteMessageAsync(messageId: message);
            BotConfig.LockList.Remove(new LockChannel { Message = message });
            BotConfig.SaveLocks();
        }
    }
}
