using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace AltimitBot.Modules
{
    public class DateOfBirth : ModuleBase<SocketCommandContext>
    {
        [Command("dob")]
        public async Task dob([Remainder]string message)
        {
            if (message.Length != 12 | message.Contains(Environment.NewLine))
            {
                await Context.Channel.DeleteMessageAsync(Context.Message);
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Error!",
                    "You have entered an invalid string, please try again and check your formatting...",
                    time: 10000);
                return;
            }
            string userName = BotConfig.userData.FirstOrDefault(server => server.ServerId == Context.Guild.Id && server.UserName == Context.User.ToString()).UserName;
            if (userName == Context.User.ToString())
            {
                await Context.Channel.DeleteMessageAsync(Context.Message);
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Error!",
                    "You have already requested access to " + Context.Guild.Name + Environment.NewLine +
                    "please contact an admin if you think there has been an error or to find out more information.",
                    image: false, Direct: true);
                return;
            }
            else if (userName == "" | userName == null)
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
                BotConfig.UserInfo addinfo = new BotConfig.UserInfo();
                addinfo.ServerName = Context.Guild.Name;
                addinfo.ServerId = Context.Guild.Id;
                addinfo.UserName = Context.User.ToString();
                addinfo.UserId = Context.User.Id;
                addinfo.Birthday = bday;
                addinfo.Submitted = today;
                if (age >= 18 && age <= 60)
                {
                    addinfo.reason = BotConfig.Reason.Accepted;
                    IGuildUser user = Context.User as IGuildUser;
                    await user.RemoveRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Unverified"));
                    await user.AddRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == serverRole));
                }
                else if (age < 18 | age > 60)
                {
                    addinfo.Flagged = true;
                    if (age < 18)
                    {
                        addinfo.reason = BotConfig.Reason.Underage;
                        await Misc.EmbedWriter(Context.Channel, Context.User, 
                            "Not Approved!",
                            "Sorry but you are underage and cannot access " + Context.Guild.Name + ": " + Context.Channel.Name + "." + Environment.NewLine +
                            "Please notify an admin if you think this has been a mistake and please note this access request has been logged for safety.",
                            image: false, Direct: true);
                    }
                    else if (age > 60)
                    {
                        addinfo.reason = BotConfig.Reason.Overage;
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
                    addinfo.reason = BotConfig.Reason.Close;
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
                BotConfig.userData.Add(addinfo);
                BotConfig.SaveUserInfo();
            }
        }
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("dobsetup")]
        public async Task dobSetup([Remainder]string role)
        {
            var defaultRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Unverified");
            var serverRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == role);
            if (defaultRole == null)
            {
                await Context.Guild.CreateRoleAsync("Unverified");
            }
            var users = Context.Guild.Users;
            foreach (var user in users)
            {
                var userRole = user.Roles.FirstOrDefault(_role => _role.ToString() == role);
                if (userRole != serverRole)
                    await user.AddRoleAsync(defaultRole);
            }
            BotConfig.ServerInfo addinfo = new BotConfig.ServerInfo();
            addinfo.ServerName = Context.Guild.Name;
            addinfo.ServerId = Context.Guild.Id;
            addinfo.dobChannel = Context.Channel.Name;
            addinfo.dobRole = role;
            BotConfig.serverData.Add(addinfo);
            BotConfig.SaveServerInfo();
            await Context.Channel.DeleteMessageAsync(Context.Message);
            await Misc.EmbedWriter(Context.Channel, Context.User,
                "Age Verification",
                "The role @" + role + " is now being used as age verification purposes!" + Environment.NewLine +
                "The channel #" + Context.Channel + " is now being monitored for age verification purposes!" + Environment.NewLine +
                Environment.NewLine +
                "Please submit your age to this channel in the format:" + Environment.NewLine +
                "$dob YYYY, MM, DD" + Environment.NewLine +
                Environment.NewLine +
                "Example:" + Environment.NewLine +
                "$dob 2000, 01, 01" + Environment.NewLine +
                Environment.NewLine +
                "Spaces are REQUIRED!" + Environment.NewLine +
                "All other input will be deleted and ignored!",
                pinned: true, image: false, time: -1);
        }
    }
}
