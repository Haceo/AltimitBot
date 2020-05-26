using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                "Altimit",
                $"User(s) found using info type {searchBy}: {info}:{Environment.NewLine}{outputString}",
                time: time);
        }
    }
}
