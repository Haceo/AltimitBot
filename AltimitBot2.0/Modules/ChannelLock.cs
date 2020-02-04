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
        public async Task LockChannel(string emote, string role, [Remainder]string msg = "")
        {
            var cLock = BotConfig.LockList.FirstOrDefault(x => x.Channel == Context.Channel.Id && x.Server == Context.Guild.Id) ?? null;
            if (cLock != null)
            {
                await Misc.EmbedWriter(Context.Channel, Context.User, "Error!", "This channel has already been locked, please use a different channel!");
                return;
            }
            var checkRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == role) ?? null;
            if (checkRole == null)
            {
                await Misc.EmbedWriter(Context.Channel, Context.User, "Error!", "Role not found or does not exist as typed!");
                return;
            }
            if (msg == "")
                msg = "React with " + emote + " to gain role " + role;
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
            LockChannel channel = BotConfig.LockList.FirstOrDefault(x => x.Channel == Context.Channel.Id && x.Server == Context.Guild.Id) ?? null;
            if (channel == null)
            {
                await Misc.EmbedWriter(Context.Channel, Context.User, "Error!", "This channel does not contain a lock!");
                return;
            }
            ulong message = channel.Message;
            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Context.Channel.DeleteMessageAsync(messageId: message);
            BotConfig.LockList.Remove(new LockChannel { Message = message });
            BotConfig.SaveLocks();
        }
    }
}
