using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace AltimitBot.Modules
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command("clean")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task clean(int count)
        {
            if (count <= 100 && !(count <= 0))
            {
                var messages = await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Clean channel", "Deleted " + count + " messages.",
                    time: 5000);
            }
            else
            {
                await Context.Channel.DeleteMessageAsync(Context.Message);
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Clean channel", "Count either over 100, blank or invalid(negative), please try your query again...",
                    time: 10000);
            }
        }

        [Command("status")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task status(int type, [Remainder]string statusMsg = "")
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            await AltimitBot.Program._client.SetGameAsync(statusMsg, type: (ActivityType)type);
            await Misc.EmbedWriter(Context.Channel, Context.User,
                "Change Status",
                "Set BOT status to " + (ActivityType)type + statusMsg,
                time: 10000);
        }
    }
}
