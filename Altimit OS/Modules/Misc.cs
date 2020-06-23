using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Altimit_OS.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        public static MainWindow _main;
        [Command("giveaway", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Giveaway(int winners, [Remainder]string roles)
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

        [Command("test")]
        public async Task test(string emote)
        {
            await Task.Delay(200);
            IEmote output;
            if (Emote.TryParse(emote, out var rawEmote))
                output = rawEmote;
            else
                output = new Emoji(emote);
            await Context.Message.AddReactionAsync(output);
        }

        /*
[Command("poll", RunMode = RunMode.Async)]
public async Task PollStart(string question, [Remainder]string options = "")
{
   await Task.Delay(200);
   await Context.Channel.DeleteMessageAsync(Context.Message);
   ulong message;
   bool res = ulong.TryParse(question, out message);
   if (res)
   {
       //end poll
       var getMsg = Context.Channel.GetMessageAsync(message).Result;
       if (getMsg == null)
       {
           BotFrame.EmbedWriter(Context.Channel, Context.User,
               "Altimit Poll",
               $"No poll found with message ID: {question}! Please check you are in the correct channel.");
           return;
       }
       if (Context.User.ToString() != getMsg.Embeds.First().Footer.ToString().Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[1])
       {
           await BotFrame.EmbedWriter(Context.Channel, Context.User,
               "Altimit Poll",
               $"You do not have permission to end a poll you did not make yourself!");
           return;
       }
       DateTimeOffset today = new DateTimeOffset(DateTime.Now);
       var embedAnswers = getMsg.Embeds.First().Description.Split('\n');
       List<IEmote> choiceList = new List<IEmote>();
       var rawReactions = getMsg.Reactions.Where(x => !x.Value.IsMe);//this works
       for (var i = 1; i < embedAnswers.Length; i++)
       {
           //VV---------Issues----------------VV
           string[] split = embedAnswers[i].Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
           Emote emote;
           res = Emote.TryParse(split[0].Trim(), out emote);
           if (res)
               choiceList.Add(emote);
           else
               choiceList.Add(new Emoji(split[0].Trim()));
       }
       //await Context.Channel.DeleteMessageAsync(getMsg);
       return;
   }
   string[] op = options.Split(';');
   if (op.Length <= 1)
   {
       //poll needs more than 1 answer
       return;
   }
   List<Tuple<string, IEmote>> optionList = new List<Tuple<string, IEmote>>();
   foreach (var option in op)
   {
       string[] split = option.Split(',');
       Emote emote;
       res = Emote.TryParse(split[1].Trim(), out emote);
       if (res)
           optionList.Add(new Tuple<string, IEmote>(split[0].Trim(), emote));
       else
           optionList.Add(new Tuple<string, IEmote>(split[0].Trim(), new Emoji(split[1].Trim())));
   }
   string optionsOut = "";
   foreach (var option in optionList)
       optionsOut += $"{option.Item2}: {option.Item1}{Environment.NewLine}";
   var msg = await BotFrame.EmbedWriter(Context.Channel, Context.User,
       "Altimit Poll",
       $"{question}{Environment.NewLine}{optionsOut}", time: -1);
   IMessage iMsg = Context.Channel.GetMessageAsync(msg).Result;
   foreach (var option in optionList)
   {
       await iMsg.AddReactionAsync(option.Item2);
       await Task.Delay(1000);
   }
}*/



        /*[Command("poll")]//this is the old code
        public async Task Poll(string question, [Remainder]string options)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            string[] op = options.Split(';');
            List<Option> optionsList = new List<Option>();
            foreach (var option in op)
            {
                string[] split = option.Split(',');
                Emote emote;
                bool res = Emote.TryParse(split[1].Trim(), out emote);
                if (res)
                    optionsList.Add(new Option() { Answer = split[0].Trim(), Emote = emote });
                else
                    optionsList.Add(new Option() { Answer = split[0].Trim(), Emote = new Emoji(split[1].Trim()) });
            }
            string optionOut = "";
            foreach (var option in optionsList)
                optionOut = $"{optionOut}{option.Emote}: {option.Answer}{Environment.NewLine}";
            var msg = await BotFrame.EmbedWriter(Context.Channel, Context.User,
                "Altimit Poll",
                $"{question}{Environment.NewLine}{optionOut}", time: -1);
            var message = (Context.Guild.Channels.FirstOrDefault(x => x.Id == Context.Channel.Id) as SocketTextChannel).GetMessageAsync(msg).Result;
            Poll newPoll = new Poll()
            {
                Channel = Context.Channel.Id,
                Message = Context.Message.Id,
                User = Context.User.Id,
                Started = DateTime.Now,
                Question = question,
                Options = optionsList
            };
            if (server.PollList == null)
                server.PollList = new List<Poll>();
            server.PollList.Add(newPoll);
            await BotFrame.SaveFile("servers");
            foreach (var option in newPoll.Options)
                await message.AddReactionAsync(option.Emote);
        }
        [Command("pollend", RunMode = RunMode.Async)]
        public async Task PollEnd(ulong channel, ulong message)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            var poll = server.PollList.FirstOrDefault(x => x.Channel == channel && x.Message == message);
            if (poll == null)
            {
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Poll",
                    $"Could not find poll with message ID matching {message}", time: 10000);
                return;
            }
            if (Context.User.Id != poll.User)
            {
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Poll",
                    $"You do not have permission to end a poll you did not make yourself!");
                return;
            }
            var msg = (Context.Guild.Channels.FirstOrDefault(x => x.Id == poll.Channel) as ITextChannel).GetMessageAsync(poll.Message).Result;
            if (msg == null)
            {
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Altimit Poll",
                    $"That poll message could not be found! Deleting poll from server files.");
                server.PollList.Remove(poll);
                return;
            }
            TimeSpan elapsed = DateTime.Now - poll.Started;
            var rawReactions = msg.Reactions;
            string reactions = "";
            foreach (var reaction in rawReactions.Where(x => !x.Value.IsMe))
                foreach (var option in poll.Options.Where(x => x.Emote == reaction.Key))
                    reactions = $"{reactions}{reaction.Key} {option.Answer}: {reaction.Value.ReactionCount}{Environment.NewLine}";
            await BotFrame.EmbedWriter(Context.Channel, Context.User,
                "Altimit Poll",
                $"Poll has ended after {elapsed}!{Environment.NewLine}{poll.Question}{Environment.NewLine}{reactions}", time: -1);
            server.PollList.Remove(poll);
            BotFrame.SaveFile("servers");
        }*/
    }
}
