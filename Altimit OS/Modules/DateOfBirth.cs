using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;

namespace Altimit_OS.Modules
{
    public class DateOfBirth : ModuleBase<SocketCommandContext>
    {
        public static MainWindow _main;
        [Command("dobprime", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task DOBPrime()
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            if (server.NewUserRole == 0 || server.MemberRole == 0)
                return;//add error later
            var newRole = await ParseRole(Context, server.NewUserRole.ToString());
            var memberRole = await ParseRole(Context, server.MemberRole.ToString());
            foreach (var user in Context.Guild.Users.Where(x => !x.Roles.Contains(memberRole)))
                await user.AddRoleAsync(newRole);
        }
        [Command("dobcorrect", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task DOBCorrect()
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            if (server.UnderageRole == 0)
                return;//add error later
            var newRole = await ParseRole(Context, server.NewUserRole.ToString());
            var underAgeRole = await ParseRole(Context, server.UnderageRole.ToString());
            foreach (var underageUser in server.UserInfoList.Where(x => x.Status == UserStatus.Underage))
            {
                var guildUser = Context.Guild.Users.FirstOrDefault(x => x.Id == underageUser.UserId);
                await guildUser.AddRoleAsync(underAgeRole);
                await guildUser.RemoveRoleAsync(newRole);
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
        public static async Task Submit(DiscordServer server, SocketCommandContext context)
        {
            var adminChan = context.Guild.Channels.FirstOrDefault(x => x.Id == server.AdminChannel) as ISocketMessageChannel;
            var adminRole = context.Guild.Roles.FirstOrDefault(x => x.Id == server.AdminRole);
            var newUserRole = context.Guild.Roles.FirstOrDefault(x => x.Id == server.NewUserRole);
            var memberRole = context.Guild.Roles.FirstOrDefault(x => x.Id == server.MemberRole);
            var underageRole = context.Guild.Roles.FirstOrDefault(x => x.Id == server.UnderageRole);
            var guildUser = context.Guild.Users.FirstOrDefault(x => x.Id == context.User.Id);
            if (newUserRole != null && !guildUser.Roles.Contains(newUserRole))
                return;
            Regex rx = new Regex(@"([12][09][0-9][0-9])\W(1[0-2]|0[1-9]|[1-9])\W(3[01]|2[0-9]|1[0-9]|0[1-9]|[1-9])");//format G1(YYYY)G2(MM)G3(DD)
            MatchCollection matches = rx.Matches(context.Message.Content.Trim());
            if (matches.Count == 0)
            {
                BotFrame.consoleOut($"Deleted message from: {context.User} In channel: {context.Channel}{Environment.NewLine}" +
                    $"Message: {context.Message.Content}");
                await BotFrame.EmbedWriter(context.Channel, context.User,
                    "Altimit DOB",
                    "Check formatting and try again!" + Environment.NewLine +
                    "Format is: Year Month Day" + Environment.NewLine +
                    $"Example: {DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}", time: 10000);
                return;
            }
            DateTime birthday = new DateTime(
                int.Parse(matches[0].Groups[1].ToString()),
                int.Parse(matches[0].Groups[2].ToString()),
                int.Parse(matches[0].Groups[3].ToString()));
            DateTime today = DateTime.Now;
            if (server.UserInfoList == null)
                server.UserInfoList = new List<UserInfo>();
            var user = server.UserInfoList.FirstOrDefault(x => x.UserId == context.User.Id);
            if (user != null)
            {
                await BotFrame.EmbedWriter(adminChan, context.User,
                    "Altimit DOB",
                    $"{adminRole.Mention} A user is trying to enter DOB {context.Message} and already has an entry...{Environment.NewLine}" +
                    $"Username: {user.UserName}{Environment.NewLine}" +
                    $"User ID: {user.UserId}{Environment.NewLine}" +
                    $"Birthday: {user.Birthday}{Environment.NewLine}" +
                    $"Submited: {user.Submitted}{Environment.NewLine}" +
                    $"Flagged: {user.Flagged} Reason: {user.Status}", time: -1);
                if (user.Flagged || birthday != user.Birthday)
                {
                    string OutwardReason = "";
                    if (user.Flagged)
                        OutwardReason += $"You have been flagged in the system!{Environment.NewLine}Reason: {user.Status}{Environment.NewLine}";
                    if (server.UnderageRole != 0 && birthday == user.Birthday)
                            OutwardReason += $"Note: This server allows users under 18, you can ignore this message.{Environment.NewLine}";
                    if (birthday != user.Birthday)
                        OutwardReason += $"You have submitted info that does not match info on file!{Environment.NewLine}";
                    await BotFrame.EmbedWriter(context.Channel, context.User,
                        "Altimit DOB",
                        "You have already submited your info to this server!" + Environment.NewLine +
                        OutwardReason +
                        "Admins have been notified and will handle your situation as soon as possible.",
                        image: false, direct: true);
                    if (user.Status == UserStatus.Underage && birthday == user.Birthday)
                    {
                        if (server.NewUserRole != 0)
                            await guildUser.RemoveRoleAsync(newUserRole);
                        if (server.UnderageRole != 0)
                        {
                            await guildUser.AddRoleAsync(underageRole);
                            await BotFrame.EmbedWriter(adminChan, context.User,
                                "Altimit DOB",
                                $"{adminRole.Mention} The DOB provided by {context.User} matched my records and they were not flagged for any reason.{Environment.NewLine}" +
                                $"{memberRole} access granted.", time: -1);
                        }
                    }
                    return;
                }
                else if (!user.Flagged && birthday == user.Birthday)
                {
                    if (server.MemberRole != 0)
                        await guildUser.AddRoleAsync(memberRole);
                    if (server.NewUserRole != 0);
                        await guildUser.RemoveRoleAsync(newUserRole);
                    await BotFrame.EmbedWriter(adminChan, context.User,
                        "Altimit DOB",
                        $"{adminRole.Mention} The DOB provided by {context.User} matched my records and they were not flagged for any reason.{Environment.NewLine}" +
                        $"{memberRole} access granted.", time: -1);
                    return;
                }
            }
            UserInfo newUser = new UserInfo()
            {
                UserName = context.User.ToString(),
                UserId = context.User.Id,
                Birthday = birthday,
                Submitted = today
            };
            if (today.Year - birthday.Year >= 18 && birthday.Year > 1940 && birthday.Year < today.Year)
            {
                if (birthday.Year == 18 && birthday.Month == today.Month)
                {
                    //close warn
                    newUser.Flagged = true;
                    newUser.Status = UserStatus.Close;
                    await BotFrame.EmbedWriter(adminChan, context.User,
                        "Altimit DOB",
                        $"{adminRole.Mention} User turned 18 within the last month!{Environment.NewLine}" +
                        $"Username: {newUser.UserName}{Environment.NewLine}" +
                        $"User ID: {newUser.UserId}{Environment.NewLine}" +
                        $"Birthday: {newUser.Birthday}{Environment.NewLine}" +
                        $"Submited: {newUser.Submitted}{Environment.NewLine}" +
                        $"Flagged: {newUser.Flagged} Reason: {newUser.Status}", time: -1);
                }
                else
                {
                    //accept
                    newUser.Flagged = false;
                    newUser.Status = UserStatus.Accepted;
                }
                if (server.MemberRole != 0)
                    await guildUser.AddRoleAsync(memberRole);
                if (server.NewUserRole != 0)
                    await guildUser.RemoveRoleAsync(newUserRole);
            }
            else if (today.Year - birthday.Year < 18)
            {
                //underage
                newUser.Flagged = true;
                newUser.Status = UserStatus.Underage;
                string allowUnderage = "";
                if (server.UnderageRole != 0)
                    allowUnderage = $"Note: This server allows users under 18, you can ignore this message.{Environment.NewLine}";
                await BotFrame.EmbedWriter(context.Channel, context.User,
                    "Altimit DOB",
                    $"You have entered a date that shows you are under 18!{Environment.NewLine}" +
                    $"{allowUnderage}" +
                    $"Admins have been notified!",
                    image: false, direct: true);
                int ageYear = today.Year - newUser.Birthday.Year;
                int ageMonth = today.Month - newUser.Birthday.Month;
                int ageDay = today.Day - newUser.Birthday.Day;
                await BotFrame.EmbedWriter(adminChan, context.User,
                    "Altimit DOB",
                    $"{adminRole.Mention} User is under 18!{Environment.NewLine}" +
                    $"Username: {newUser.UserName} Age: {ageYear} years, {ageMonth} months, {ageDay} days{Environment.NewLine}" +
                    $"User ID: {newUser.UserId}{Environment.NewLine}" +
                    $"Birthday: {newUser.Birthday}{Environment.NewLine}" +
                    $"Submited: {newUser.Submitted}{Environment.NewLine}" +
                    $"Flagged: {newUser.Flagged} Reason: {newUser.Status}", time: -1);
                if (server.UnderageRole != 0)
                {
                    if (server.NewUserRole != 0)
                        await guildUser.RemoveRoleAsync(newUserRole);
                    if (server.UnderageRole != 0)
                        await guildUser.AddRoleAsync(underageRole);
                }
            }
            else if (birthday.Year < 1940)
            {
                //too old
                newUser.Flagged = true;
                newUser.Status = UserStatus.Overage;
                await BotFrame.EmbedWriter(context.Channel, context.User,
                    "Altimit DOB",
                    $"You have entered a questionably old date of birth!{Environment.NewLine}" +
                    $"Admins have been notified!",
                    image: false, direct: true);
                await BotFrame.EmbedWriter(adminChan, context.User,
                    "Altimit DOB",
                    $"{adminRole.Mention} User has entered a questionably old age!{Environment.NewLine}" +
                    $"Username: {newUser.UserName}{Environment.NewLine}" +
                    $"User ID: {newUser.UserId}{Environment.NewLine}" +
                    $"Birthday: {newUser.Birthday}{Environment.NewLine}" +
                    $"Submited: {newUser.Submitted}{Environment.NewLine}" +
                    $"Flagged: {newUser.Flagged} Reason: {newUser.Status}", time: -1);
                return;
            }
            if (server.UserInfoList == null)
                server.UserInfoList = new List<UserInfo>();
            server.UserInfoList.Add(newUser);
            BotFrame.SaveFile("servers");
        }

        public static async Task Birthday(DiscordServer server, DiscordSocketClient _client)
        {
            if (server.BirthdayChannel != 0)
            {
                SocketGuild guild = _client.Guilds.FirstOrDefault(x => x.Id == server.ServerId);
                ISocketMessageChannel channel = guild.Channels.FirstOrDefault(x => x.Id == server.BirthdayChannel) as ISocketMessageChannel;
                SocketRole adminRole = guild.Roles.FirstOrDefault(x => x.Id == server.AdminRole);
                string birthdayList = "";
                foreach (UserInfo user in server.UserInfoList.Where(x => x.Birthday.Month == DateTime.Now.Month && x.Birthday.Day == DateTime.Now.Day
                && x.Status != UserStatus.Banned && x.Status != UserStatus.NA))
                {
                    SocketGuildUser socketUser = guild.Users.FirstOrDefault(x => x.Id == user.UserId);
                    if (socketUser == null)
                        continue;
                    birthdayList += $"{socketUser.Mention} : {socketUser.Username} is {DateTime.Now.Year - user.Birthday.Year} years old today!{Environment.NewLine}";
                }
                BotFrame.EmbedWriter(channel, _client.CurrentUser,
                    "Altimit Birthday!!",
                    $"Happy Birthday!{Environment.NewLine}" +
                    $"{birthdayList}", time: -1, mentions: adminRole.Mention);
            }
        }
    }
}
