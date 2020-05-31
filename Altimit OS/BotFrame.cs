﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using System.Diagnostics;

namespace Altimit_OS
{
    class BotFrame
    {
        private const string resDir = "Resources";
        public static MainWindow _main;
        public static Config config;
        public static bool TimeStamp = true;

        public static async Task LoadFile(string file)
        {
            if (!Check(file))
            {
                consoleOut($"File {file} not found!");
                return;
            }
            else
            {
                consoleOut($"File {file} found! Loading...");
                string json = File.ReadAllText(resDir + "/" + file + ".json");
                switch (file)
                {
                    case "config":
                        config = new Config();
                        config = JsonConvert.DeserializeObject<Config>(json);
                        consoleOut("Config loaded!");
                        break;
                    case "servers":
                        _main.ServerList = new ObservableCollection<DiscordServer>();
                        _main.ServerList = JsonConvert.DeserializeObject<ObservableCollection<DiscordServer>>(json);
                        consoleOut("Servers loaded!");
                        break;
                }
            }
        }
        public static async Task SaveFile(string file)
        {
            consoleOut($"Saving {file}...");
            string json = "";
            switch (file)
            {
                case "config":
                    json = JsonConvert.SerializeObject(config, Formatting.Indented);
                    break;
                case "servers":
                    json = JsonConvert.SerializeObject(_main.ServerList, Formatting.Indented);
                    break;
            }
            try
            {
                File.WriteAllText(resDir + "/" + file + ".json", json);
            }
            catch (Exception ex)
            {
                BotFrame.consoleOut(ex.Message);
                return;
            }
            consoleOut($"Saved {file}!");
        }
        public static bool Check(string file)
        {
            if (!Directory.Exists(resDir))
                Directory.CreateDirectory(resDir);
            if (!File.Exists(resDir + "/" + file + ".json"))
                return false;
            else
                return true;
        }
        public static void consoleOut(string msg)
        {
            string timeNow = "";
            if (TimeStamp)
                timeNow = DateTime.Now.ToString() + ": ";
            _main.ConsoleString = _main.ConsoleString + timeNow + msg + Environment.NewLine;
        }
        public static void consoleClear()
        {
            string timeNow = "";
            if (TimeStamp)
                timeNow = DateTime.Now.ToString() + ": ";
            _main.ConsoleString = timeNow + "I was just cleared!" + Environment.NewLine;
        }
        public static async Task<ulong> EmbedWriter(ISocketMessageChannel chan, IUser user, string title, string data, bool pinned = false, bool image = true, bool direct = false, int time = 30000)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(title);
            embed.WithUrl("https://github.com/Haceo/AltimitBot");
            embed.WithDescription(data);
            embed.WithColor(new Color(0, 255, 0));
            embed.WithFooter($"Author: {user}");
            if (image)
                embed.WithThumbnailUrl(user.GetAvatarUrl());
            var embedded = embed.Build();
            if (time == -1 && !direct)
            {
                var msg = await chan.SendMessageAsync("", false, embedded);
                if (pinned)
                    await msg.PinAsync();
                return msg.Id;
            }
            else
            {
                if (!direct)
                {
                    var msg = await chan.SendMessageAsync("", false, embedded);
                    await Task.Delay(time);
                    await chan.DeleteMessageAsync(msg);
                    return msg.Id;
                }
                else
                {
                    var msg = await user.SendMessageAsync("", false, embedded);
                    return msg.Id;
                }
            }
        }
    }

    public class Config
    {
        public string Token { get; set; }
    }
    public enum PrefixChar
    {
        None,
        Exclamation = '!',
        At = '@',
        Pound = '#',
        Dollar = '$',
        Percent = '%',
        Carrot = '^',
        Ampersand = '&',
        Astrisk = '*'
    }
    public class DiscordServer
    {
        public bool Active { get; set; }
        public string ServerName { get; set; }
        public ulong ServerId { get; set; }
        public ulong BotChannel { get; set; }
        public ulong DOBChannel { get; set; }
        public ulong AdminChannel { get; set; }
        public ulong WelcomeChannel { get; set; }
        public bool UseWelcomeForLeave { get; set; }
        public bool UseWelcomeForDob { get; set; }
        public ulong AdminRole { get; set; }
        public ulong NewUserRole { get; set; }
        public ulong MemberRole { get; set; }
        public PrefixChar Prefix { get; set; }
        public string ServerJoined { get; set; }
        public List<UserInfo> UserInfoList { get; set; }
        public List<ReactionLockItem> ReactionLockList { get; set; }
        public List<TimeoutMember> TimeoutList { get; set; }
        public List<Song> SongList { get; set; }
        public int MaxLength { get; set; }
        public bool Continuous { get; set; }
        public bool LoopOne { get; set; }
    }
    public class Song
    {
        public string Title { get; set; }
        public string Duration { get; set; }
        public string User { get; set; }
        public string Path { get; set; }
    }
    public class AudioContainer
    {
        public ulong Server { get; set; }
        public IVoiceChannel Channel { get; set; }
        public Process ffmpeg { get; set; }
        public bool Playing { get; set; }
        public bool Interrupt { get; set; }
    }
    public class UserInfo
    {
        public string UserName { get; set; }
        public ulong UserId { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime Submitted { get; set; }
        public bool Flagged { get; set; }
        public UserStatus Status { get; set; }
    }
    public class OldUserInfo
    {
        public string ServerName { get; set; }
        public ulong ServerId { get; set; }
        public string UserName { get; set; }
        public ulong UserId { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime Submitted { get; set; }
        public bool Flagged { get; set; }
        public UserStatus Status { get; set; }
    }
    public enum UserStatus
    {
        NA,
        Accepted,
        Underage,
        Overage,
        Close,
        Banned
    }
    public class ReactionLockItem
    {
        public ulong Channel { get; set; }
        public ulong Message { get; set; }
        public string Emote { get; set; }
        public ulong Role { get; set; }
    }
    public class TimeoutMember
    {
        public ulong UserId { get; set; }
        public string Reason { get; set; }
        public ulong IssuerId { get; set; }
        public DateTime Start { get; set; }
        public int Duration { get; set; }
        public List<ulong> SavedRoles { get; set; }
        public ulong TimeoutRole { get; set; }
    }
}
