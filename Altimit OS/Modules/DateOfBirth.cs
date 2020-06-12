using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Altimit_OS.Modules
{
    public class DateOfBirth : ModuleBase<SocketCommandContext>
    {
        public static async Task Submit(DiscordServer server, SocketCommandContext context)
        {
            var adminChan = context.Guild.Channels.FirstOrDefault(x => x.Id == server.AdminChannel) as ISocketMessageChannel;
            var adminRole = context.Guild.Roles.FirstOrDefault(x => x.Id == server.AdminRole);
            var guildUser = context.Guild.Users.FirstOrDefault(x => x.Id == context.User.Id);
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
                        OutwardReason = OutwardReason + $"You have been flagged in the system!{Environment.NewLine}Reason: {user.Status}{Environment.NewLine}";
                    if (birthday != user.Birthday)
                        OutwardReason = OutwardReason + $"You have submitted info that does not match info on file!{Environment.NewLine}";
                    await BotFrame.EmbedWriter(context.Channel, context.User,
                        "Altimit DOB",
                        "You have already submited your info to this server!" + Environment.NewLine +
                        OutwardReason +
                        "Admins have been notified and will handle your situation as soon as possible.",
                        image: false, direct: true);
                    return;
                }
                else if (!user.Flagged && birthday == user.Birthday)
                {
                    if (server.MemberRole != 0)
                    {
                        var addRole = context.Guild.Roles.FirstOrDefault(x => x.Id == server.MemberRole);
                        await guildUser.AddRoleAsync(addRole);
                    }
                    if (server.NewUserRole != 0)
                    {
                        var removeRole = context.Guild.Roles.FirstOrDefault(x => x.Id == server.NewUserRole);
                        await guildUser.RemoveRoleAsync(removeRole);
                    }
                    await BotFrame.EmbedWriter(adminChan, context.User,
                        "Altimit DOB",
                        $"{adminRole.Mention} The DOB provided by {context.User} matched my records and they were not flagged for any reason.{Environment.NewLine}" +
                        $"{context.Guild.Roles.FirstOrDefault(x => x.Id == server.MemberRole)} access granted.", time: -1);
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
                {
                    var addRole = context.Guild.Roles.FirstOrDefault(x => x.Id == server.MemberRole);
                    await guildUser.AddRoleAsync(addRole);
                }
                if (server.NewUserRole != 0)
                {
                    var removeRole = context.Guild.Roles.FirstOrDefault(x => x.Id == server.NewUserRole);
                    await guildUser.RemoveRoleAsync(removeRole);
                }
            }
            else if (today.Year - birthday.Year < 18)
            {
                //underage
                newUser.Flagged = true;
                newUser.Status = UserStatus.Underage;
                await BotFrame.EmbedWriter(context.Channel, context.User,
                    "Altimit DOB",
                    $"You have entered a date that shows you are underage!{Environment.NewLine}" +
                    $"Admins have been notified!",
                    image: false, direct: true);
                await BotFrame.EmbedWriter(adminChan, context.User,
                    "Altimit DOB",
                    $"{adminRole.Mention} User is underage!{Environment.NewLine}" +
                    $"Username: {newUser.UserName}{Environment.NewLine}" +
                    $"User ID: {newUser.UserId}{Environment.NewLine}" +
                    $"Birthday: {newUser.Birthday}{Environment.NewLine}" +
                    $"Submited: {newUser.Submitted}{Environment.NewLine}" +
                    $"Flagged: {newUser.Flagged} Reason: {newUser.Status}", time: -1);
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
    }
}
