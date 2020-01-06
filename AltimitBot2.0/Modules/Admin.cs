using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace AltimitBot2._0.Modules
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command("clean", RunMode = RunMode.Async)]
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

        [Command("status", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task status(int type, [Remainder]string statusMsg = "")
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            await MainWindow._client.SetGameAsync(statusMsg, type: (ActivityType)type);
            await Misc.EmbedWriter(Context.Channel, Context.User,
                "Change Status",
                "Set BOT status to " + (ActivityType)type + statusMsg,
                time: 10000);
        }

        [Command("botchan", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task botchan(string Override = "", bool enable = false)
        {
            if (enable)
            {
                if (Override == "" | Override == null)
                {
                    await Misc.EmbedWriter(Context.Channel, Context.User,
                        "Error!!",
                        "No Override role specified, please enter a role as the first argument...",
                        time: 10000);
                }
                BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botChannel = Context.Channel.ToString();
                BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botRole = Override;
                await Context.Channel.DeleteMessageAsync(Context.Message);
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Bot Channel",
                    "Channel " + Context.Channel + " is now the only place non admin commands will be executed without admin access..." + Environment.NewLine +
                    "Role " + Override + " is being used to allow server wide non admin commands...",
                    time: -1);
            }
            else
            {
                string role = BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botRole;
                BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botChannel = "";
                BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botRole = "";
                await Context.Channel.DeleteMessageAsync(Context.Message);
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Bot Channel",
                    "Channel " + Context.Channel + " has been released from bot control." + Environment.NewLine +
                    "Role " + role + " has been released from bot control." + Environment.NewLine +
                    "WARNING!!! Non admin commands can be used anywhere in the server now without discretion!!");
            }
            BotConfig.SaveServerData();
        }
    }
}
