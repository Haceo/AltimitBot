using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace AltimitBot2._0
{
    class BotConfig
    {
        public static MainWindow _this;
        private const string confFolder = "Resources";
        private const string confFile = "botconf.json";
        private const string playlistFile = "/playlist.json";
        private const string locklistFile = "/locklist.json";
        public static BotConf botConfig;

        private const string serverFolder = "ServerData";
        public static ObservableCollection<ServerInfo> serverData = new ObservableCollection<ServerInfo>();
        public static ObservableCollection<UserInfo> userData = new ObservableCollection<UserInfo>();
        public static ObservableCollection<ServerSAR> serverSar = new ObservableCollection<ServerSAR>();
        public static ObservableCollection<Tracks> playList = new ObservableCollection<Tracks>();
        public static List<LockChannel> LockList = new List<LockChannel>();
        public static void LoadConfig()
        {
            if (!Directory.Exists(confFolder))
                Directory.CreateDirectory(confFolder);
            if (!File.Exists(confFolder + "/" + confFile))
            {
                consoleOut("No config found! Please use File>Setup and input your BOT info...");
                return;
            }
            else
            {
                consoleOut("Config found! Loading...");
                string json = File.ReadAllText(confFolder + "/" + confFile);
                botConfig = new BotConf();
                botConfig = JsonConvert.DeserializeObject<BotConf>(json);
                if (botConfig.token == "" | botConfig.token == null | botConfig.cmdPrefix == "" | botConfig.cmdPrefix == null)
                {
                    consoleOut("Error!: No bot token found in config!");
                    return;
                }
            }
        }
        public static void SaveConfig()
        {
            consoleOut("Saving config...");
            string json = JsonConvert.SerializeObject(botConfig, Formatting.Indented);
            File.WriteAllText(confFolder + "/" + confFile, json);
            consoleOut("Saving complete!");
        }
        public static void LoadServerData()
        {
            if (!Directory.Exists(serverFolder))
            {
                consoleOut("Folder not found! Creating folder /ServerData...");
                Directory.CreateDirectory(serverFolder);
            }
            else
            {
                if (File.Exists(serverFolder + "/Serverdata.json"))
                {
                    consoleOut("Server data found!");
                    string json = File.ReadAllText(serverFolder + "/ServerData.json");
                    serverData = JsonConvert.DeserializeObject<ObservableCollection<ServerInfo>>(json);
                }
                else
                {
                    consoleOut("No server data found...");
                }
                if (File.Exists(serverFolder + "/ServerSAR.json"))
                {
                    consoleOut("Server SAR found!");
                    string json = File.ReadAllText(serverFolder + "/ServerSAR.json");
                    serverSar = JsonConvert.DeserializeObject<ObservableCollection<ServerSAR>>(json);
                }
                else
                {
                    consoleOut("No SAR data found...");
                }
                consoleOut("Done loading...");
                _this.ServerCount = serverData.Count();
            }
        }
        public static void LoadUserData()
        {
            if (!Directory.Exists(serverFolder))
            {
                consoleOut("Folder not found! Creating folder /ServerData...");
                Directory.CreateDirectory(serverFolder);
            }
            else
            {
                if (File.Exists(serverFolder + "/UserData.json"))
                {
                    consoleOut("User data found!");
                    string json = File.ReadAllText(serverFolder + "/UserData.json");
                    userData = JsonConvert.DeserializeObject<ObservableCollection<UserInfo>>(json);
                }
                else
                {
                    consoleOut("No user data found...");
                }
                consoleOut("Done loading...");
            }
        }
        public static void LoadInfo()
        {
            if (!Directory.Exists(serverFolder))
            {
                consoleOut("Folder not found! Creating folder /ServerData...");
                Directory.CreateDirectory(serverFolder);
            }
            else
            {
                if (File.Exists(serverFolder + "/Serverdata.json"))
                {
                    consoleOut("Server data found!");
                    string json = File.ReadAllText(serverFolder + "/ServerData.json");
                    serverData = JsonConvert.DeserializeObject<ObservableCollection<ServerInfo>>(json);
                }
                if (File.Exists(serverFolder + "ServerSAR.json"))
                {
                    consoleOut("Server SAR found!");
                    string json = File.ReadAllText(serverFolder + "/ServerSAR.json");
                    serverSar = JsonConvert.DeserializeObject<ObservableCollection<ServerSAR>>(json);
                }
                if (File.Exists(serverFolder + "/UserData.json"))
                {
                    consoleOut("User data found!");
                    string json = File.ReadAllText(serverFolder + "/UserData.json");
                    userData = JsonConvert.DeserializeObject<ObservableCollection<UserInfo>>(json);
                }
                else
                {
                    consoleOut("No data found...");
                }
                consoleOut("Done loading...");
            }
        }
        public static  void SaveServerData()
        {
            consoleOut("Saving server data...");
            string json = null;
            json = JsonConvert.SerializeObject(serverData, Formatting.Indented);
            File.WriteAllText(serverFolder + "/ServerData.json", json);
            json = null;
            json = JsonConvert.SerializeObject(serverSar, Formatting.Indented);
            File.WriteAllText(serverFolder + "/ServerSAR.json", json);
            consoleOut("Saving complete!");
            _this.ServerCount = serverData.Count();
        }
        public static void SaveUserData()
        {
            consoleOut("Saving user data...");
            string json = null;
            json = JsonConvert.SerializeObject(userData, Formatting.Indented);
            File.WriteAllText(serverFolder + "/UserData.json", json);
            consoleOut("Saving complete!");
        }
        public static void LoadPlaylist()
        {
            if (!Directory.Exists(serverFolder))
            {
                consoleOut("Folder not found! Creating folder /ServerData...");
                Directory.CreateDirectory(serverFolder);
            }
            else
            {
                if (File.Exists(serverFolder + playlistFile))
                {
                    consoleOut("Playlist found!");
                    string json = File.ReadAllText(serverFolder + playlistFile);
                    playList = JsonConvert.DeserializeObject<ObservableCollection<Tracks>>(json);
                }
                else
                {
                    consoleOut("No Playlist found...");
                }
                consoleOut("Done loading playlist...");
            }
        }
        public static void SavePlaylist()
        {
            consoleOut("Saving Playlist...");
            string json = null;
            json = JsonConvert.SerializeObject(playList, Formatting.Indented);
            File.WriteAllText(serverFolder + playlistFile, json);
            consoleOut("Saving playlist complete!");
        }
        public static void LoadLocks()
        {
            if (!Directory.Exists(serverFolder))
            {
                consoleOut("Folder not found! Creating folder /ServerData...");
                Directory.CreateDirectory(serverFolder);
            }
            else
            {
                if (File.Exists(serverFolder + locklistFile))
                {
                    consoleOut("LockList found!");
                    string json = File.ReadAllText(serverFolder + locklistFile);
                    LockList = JsonConvert.DeserializeObject<List<LockChannel>>(json);
                }
                else
                {
                    consoleOut("No LockList found...");
                }
        }
            consoleOut("Done loading LockList...");
        }
        public static void SaveLocks()
        {
            consoleOut("Saving LockList...");
            string json = null;
            json = JsonConvert.SerializeObject(LockList, Formatting.Indented);
            File.WriteAllText(serverFolder + locklistFile, json);
            consoleOut("Saving LockList complete!");
        }
        //VVV----copy to all modules----VVV
        public static void consoleOut(string msg)
        {
            _this.ConsoleString = _this.ConsoleString + DateTime.Now + ": " + msg + Environment.NewLine;
        }
        public static void consoleClear()
        {
            _this.ConsoleString = DateTime.Now + ": I was just cleared!" + Environment.NewLine;
        }
    }
    public class BotConf
    {
        public string token { get; set; }
        public string cmdPrefix { get; set; }
    }
    public class ServerInfo
    {
        public string ServerName { get; set; }
        public ulong ServerId { get; set; }
        public string dobChannel { get; set; }
        public string dobRole { get; set; }
        public string botChannel { get; set; }
        public string botRole { get; set; }
    }
    public class ServerSAR
    {
        public string ServerName { get; set; }
        public ulong ServerId { get; set; }
        public string Role { get; set; }
    }
    public class UserInfo
    {
        public string ServerName { get; set; }
        public ulong ServerId { get; set; }
        public string UserName { get; set; }
        public ulong UserId { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime Submitted { get; set; }
        public bool Flagged { get; set; }
        public userStatus Status { get; set; }
    }
    public enum userStatus
    {
        NA,
        Accepted,
        Underage,
        Overage,
        Close,
        Banned
    }
    public class Tracks
    {
        public string Title { get; set; }
        public string Durration { get; set; }
        public string Path { get; set; }
        public ulong Server { get; set; }
    }
    public class LockChannel : IEquatable<LockChannel>
    {
        public ulong Server { get; set; }
        public ulong Message { get; set; }
        public ulong Channel { get; set; }
        public string Emote { get; set; }
        public string Role { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            LockChannel objAsLock = obj as LockChannel;
            if (objAsLock == null) return false;
            else return Equals(objAsLock);
        }
        public bool Equals(LockChannel other)
        {
            if (other == null) return false;
            return (this.Message.Equals(other.Message));
        }
    }
}
