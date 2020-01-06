using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace AltimitBot2._0.Modules
{
    public class SAR : ModuleBase<SocketCommandContext>
    {
        [Command("asar", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task asar(string role)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var addRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == role) ?? null;
            if (addRole == null)
            {
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Add self assignable role.",
                    "Error! You have not chose a role or have chose a role that doesnt exist." + Environment.NewLine +
                    "Please try your query again or contact an admin for more info.");
                return;
            }
            else
            {
                ServerSAR newSar = new ServerSAR();
                newSar.ServerName = Context.Guild.Name;
                newSar.ServerId = Context.Guild.Id;
                newSar.Role = addRole.Name;
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    BotConfig.serverSar.Add(newSar);
                });
                BotConfig.SaveServerData();
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Add self assignable role.",
                    "Role " + role + " added as a self assignable role to server " + Context.Guild.Name + "." + Environment.NewLine +
                    "Use " + BotConfig.botConfig.cmdPrefix + "rsar followed by the role to remove the role from use.");
            }
        }
        [Command("rsar", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task rsar(string role)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var remRole = BotConfig.serverSar.FirstOrDefault(x => x.Role == role && x.ServerId == Context.Guild.Id) ?? null;
            if (remRole == null)
            {
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Remove self assignable role.",
                    "Error! You are trying to remove a role that isnt a self assignable role on the server: " + Environment.NewLine +
                    Context.Guild.Name + " please try your request again or contact an admin.");
                return;
            }
            else
            {
                BotConfig.serverSar.Remove(remRole);
                BotConfig.SaveServerData();
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "Remove self assignable role.",
                    "Role " + role + " has been removed from use as a self assignable role on server " + Context.Guild.Name + ".");
            }
        }
        [Command("lsar", RunMode = RunMode.Async)]
        public async Task lsar()
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            string chan = BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botChannel;
            string botrole = BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botRole;
            var user = (Context.User as SocketGuildUser);
            var hasRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == botrole) ?? null;
            if ((chan != null | chan != "") && Context.Channel.ToString() == chan && user.Roles.Contains(hasRole))
            {
                int i = 0;
                string roleList = "";
                foreach (var role in BotConfig.serverSar)
                {
                    i++;
                    roleList = roleList + Environment.NewLine + i + ". " + role.Role;
                }
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "List self assignable roles.",
                    "Self assignable roles are:" + Environment.NewLine +
                    roleList + Environment.NewLine +
                    "To claim a role type " + BotConfig.botConfig.cmdPrefix + "iam and then the role.");
            }
            else if (chan == null | chan == "")
            {
                int i = 0;
                string roleList = "";
                foreach (var role in BotConfig.serverSar)
                {
                    i++;
                    roleList = roleList + Environment.NewLine + i + ". " + role.Role;
                }
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "List self assignable roles.",
                    "Self assignable roles are:" + Environment.NewLine +
                    roleList + Environment.NewLine +
                    "To claim a role type " + BotConfig.botConfig.cmdPrefix + "iam and then the role.");
            }
            else if (user.Roles.Contains(hasRole))
            {
                int i = 0;
                string roleList = "";
                foreach (var role in BotConfig.serverSar)
                {
                    i++;
                    roleList = roleList + Environment.NewLine + i + ". " + role.Role;
                }
                await Misc.EmbedWriter(Context.Channel, Context.User,
                    "List self assignable roles.",
                    "Self assignable roles are:" + Environment.NewLine +
                    roleList + Environment.NewLine +
                    "To claim a role type " + BotConfig.botConfig.cmdPrefix + "iam and then the role.");
            }
        }
        [Command("iam", RunMode = RunMode.Async)]
        public async Task iam(string role)
        {
            await Context.Channel.DeleteMessageAsync(Context.Message);
            string chan = BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botChannel;
            string botrole = BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botRole;
            var user = (Context.User as SocketGuildUser);
            var hasRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == botrole) ?? null;
            if ((chan != null | chan != "") && Context.Channel.ToString() == chan && user.Roles.Contains(hasRole))
            {
                var addRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == role) ?? null;
                if (addRole == null)
                {
                    await Misc.EmbedWriter(Context.Channel, Context.User,
                        "Assign self assignable role.",
                        "Error! No role has been found with that name or you have not entered a role" + Environment.NewLine +
                        "Please try again or contact a server admin for more help.");
                }
                else if (addRole != null && !user.Roles.Contains(addRole))
                {
                    await user.AddRoleAsync(addRole);
                    await Misc.EmbedWriter(Context.Channel, Context.User,
                        "Assign self assignable role.",
                        "You have been assigned the role " + addRole + ".");
                }
            }
            else if (chan == null | chan == "")
            {
                var addRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == role) ?? null;
                if (addRole == null)
                {
                    await Misc.EmbedWriter(Context.Channel, Context.User,
                        "Assign self assignable role.",
                        "Error! No role has been found with that name or you have not entered a role" + Environment.NewLine +
                        "Please try again or contact a server admin for more help.");
                }
                else if (addRole != null && !user.Roles.Contains(addRole))
                {
                    await user.AddRoleAsync(addRole);
                    await Misc.EmbedWriter(Context.Channel, Context.User,
                        "Assign self assignable role.",
                        "You have been assigned the role " + addRole + ".");
                }
            }
            else if (user.Roles.Contains(hasRole))
            {
                var addRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == role) ?? null;
                if (addRole == null)
                {
                    await Misc.EmbedWriter(Context.Channel, Context.User,
                        "Assign self assignable role.",
                        "Error! No role has been found with that name or you have not entered a role" + Environment.NewLine +
                        "Please try again or contact a server admin for more help.");
                }
                else if (addRole != null && !user.Roles.Contains(addRole))
                {
                    await user.AddRoleAsync(addRole);
                    await Misc.EmbedWriter(Context.Channel, Context.User,
                        "Assign self assignable role.",
                        "You have been assigned the role " + addRole + ".");
                }
            }
        }
    }
}
