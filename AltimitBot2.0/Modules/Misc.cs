using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace AltimitBot2._0.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("help", RunMode = RunMode.Async)]
        public async Task help(int time = 30000)
        {
            string chan = BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botChannel;
            string botrole = BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botRole;
            var user = (Context.User as SocketGuildUser);
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == botrole) ?? null;
            if ((chan != null | chan != "") && Context.Channel.ToString() == chan && user.Roles.Contains(role))
            {
                var embed = new EmbedBuilder();
                string helpData = "";
                if (!File.Exists("Resources/Help.txt"))
                {
                    helpData = "No help file found, please contact the server admins...";
                }
                else
                {
                    helpData = File.ReadAllText("Resources/Help.txt");
                }
                await EmbedWriter(Context.Channel, Context.User, "Help!", helpData, time: time);
                await Context.Channel.DeleteMessageAsync(Context.Message);
            }
            else if (chan == null | chan == "")
            {
                var embed = new EmbedBuilder();
                string helpData = "";
                if (!File.Exists("Resources/Help.txt"))
                {
                    helpData = "No help file found, please contact the server admins...";
                }
                else
                {
                    helpData = File.ReadAllText("Resources/Help.txt");
                }
                await EmbedWriter(Context.Channel, Context.User, "Help!", helpData, time: time);
                await Context.Channel.DeleteMessageAsync(Context.Message);
            }
            else if (user.Roles.Contains(role))
            {
                var embed = new EmbedBuilder();
                string helpData = "";
                if (!File.Exists("Resources/Help.txt"))
                {
                    helpData = "No help file found, please contact the server admins...";
                }
                else
                {
                    helpData = File.ReadAllText("Resources/Help.txt");
                }
                await EmbedWriter(Context.Channel, Context.User, "Help!", helpData, time: time);
                await Context.Channel.DeleteMessageAsync(Context.Message);
            }
            else
                await Context.Channel.DeleteMessageAsync(Context.Message);
        }
        [Command("pick", RunMode = RunMode.Async)]
        public async Task pick([Remainder]string message)
        {
            string chan = BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botChannel;
            string botrole = BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id).botRole;
            var user = (Context.User as SocketGuildUser);
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == botrole) ?? null;
            if ((chan != null | chan != "") && Context.Channel.ToString() == chan && user.Roles.Contains(role))
            {
                string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (options.Length <= 1)
                {
                    await Context.Channel.DeleteMessageAsync(Context.Message);
                    await EmbedWriter(Context.Channel, Context.User, "Choose an object", "You did not chose more than one object...");
                    return;
                }
                Random r = new Random();
                string data = options[r.Next(0, options.Length)];
                await EmbedWriter(Context.Channel, Context.User, "Choose an object", data);
                await Context.Channel.DeleteMessageAsync(Context.Message);
            }
            else if (chan == null | chan == "")
            {
                string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (options.Length <= 1)
                {
                    await Context.Channel.DeleteMessageAsync(Context.Message);
                    await EmbedWriter(Context.Channel, Context.User, "Choose an object", "You did not chose more than one object...");
                    return;
                }
                Random r = new Random();
                string data = options[r.Next(0, options.Length)];
                await EmbedWriter(Context.Channel, Context.User, "Choose an object", data);
                await Context.Channel.DeleteMessageAsync(Context.Message);
            }
            else if (user.Roles.Contains(role))
            {
                string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (options.Length <= 1)
                {
                    await Context.Channel.DeleteMessageAsync(Context.Message);
                    await EmbedWriter(Context.Channel, Context.User, "Choose an object", "You did not chose more than one object...");
                    return;
                }
                Random r = new Random();
                string data = options[r.Next(0, options.Length)];
                await EmbedWriter(Context.Channel, Context.User, "Choose an object", data);
                await Context.Channel.DeleteMessageAsync(Context.Message);
            }
            else
                await Context.Channel.DeleteMessageAsync(Context.Message);
        }

        public static async Task EmbedWriter(ISocketMessageChannel chan, IUser user, string title, string data, bool pinned = false, bool image = true, bool Direct = false, int time = 30000)
        {

            var embed = new EmbedBuilder();
            embed.WithTitle(title);
            embed.WithDescription(data);
            embed.WithColor(new Color(0, 255, 0));
            if (image)
                embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithFooter("Author: " + user);
            var embedded = embed.Build();
            if (time == -1 && !Direct)
            {
                var msg = await chan.SendMessageAsync("", false, embedded);
                if (pinned)
                    await msg.PinAsync();
            }
            else
            {
                if (!Direct)
                {
                    var msg = await chan.SendMessageAsync("", false, embedded);
                    await Task.Delay(time);
                    await chan.DeleteMessageAsync(msg);
                }
                else
                {
                    await user.SendMessageAsync("", false, embedded);
                }
            }
        }
    }
}
