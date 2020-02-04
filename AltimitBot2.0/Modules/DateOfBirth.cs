using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace AltimitBot2._0.Modules
{
    public class DateOfBirth : ModuleBase<SocketCommandContext>
    {
        [Command("dob", RunMode = RunMode.Async)]
        public async Task dob([Remainder]string message = null)
        {
            if (message == null)
            {
                await Context.Message.DeleteAsync();
                CommandHandler.consoleOut("Deleted message from user: " + Context.User.Username + " within server: " + Context.Guild.Name + " in channel: " + Context.Channel.Name);
                CommandHandler.consoleOut("Message: " + Context.Message.Content);
                return;
            }
            if (message.Length > 10 | message.Contains(Environment.NewLine))
            {
                await Context.Channel.DeleteMessageAsync(Context.Message);
                CommandHandler.consoleOut("User: " + Context.User + " Server: " + Context.Guild.Name + " Channel: " + Context.Channel.Name + " Error: Invalid");
                CommandHandler.consoleOut("Message: " + Context.Message.Content);
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Error!",
                    "You have entered an invalid string, please try again and check your formatting...",
                    time: 10000);
                return;
            }
            string[] dob = message.Split(new char[] { ',', '/', '-', ' ' });
            int dobYear = int.Parse(dob[0].Trim());
            int dobMon = int.Parse(dob[1].Trim());
            int dobDay = int.Parse(dob[2].Trim());

            if (dobYear <= 1948 | dobYear >= DateTime.Now.Year)
            {
                await Context.Message.DeleteAsync();
                CommandHandler.consoleOut("User: " + Context.User + " Server: " + Context.Guild.Name + " Channel: " + Context.Channel.Name + " Error: Bad year");
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Error!",
                    "You have entered an invalid year, if you believe this is a mistake please contact the server admins.",
                    time: 10000);
                return;
            }
            if (dobMon < 1 | dobMon > 12)
            {
                await Context.Message.DeleteAsync();
                CommandHandler.consoleOut("User: " + Context.User + " Server: " + Context.Guild.Name + " Channel: " + Context.Channel.Name + " Error: Bad month");
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Error!",
                    "You have entered an invalid month, please check your formatting and try again.",
                    time: 10000);
                return;
            }
            if (dobDay < 1 | dobDay > 31)
            {
                await Context.Message.DeleteAsync();
                CommandHandler.consoleOut("User: " + Context.User + " Server: " + Context.Guild.Name + " Channel: " + Context.Channel.Name + " Error: Bad day");
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Error!",
                    "You have entered an invalid day, please check your formatting and try again.",
                    time: 10000);
                return;
            }
            var uid = BotConfig.userData.FirstOrDefault(x => x.UserId == Context.User.Id && x.ServerId == Context.Guild.Id) ?? null;
            if (uid != null)
            {
                await Context.Channel.DeleteMessageAsync(Context.Message);
                CommandHandler.consoleOut("User: " + Context.User + " Server: " + Context.Guild.Name + " Channel: " + Context.Channel.Name + " Error: Double Entry");
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Error!",
                    "You have already requested access to " + Context.Guild.Name + ": " + Context.Channel + Environment.NewLine +
                    "please contact an admin if you think there has been an error or to find out more information.",
                    image: false, Direct: true);
                return;
            }
            else
            {
                string chanName = BotConfig.serverData.FirstOrDefault(server => server.ServerId == Context.Guild.Id).dobChannel;
                string serverRole = BotConfig.serverData.FirstOrDefault(server => server.ServerId == Context.Guild.Id).dobRole;
                if (Context.Channel.Name != chanName)
                    return;
                var today = DateTime.Today;
                DateTime bday = DateTime.Parse(message);
                int age = today.Year - bday.Year;

                if (today.Month < bday.Month | (today.Month == bday.Month && today.Day < bday.Day))
                    age--;
                await Context.Channel.DeleteMessageAsync(Context.Message);
                UserInfo addinfo = new UserInfo();
                addinfo.ServerName = Context.Guild.Name;
                addinfo.ServerId = Context.Guild.Id;
                addinfo.UserName = Context.User.ToString();
                addinfo.UserId = Context.User.Id;
                addinfo.Birthday = bday;
                addinfo.Submitted = today;
                if (age >= 18 && age <= 60)
                {
                    addinfo.Status = userStatus.Accepted;
                    IGuildUser user = Context.User as IGuildUser;
                    await user.RemoveRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Unverified"));
                    await user.AddRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == serverRole));
                }
                else if (age < 18 | age > 60)
                {
                    addinfo.Flagged = true;
                    if (age < 18)
                    {
                        addinfo.Status = userStatus.Underage;
                        CommandHandler.consoleOut("User: " + Context.User + " Server: " + Context.Guild.Name + " Channel: " + Context.Channel.Name + " Error: Underage");
                        await Misc.EmbedWriter(Context.Channel, Context.User, 
                            "Not Approved!",
                            "Sorry but you are underage and cannot access " + Context.Guild.Name + ": " + Context.Channel.Name + "." + Environment.NewLine +
                            "Please notify an admin if you think this has been a mistake and please note this access request has been logged for safety.",
                            image: false, Direct: true);
                    }
                    else if (age > 60)
                    {
                        addinfo.Status = userStatus.Overage;
                        CommandHandler.consoleOut("User: " + Context.User + " Server: " + Context.Guild.Name + " Channel: " + Context.Channel.Name + " Error: Overage");
                        await Misc.EmbedWriter(Context.Channel, Context.User,
                            "Not Approved!",
                            "Sorry but you have entered an age that exceeds this bots limits," + Environment.NewLine +
                            "you cannot access " + Context.Guild.Name + " untill you clear the matter up, please contact an admin to resolve the issue.",
                            image: false, Direct: true);
                    }
                }
                if (age == 18 && today.Month == bday.Month)
                {
                    addinfo.Flagged = true;
                    addinfo.Status = userStatus.Close;
                    CommandHandler.consoleOut("User: " + Context.User + " Server: " + Context.Guild.Name + " Channel: " + Context.Channel.Name + " Error: Close");
                    await Misc.EmbedWriter(Context.Channel, Context.User,
                        "Approved with warning!",
                        "Sorry but youre birthday is close to todays date," + Environment.NewLine + 
                        "this is just a warning and you have been granted roles but you may be contacted by an admin to confirm details.",
                        image: false, Direct: true);
                }
                else if (!addinfo.Flagged)
                {
                    await Misc.EmbedWriter(Context.Channel, Context.User,
                        "Welcome!",
                        "Welcome to " + Context.Channel + " you have been granted the role " + serverRole,
                        image: false, Direct: true);
                }
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    BotConfig.userData.Add(addinfo);
                });
                BotConfig.SaveUserData();
            }
        }
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("dobsetup", RunMode = RunMode.Async)]
        public async Task dobSetup(string role = "Verified", bool fullSetup = false)
        {
            if (role == "Verified" && (Context.Guild.Roles.FirstOrDefault(x => x.Name == "Verified") != null))
            {
                await Context.Guild.CreateRoleAsync("Verified");
            }
            if (role != "Verified" && (Context.Guild.Roles.FirstOrDefault(x => x.Name == role) == null))
            {
                await Context.Guild.CreateRoleAsync(role);
            }
            var defaultRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Unverified");
            var serverRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == role);
            if (defaultRole == null)
            {
                await Context.Guild.CreateRoleAsync("Unverified");
            }
            if (fullSetup)
            {
                var users = Context.Guild.Users;
                foreach (var user in users)
                {
                    var userRole = user.Roles.FirstOrDefault(_role => _role.ToString() == role);
                    if (userRole != serverRole)
                        await user.AddRoleAsync(defaultRole);
                }
            }
            ServerInfo addinfo = new ServerInfo();
            addinfo.ServerName = Context.Guild.Name;
            addinfo.ServerId = Context.Guild.Id;
            addinfo.dobChannel = Context.Channel.Name;
            addinfo.dobRole = role;
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                BotConfig.serverData.Add(addinfo);
            });
            BotConfig.SaveServerData();
            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Misc.EmbedWriter(Context.Channel, Context.User,
                "Age Verification",
                "The role @" + role + " is now being used as age verification purposes!" + Environment.NewLine +
                "The channel #" + Context.Channel + " is now being monitored for age verification purposes!" + Environment.NewLine +
                Environment.NewLine +
                "Please submit your date of birth to this channel in the format:" + Environment.NewLine +
                "``$dob YYYY,MM,DD``" + Environment.NewLine +
                Environment.NewLine +
                "Example:" + Environment.NewLine +
                "``$dob 2000,01,01``" + Environment.NewLine +
                "``$dob 2000,1,1``" + Environment.NewLine +
                Environment.NewLine +
                "All other input will be deleted and ignored! This includes failed attempts!" + Environment.NewLine +
                "Providing false or misleading information will result in a ban!!",
                pinned: true, image: false, time: -1);
        }

        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("doblist", RunMode = RunMode.Async)]
        public async Task dobList(string command, string filter = "", int time = 10000)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            List<UserInfo> outputList = new List<UserInfo>();
            if (command.ToLower() == "status")
            {
                if (filter.ToLower() == "na")
                {
                    foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.Status == userStatus.NA))
                    {
                        outputList.Add(user);
                    }
                }
                if (filter.ToLower() == "accepted")
                {
                    foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.Status == userStatus.Accepted))
                    {
                        outputList.Add(user);
                    }
                }
                if (filter.ToLower() == "underage")
                {
                    foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.Status == userStatus.Underage))
                    {
                        outputList.Add(user);
                    }
                }
                if (filter.ToLower() == "overage")
                {
                    foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.Status == userStatus.Overage))
                    {
                        outputList.Add(user);
                    }
                }
                if (filter.ToLower() == "close")
                {
                    foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.Status == userStatus.Close))
                    {
                        outputList.Add(user);
                    }
                }
                if (filter.ToLower() == "banned")
                {
                    foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.Status == userStatus.Banned))
                    {
                        outputList.Add(user);
                    }
                }
            }
            else if (command.ToLower() == "flagged")
            {
                foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.Flagged == true))
                {
                    outputList.Add(user);
                }
            }
            else if (command.ToLower() == "unflagged")
            {

                foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.Flagged == false))
                {
                    outputList.Add(user);
                }
            }
            else if (command.ToLower() == "")
            {
                foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id))
                {
                    outputList.Add(user);
                }
            }
            else
            {
                //cnr
                return;
            }
            string outputString = "";
            foreach (UserInfo user in outputList)
            {
                outputString = outputString + user.UserName + Environment.NewLine;
            }
            await Misc.EmbedWriter(Context.Channel, Context.User,
                "Search Users",
                "Users found using filter " + command + " " + filter + ":" + Environment.NewLine +
                outputString,
                time: time);
        }
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("lookup", RunMode = RunMode.Async)]
        public async Task lookup(string info, string searchBy = "username", int time = 10000)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            List<UserInfo> outputList = new List<UserInfo>();
            if (searchBy.ToLower() == "username")
            {
                foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.UserName == info))
                {
                    outputList.Add(user);
                }
            }
            else if (searchBy.ToLower() == "userid")
            {
                foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.UserId == ulong.Parse(info)))
                {
                    outputList.Add(user);
                }
            }
            else if (searchBy.ToLower() == "dob")
            {
                char[] sep = { ',', '/', '.' };
                string[] dateRaw = info.Split(sep);
                int month = int.Parse(dateRaw[1].Trim());
                int day = int.Parse(dateRaw[2].Trim());
                int year = int.Parse(dateRaw[0].Trim());
                DateTime date = DateTime.Parse(year + "," + month + "," + day);
                foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.Birthday == date))
                {
                    outputList.Add(user);
                }
            }
            else if (searchBy.ToLower() == "submitted")
            {
                char[] sep = { ',', '/', '.' };
                string[] dateRaw = info.Split(sep);
                int month = int.Parse(dateRaw[1].Trim());
                int day = int.Parse(dateRaw[2].Trim());
                int year = int.Parse(dateRaw[0].Trim());
                DateTime date = DateTime.Parse(year + "," + month + "," + day);
                foreach (var user in BotConfig.userData.Where(x => x.ServerId == Context.Guild.Id && x.Submitted == date))
                {
                    outputList.Add(user);
                }
            }
            else
            {
                //cnr
                return;
            }
            string outputString = "";
            foreach (UserInfo user in outputList)
            {
                outputString = outputString + "Username: " + user.UserName + " - " + Environment.NewLine +
                    "User ID: " + user.UserId + Environment.NewLine +
                    "Birthday: " + user.Birthday + Environment.NewLine +
                    "Submitted: " + user.Submitted + Environment.NewLine +
                    "Flagged: " + user.Flagged + Environment.NewLine +
                    "Status: " + user.Status + Environment.NewLine + Environment.NewLine;
            }
            await Misc.EmbedWriter(Context.Channel, Context.User,
                "Search for user",
                "Users found using info " + info + ":" + Environment.NewLine +
                outputString,
                time: time);
        }
    }
}
