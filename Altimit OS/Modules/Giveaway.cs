using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Altimit_OS.Modules
{
    class Giveaway : ModuleBase<SocketCommandContext>
    {
        public MainWindow _main;
        [Command("giveaway", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task GiveawayPost(int winners, [Remainder] string roles)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            string outString = "";
            string[] splitRoles;
            string[] entryRoles = null;
            string[] excludeRoles = null;
            if (roles.Contains('/'))
            {
                splitRoles = roles.Split('/');
                entryRoles = splitRoles[0].Split(',');
                excludeRoles = splitRoles[1].Split(',');
            }
            else
                entryRoles = roles.Split(',');
            List<SocketGuildUser> entries = new List<SocketGuildUser>();
            foreach (var role in entryRoles)
            {
                var rawRole = await ParseRole(Context, role.Trim());
                foreach (var user in Context.Guild.Users.Where(x => x.Roles.Contains(rawRole)))
                {
                    if (excludeRoles != null)
                    {
                        bool isExcluded = false;
                        foreach (var exclude in excludeRoles)
                        {
                            var rawExclude = await ParseRole(Context, exclude.Trim());
                            if (user.Roles.Contains(rawExclude))
                                isExcluded = true;
                        }
                        if (!isExcluded && !entries.Contains(user))
                            entries.Add(user);
                    }
                    else if (!entries.Contains(user))
                        entries.Add(user);
                }
            }
            if (entries.Count == 0)
            {
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Giveaway",
                    $"No users found with roles {roles}");
                return;
            }
            else if (entries.Count <= winners)
            {
                foreach (var entry in entries)
                    outString += $"{entry}{Environment.NewLine}";
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Giveaway",
                    $"Not enough users to run a random giveaway{Environment.NewLine}Winners are {outString}", time: -1);
                return;
            }
            Random r = new Random();
            for (int i = 0; i < winners; i++)
            {
                var ran = r.Next(0, entries.Count);
                outString += $"{i + 1}: {entries[ran]}{Environment.NewLine}";
                entries.RemoveAt(ran);
            }
            await BotFrame.EmbedWriter(Context.Channel, Context.User,
                "Altimit Giveaway",
                $"Winners are: {Environment.NewLine}{outString}", time: -1);
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
