using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace Altimit_OS.Modules
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        public static MainWindow _main;
        [Command("lookup", RunMode = RunMode.Async)]
        public async Task lookup(string searchBy, string info = "", int time = 20000)
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
            switch (searchBy.ToLower())
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
        //Add edit mode.?
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
            string[] roleSplit = null;
            if (roles != "")
                roleSplit = roles.Split(',');
            switch (option.ToLower())
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
                            outCount = $"No role found matching this name or ID!";
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
                    DateTimeOffset today = new DateTimeOffset(DateTime.Now);
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
                                members += $"{member} {today.Subtract((DateTimeOffset)member.JoinedAt).Days} days ago.{Environment.NewLine}";
                        }
                        if (members == "")
                            members = "No members found with this role!";
                        membersString = $"{membersString}{outRole}:{Environment.NewLine}{members}{Environment.NewLine}";
                    }
                    if (membersString.Length < 2048)
                        await BotFrame.EmbedWriter(Context.Channel, Context.User,
                            "Altimit Admin",
                            membersString);
                    else
                    {
                        File.WriteAllText("RoleMembers.txt", membersString);
                        BotFrame.EmbedWriter(Context.Channel, Context.User,
                            "Altimit Admin",
                            $"List too large, outputting to file below!{Environment.NewLine}Note: Roles will be in ID format.");
                        await Context.Channel.SendFileAsync("RoleMembers.txt");
                        File.Delete("RoleMembers.txt");
                    }
                    break;
                case "none":
                    string outNone = "";
                    foreach (var user in Context.Guild.Users.Where(x => x.Roles.Count == 1))
                        outNone += $"{user} Joined: {user.JoinedAt}";
                    if (outNone == "")
                    {
                        await BotFrame.EmbedWriter(Context.Channel, Context.User,
                            "Altimit Admin",
                            $"Sorry no users found without any roles.");
                        return;
                    }
                    await BotFrame.EmbedWriter(Context.Channel, Context.User,
                            "Altimit Admin",
                            $"Users found with no role:{Environment.NewLine}{outNone}");
                    break;
            }
        }
        [Command("stats", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Stats(string input, int time = -1)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            ulong userid;
            SocketGuildUser guildUser;
            var res = ulong.TryParse(input, out userid);
            if (res)
                guildUser = Context.Guild.Users.FirstOrDefault(x => x.Id == userid);
            else if (!input.Contains('#'))
                guildUser = Context.Guild.Users.FirstOrDefault(x => x.Username.ToLower() == input.ToLower());
            else
                guildUser = Context.Guild.Users.FirstOrDefault(x => x.ToString().ToLower() == input.ToLower());
            if (guildUser == null)
            {
                BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Admin",
                    $"User matching input: {input} was not found or does not exist as written.");
                return;
            }
            DateTimeOffset today = new DateTimeOffset(DateTime.Now);
            string roleString = "";
            foreach (SocketRole role in guildUser.Roles.Where(x => !x.IsEveryone))
                roleString += $"{role.Mention}{Environment.NewLine}";
            BotFrame.EmbedWriter(Context.Channel, Context.User,
                "Altimit Admin",
                $"User {guildUser.Mention} found!{Environment.NewLine}" +
                $"Discord stats:{Environment.NewLine}" +
                $"Discord acc Date: {guildUser.CreatedAt}" +
                $"Time in server: {today.Subtract((DateTimeOffset)guildUser.JoinedAt).Days} days{Environment.NewLine}" +
                $"Roles:{Environment.NewLine}" +
                $"{roleString}", time: time);
        }
        [Command("prune", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Prune(int days, string role = "")
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            DateTimeOffset today = new DateTimeOffset(DateTime.Now);
            SocketRole rawRole = null;
            if (role == "")
                rawRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == server.NewUserRole);
            else
                rawRole = await ParseRole(Context, role);
            if (rawRole == null)
                return;
            foreach (var user in Context.Guild.Users.Where(x => x.Roles.Count == 1 && x.Roles.Contains(rawRole) && today.Subtract((DateTimeOffset)x.JoinedAt).Days >= days))
            {
                string invite = null;
                RestInviteMetadata res = null;
                try
                {
                    res = await Context.Guild.GetVanityInviteAsync();
                }
                catch (Exception ex)
                {
                    res = null;
                }
                if (res != null)
                    invite = res.Url;
                else
                {
                    IReadOnlyCollection<RestInviteMetadata> invites;
                    try
                    {
                        invites = await Context.Guild.GetInvitesAsync();
                    }
                    catch (Exception ex)
                    {
                        invites = null;
                    }
                    if (invites != null)
                        invite = invites.FirstOrDefault(x => !x.IsRevoked && !x.IsTemporary && x.Uses < x.MaxUses).Url;
                    else
                        invite = "No invite found";
                }    
                int count = today.Subtract((DateTimeOffset)user.JoinedAt).Days;
                await user.SendMessageAsync($"We're sorry but you have been kicked from {Context.Guild.Name} as you have been unverified for {count} days.{Environment.NewLine}" +
                    $"If you would like to rejoin in the future and submit for verification you are welcome at {invite}");
                await user.KickAsync();
                await Task.Delay(5000);
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
        [Command("blank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task FindBlanks(int length = 0)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            string userOut = "";
            foreach (var user in Context.Guild.Users.Where(x => x.Username.Length <= length))
                userOut = $"{user.Mention} - {user} - {user.Id}{Environment.NewLine}";
            if (userOut != "" && userOut.Length < 2056)
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Admin",
                    $"Users found with names {length} chars or less:{Environment.NewLine}" +
                    $"{userOut}", time: -1);
            else if (userOut != "" && userOut.Length > 2056)
            {
                File.WriteAllText("BlankUsers.txt", userOut);
                BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Admin",
                    $"Sorry list was to long, outputting as file instead...");
                await Context.Channel.SendFileAsync("BlankUsers.txt");
                File.Delete("BlankUsers.txt");
            }
            else
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Admin",
                    $"Sorry no users found with names {length} chars or less...");
        }
        [Command("emoteid", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task GetEmoteId(string emote)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            BotFrame.consoleOut($"{emote}");
        }
    }
}
