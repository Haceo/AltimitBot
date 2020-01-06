using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace AltimitBot
{
    class BotConfig
    {
        private const string confFolder = "Resources";
        private const string confFile = "botconf.json";
        public static BotConf botConfig;

        private const string serverFolder = "ServerData";
        public static List<ServerInfo> serverData = new List<ServerInfo>();
        public static List<UserInfo> userData = new List<UserInfo>();
        public static void Config()
        {
            if (!Directory.Exists(confFolder))
                Directory.CreateDirectory(confFolder);
            if (!File.Exists(confFolder + "/" + confFile))
            {
                Console.WriteLine("No config file found! Creating file...");
                MkConf();
                return;
            }
            else
            {
                Console.WriteLine("Config found! Loading...");
                Console.WriteLine();
                string json = File.ReadAllText(confFolder + "/" + confFile);
                botConfig = new BotConf();
                botConfig = JsonConvert.DeserializeObject<BotConf>(json);
                if (botConfig.token == "" | botConfig.token == null | botConfig.cmdPrefix == "" | botConfig.cmdPrefix == null)
                {
                    Console.WriteLine("Error!: No bot token found in config!");
                    Console.WriteLine("Y to run config N to exit...");
                    switch (Console.ReadLine())
                    {
                        case "y":
                            MkConf();
                            return;
                        case "n":
                            Environment.Exit(0);
                            return;
                    }
                }
                Console.WriteLine("Bot token: " + botConfig.token);
                Console.WriteLine("Command prefix:  " + botConfig.cmdPrefix);
                Console.WriteLine();
                Console.WriteLine("Is this info current?");
                Console.WriteLine("Y to continue N to edit config or Q to quit...");
                switch (Console.ReadLine())
                {
                    case "y":
                        Console.WriteLine("Config loaded!");
                        return;
                    case "n":
                        MkConf();
                        return;
                    case "q":
                        Environment.Exit(0);
                        return;
                }
            }
        }
        static void MkConf()
        {
            botConfig = new BotConf();
            Console.WriteLine("Please enter bot info...");
            Console.WriteLine();
            Console.Write("Bot token:  ");
            botConfig.token = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Command prefix:  ");
            botConfig.cmdPrefix = Console.ReadLine();
            Console.WriteLine();
            SaveConfig();
            Console.WriteLine("Config made!");
            return;
        }
        public struct BotConf
        {
            public string token;
            public string cmdPrefix;
        }

        public static void SaveConfig()
        {
            Console.WriteLine("Saving config...");
            string json = JsonConvert.SerializeObject(botConfig, Formatting.Indented);
            File.WriteAllText(confFolder + "/" + confFile, json);
            Console.WriteLine("Saving complete!");
        }
        public static void LoadInfo()
        {
            if (!Directory.Exists(serverFolder))
            {
                Console.WriteLine("Folder not found! Creating ServerData folder...");
                Directory.CreateDirectory(serverFolder);
            }
            else
            {
                if (File.Exists(serverFolder + "/ServerData.json"))
                {
                    Console.WriteLine("Server data found!");
                    string json = File.ReadAllText(serverFolder + "/ServerData.json");
                    serverData = JsonConvert.DeserializeObject<List<ServerInfo>>(json);
                }
                if (File.Exists(serverFolder + "/UserData.json"))
                {
                    Console.WriteLine("User data found!");
                    string json = File.ReadAllText(serverFolder + "/UserData.json");
                    userData = JsonConvert.DeserializeObject<List<UserInfo>>(json);
                }
                else
                {
                    Console.WriteLine("No data found...");
                }
            }
            Console.WriteLine("Done loading...");
        }
        public static void SaveServerInfo()
        {
            Console.WriteLine("Saving server data...");
            string json = null;
            json = JsonConvert.SerializeObject(serverData, Formatting.Indented);
            File.WriteAllText(serverFolder + "/ServerData.json", json);
            Console.WriteLine("Saving complete!");
        }
        public static void SaveUserInfo()
        {
            Console.WriteLine("Saving user data...");
            string json = null;
            json = JsonConvert.SerializeObject(userData, Formatting.Indented);
            File.WriteAllText(serverFolder + "/UserData.json", json);
            Console.WriteLine("Saving complete!");
        }
        public struct ServerInfo
        {
            public string ServerName;
            public ulong ServerId;
            public string dobChannel;
            public string dobRole;
        }
        public struct UserInfo
        {
            public string ServerName;
            public ulong ServerId;
            public string UserName;
            public ulong UserId;
            public DateTime Birthday;
            public DateTime Submitted;
            public bool Flagged;
            public Reason reason;
        }
        public enum Reason
        {
            NA,
            Accepted,
            Underage,
            Overage,
            Close,
            Banned
        }
    }
}
