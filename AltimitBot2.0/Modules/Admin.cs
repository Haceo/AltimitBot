﻿using System;
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
        public static MainWindow _this;
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
        [Command("clear", RunMode = RunMode.Async)]
        [RequireOwner]
        private async Task clear()
        {
            _this.ConsoleString = "I'm still alive!" + Environment.NewLine;
        }
        [Command("status", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task status(int type = -1, [Remainder]string statusMsg = "")
        {
            if (type == -1)
            {
                await MainWindow._client.SetGameAsync("");
                return;
            }
            await Context.Channel.DeleteMessageAsync(Context.Message);
            await MainWindow._client.SetGameAsync(statusMsg, type: (ActivityType)type);
            await Misc.EmbedWriter(Context.Channel, Context.User,
                "Change Status",
                "Set BOT status to " + (ActivityType)type + " " + statusMsg,
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
        [Command("rolelist", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task rolelist([Remainder]string roles = "")
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            string outList = "";
            if (roles == "")
            {
                listallroles();
                return;
            }
            else
            {
                bool error = false;
                string[] roleList = roles.Split(new char[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (roleList.Length == 1)
                {
                    singlerole(roles);
                    return;
                }
                else
                {
                    foreach (var inputRole in roleList)
                    {
                        var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == inputRole.Trim()) ?? null;
                        if (role != null)
                        {
                            outList = outList + role.Name + ": " + role.Members.Count().ToString() + Environment.NewLine;
                        }
                        if (role == null)
                        {
                            outList = outList + inputRole + ": ERROR!" + Environment.NewLine;
                            error = true;
                        }
                    }
                    if (error)
                    {
                        outList = outList + Environment.NewLine + "Error! Some roles were not found or do not exist as written," + Environment.NewLine +
                            "Please check spelling and case and try again!";
                    }
                }
            }
            await Misc.EmbedWriter(Context.Channel, Context.User,
                "Role Counter",
                "Roles found matching search parameters:" + Environment.NewLine +
                outList);
        }
        public async Task listallroles()
        {
            string outList = "";
            foreach (var role in Context.Guild.Roles)
            {
                outList = outList + role.Name + ": " + role.Members.Count().ToString() + Environment.NewLine;
            }
            await Misc.EmbedWriter(Context.Channel, Context.User,
                "Role Counter",
                "Listing all roles:" + Environment.NewLine +
                outList);
        }
        public async Task singlerole(string role)
        {
            var foundRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == role);
            if (foundRole == null)
            {

                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Role Counter",
                    "No role found matching " + role);
                return;
            }
            await Misc.EmbedWriter(Context.Channel, Context.User,
                "Role Counter",
                "Role found matching search parameters:" + Environment.NewLine +
                foundRole.Name + ": " + foundRole.Members.Count().ToString());
        }
        [Command("rolemembers", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task rolemembers(string inputRole, int time = 30000)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var members = Context.Guild.Roles.FirstOrDefault(x => x.Name == inputRole).Members ?? null;
            if (members == null)
            {
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Error!",
                    "The role " + inputRole + " was not found or does not exist as typed, please try again.");
                return;
            }
            string memberList = "";
            foreach (var member in members)
            {
                memberList = memberList + member.Username + Environment.NewLine;
            }
            if (memberList == "")
            {
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Role Members:",
                    "The role " + inputRole + " contains zero members.");
                return;
            }
            await Misc.EmbedWriter(Context.Channel, Context.User,
                "Role Members:",
                "Role: " + inputRole + Environment.NewLine +
                "Returned members:" + Environment.NewLine +
                Environment.NewLine +
                memberList, time: time);
        }
    }
    public class RoleObject
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
