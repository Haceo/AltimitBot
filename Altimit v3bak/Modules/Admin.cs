﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Altimit_v3.Modules
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        public static MainWindow _main;
        [Command("lookup", RunMode = RunMode.Async)]
        private async Task lookup(string searchBy, string info = "", int time = 20000)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            var user = Context.Guild.Users.FirstOrDefault(x => x.Id == Context.User.Id);
            SocketRole hasAdmin = null;
            if (server.AdminRole != 0)
                hasAdmin = user.Roles.FirstOrDefault(x => x.Id == server.AdminRole);
            if (hasAdmin == null || Context.Channel.Id != server.AdminChannel)
                return;
            List<UserInfo> outputList = new List<UserInfo>();
            Regex rx = new Regex(@"([12][09][0-9][0-9])\W(1[0-2]|0[1-9]|[1-9])\W(3[01]|2[0-9]|1[0-9]|0[1-9]|[1-9])");//format G1(YYYY)G2(MM)G3(DD)
            switch (searchBy)
            {
                case "userid":
                    foreach (var data in server.UserInfoList.Where(x => x.UserId == ulong.Parse(info)))
                        outputList.Add(data);
                    break;
                case "submitted":
                    MatchCollection subMatch = rx.Matches(info.Trim());
                    if (subMatch.Count == 0)
                        return;
                    DateTime submitted = new DateTime(int.Parse(subMatch[0].Groups[1].ToString()), int.Parse(subMatch[0].Groups[2].ToString()), int.Parse(subMatch[0].Groups[3].ToString()));
                    foreach (var data in server.UserInfoList.Where(x => x.Submitted == submitted))
                        outputList.Add(data);
                    break;
                case "birthday":
                    MatchCollection birthMatch = rx.Matches(info.Trim());
                    if (birthMatch.Count == 0)
                        return;
                    DateTime birthday = new DateTime(int.Parse(birthMatch[0].Groups[1].ToString()), int.Parse(birthMatch[0].Groups[2].ToString()), int.Parse(birthMatch[0].Groups[3].ToString()));
                    foreach (var data in server.UserInfoList.Where(x => x.Birthday == birthday))
                        outputList.Add(data);
                    break;
                case "flagged":
                    foreach (var data in server.UserInfoList.Where(x => x.Flagged == true))
                        outputList.Add(data);
                    break;
                case "status":
                    UserStatus userStatus = (UserStatus)Enum.Parse(typeof(UserStatus), info, true);
                    foreach (var data in server.UserInfoList.Where(x => x.Status == userStatus))
                        outputList.Add(data);
                    break;
            }
            string outputString = "";
            foreach (var userInfo in outputList)
            {
                outputString = $"{outputString}Username: {userInfo.UserName}{Environment.NewLine}" +
                    $"User ID: {userInfo.UserId}{Environment.NewLine}" +
                    $"Birthday: {userInfo.Birthday}{Environment.NewLine}" +
                    $"Submitted: {userInfo.Submitted}{Environment.NewLine}" +
                    $"Flagged: {userInfo.Flagged}{Environment.NewLine}" +
                    $"Status: {userInfo.Status}{Environment.NewLine}{Environment.NewLine}";
            }
            await BotFrame.EmbedWriter(Context.Channel, Context.User,
                "Altimit Admin",
                $"User(s) found using info type {searchBy}: {info}:{Environment.NewLine}{outputString}",
                time: time);
        }

        [Command("clean", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ClearMessages(int count, ulong userId = 0, ITextChannel chan = null)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var user = Context.Guild.Users.FirstOrDefault(x => x.Id == userId);
            chan = chan ?? (ITextChannel)Context.Channel;
            bool loop = true;
            IEnumerable<IMessage> messages = null;
            if (count <= 100 && count >= 0)
            {
                List<ulong> remMsgs = new List<ulong>();
                while (loop)
                {
                    ulong last = 0;
                    if (messages != null)
                        last = messages.Last().Id;
                    if (last == 0)
                        messages = await chan.GetMessagesAsync().FlattenAsync();
                    else
                        messages = await chan.GetMessagesAsync(last, Direction.Before).FlattenAsync();
                    if (userId == 0)
                        foreach (var message in messages)
                        {
                            if (remMsgs.Count < count)
                                remMsgs.Add(message.Id);
                        }
                    else
                        foreach (var message in messages.Where(x => x.Author.Id == userId))
                            if (remMsgs.Count < count)
                                remMsgs.Add(message.Id);
                    if (remMsgs.Count() == count || messages.Count() != 100)
                        loop = false;
                }
                await chan.DeleteMessagesAsync(remMsgs);
                if (userId == 0)
                    await BotFrame.EmbedWriter(Context.Channel, Context.User,
                        "Altimit Admin",
                        $"Deleted {remMsgs.Count()} messages in channel {chan.Mention}", time: 5000);
                else
                    await BotFrame.EmbedWriter(Context.Channel, Context.User,
                        "Altimit Admin",
                        $"Found and deleted {remMsgs.Count} messages from {user} in channel {chan.Mention}", time: 5000);
                return;
            }
            else if (count > 100)
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Admin",
                    $"You have exceeded the message GET limit, you may only choose to delete 100 messages at a time!", time: 5000);
            else if (count <= 0)
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Admin",
                    $"Please select a positive integer!", time: 5000);
        }
        [Command("count", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task MessageCount(ulong userId = 0, ISocketMessageChannel channel = null)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            channel = channel ?? Context.Channel;
            int msgCount = 0;
            bool loop = true;
            IEnumerable<IMessage> messages = null;
            while (loop)
            {
                ulong last = 0;
                if (messages != null)
                    last = messages.Last().Id;
                if (last == 0)
                    messages = await channel.GetMessagesAsync().FlattenAsync();
                else
                    messages = await channel.GetMessagesAsync(last, Direction.Before).FlattenAsync();
                if (messages.Count() != 100)
                    loop = false;
                if (userId == 0)
                    msgCount += messages.Count();
                else
                    msgCount += messages.Where(x => x.Author.Id == userId).Count();
            }
            await BotFrame.EmbedWriter(Context.Channel, Context.User,
                "Altimit Admin",
                $"Message count: {msgCount}", time: 5000);
        }
        [Command("role", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Role(string option, [Remainder]string roles = "")
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            string[] roleSplit = roles.Split(',');
            switch (option)
            {
                case "count":
                    if (roles == "")
                    {
                        string outString = "";
                        foreach (SocketRole role in Context.Guild.Roles)
                            outString = $"{outString}{role.Mention}: {role.Members.Count()}{Environment.NewLine}";
                        await BotFrame.EmbedWriter(Context.Channel, Context.User,
                            "Altimit Admin",
                            outString);
                        return;
                    }
                    string countString = $"Listing role member count:{Environment.NewLine}";
                    foreach (var role in roleSplit)
                    {
                        string outRole = "";
                        string outCount = "";
                        var parseRole = await ParseRole(Context, role.Trim());
                        if (parseRole == null)
                        {
                            outRole = role.Trim();
                            outCount = $"No role found mathcing this name or ID!";
                        }
                        else
                        {
                            outRole = parseRole.Mention;
                            outCount = parseRole.Members.Count().ToString();
                        }
                        countString = $"{countString}{outRole}: {outCount}{Environment.NewLine}";
                    }
                    await BotFrame.EmbedWriter(Context.Channel, Context.User,
                        "Altimit Admin",
                        countString);
                    break;
                case "members":
                    if (roles == "")
                    {
                        await BotFrame.EmbedWriter(Context.Channel, Context.User,
                            "Altimit Admin",
                            $"You must specify at least one role to use role members!", time: 10000);
                        return;
                    }
                    string membersString = $"Listing role members:{Environment.NewLine}";
                    foreach (var role in roleSplit)
                    {
                        string members = "";
                        string outRole = "";
                        var parseRole = await ParseRole(Context, role.Trim());
                        if (parseRole == null)
                        {
                            outRole = role.Trim();
                            members = $"No role found with this name or ID!";
                        }
                        else
                        {
                            outRole = parseRole.Mention;
                            foreach (var member in parseRole.Members)
                                members = $"{members}{member}{Environment.NewLine}";
                        }
                        if (members == "")
                            members = "No members found with this role!";
                        membersString = $"{membersString}{outRole}:{Environment.NewLine}{members}{Environment.NewLine}";
                    }
                    await BotFrame.EmbedWriter(Context.Channel, Context.User,
                        "Altimit Admin",
                        membersString);
                    break;
            }
        }
        private async Task<SocketRole> ParseRole(SocketCommandContext context, string role)
        {
            if (role == "")
                return null;
            ulong foundRole;
            bool res = ulong.TryParse(role, out foundRole);
            if (res)
                return context.Guild.Roles.FirstOrDefault(x => x.Id == foundRole);
            else
                return context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == role.ToLower());
        }
    }
}