﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace AltimitBot2._0.Modules
{
    public class OwOify : ModuleBase<SocketCommandContext>
    {
        [Command("owo", RunMode = RunMode.Async)]
        public async Task owo([Remainder]string msg)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            if (msg == "")
                return;
            var serv = BotConfig.serverData.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            var chan = serv.botChannel;
            var role = serv.botRole;
            if (chan != null)
            {
                if (Context.Channel.ToString() == chan | (Context.User as SocketGuildUser).Roles.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Name == role)))
                {
                    await owoify(Context.Channel, Context.User, msg);
                }
                else
                    return;
            }
            else if (chan == null)
            {
                await owoify(Context.Channel, Context.User, msg);
            }
        }
        public static async Task owoify(ISocketMessageChannel chan, SocketUser user, string msg)
        {
            string[] owoFaces = { "OwO", "Owo", "owO", "ÓwÓ", "ÕwÕ", "@w@", "ØwØ", "øwø", "uwu", "UwU", "☆w☆", "✧w✧", "♥w♥", "゜w゜", "◕w◕", "ᅌwᅌ", "◔w◔", "ʘwʘ", "⓪w⓪", " ︠ʘw ︠ʘ", "(owo)" };
            string[] owoStrings = { "OwO *what's this*", "OwO *notices bulge*", "uwu yu so warm~", "owo pounces on you~~" };
            string owoified = msg;
            owoified = owoified.Replace('r', 'w').Replace('l', 'w').Replace('R', 'W').Replace('L', 'W');
            switch (new Random().Next(0, 1))
            {
                case 0:
                    owoified = owoified.Replace("n", "ny");
                    break;
                case 1:
                    owoified = owoified.Replace("n", "nya");
                    break;
            }
            switch (new Random().Next(0, 1))
            {
                case 0:
                    owoified = owoified.Replace("!", "!");
                    break;
                case 1:
                    owoified = owoified.Replace("!", $"{owoFaces[new Random().Next(0, owoFaces.Length)]}");
                    break;
            }
            switch (new Random().Next(0, 1))
            {
                case 0:
                    owoified = owoified.Replace("?", "?!");
                    break;
                case 1:
                    owoified = owoified.Replace("?", $"{owoFaces[new Random().Next(0, owoFaces.Length)]}");
                    break;
            }
            switch (new Random().Next(0, 30))
            {
                case 7:
                    owoified += $"{owoStrings[new Random().Next(0, owoStrings.Length)]}";
                    break;
            }
            await Misc.EmbedWriter(chan, (user as IUser),
                $"{owoFaces[new Random().Next(0, owoFaces.Length)]} Writer",
                owoified, time: -1);
        }
    }
}
