using Discord;
using Discord.WebSocket;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TwitchLib.Api;

namespace Altimit_OS
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static DiscordSocketClient _client;
        static CommandHandler _handler;
        public static TwitchAPI _api;
        public Timer _timer = new Timer(30000);
        public Timer _dailytimer = new Timer(3600000);
        public bool loading = false;
        public List<AudioContainer> AudioInfo = new List<AudioContainer>();
        public event PropertyChangedEventHandler PropertyChanged;
        private string consoleString;
        public string ConsoleString
        {
            get { return consoleString; }
            set
            {
                consoleString = value;
                RaisePropertyChanged("ConsoleString");
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    consoleOutBox.ScrollToEnd();
                });
            }
        }
        private ObservableCollection<DiscordServer> serverList = new ObservableCollection<DiscordServer>();
        public ObservableCollection<DiscordServer> ServerList
        {
            get { return serverList; }
            set
            {
                serverList = value;
                RaisePropertyChanged("ServerList");
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            BotFrame._main = this;
            CommandHandler._main = this;
            Modules.Audio._main = this;
            Modules.Admin._main = this;
            Modules.DateOfBirth._main = this;
            Modules.Misc._main = this;
            Modules.Signup._main = this;
            BotFrame.LoadFile("config");
            BotFrame.LoadFile("servers");
            if (BotFrame.config == null)
                BotFrame.config = new Config();
            if (BotFrame.config.DiscordToken != null || BotFrame.config.DiscordToken == "")
                connectionButton.IsEnabled = true;
            if (ServerList == null)
                ServerList = new ObservableCollection<DiscordServer>();
            _api = new TwitchAPI();
            _timer.Elapsed += StreamerCheck_Elapsed;
            _dailytimer.Elapsed += _dailytimer_Elapsed;
        }

        private void _dailytimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour == 3)
                foreach (var server in ServerList)
                    Modules.DateOfBirth.Birthday(server, _client);
        }

        private void StreamerCheck_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var server in ServerList)
            {
                server.StreamerCheckElapsed += 0.5;
                if (server.StreamerCheckElapsed >= server.StreamerCheckInterval)
                    StreamerHandler(server);
            }
        }
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //-----Console-----------
        private void ConsoleConnect_MouseOver(object sender, MouseEventArgs e)
        {
            if (_client == null || _client.ConnectionState == ConnectionState.Disconnected)
                connectionButton.Content = "Connect";
            else if (_client.ConnectionState != ConnectionState.Disconnected)
                connectionButton.Content = "Disconnect";
        }
        private void ConsoleConnect_MouseLeave(object sender, MouseEventArgs e)
        {
            connectionButton.Content = "Connected:";
        }
        private void ConsoleConnect_Click(object sender, RoutedEventArgs e)
        {
            if (_client == null || _client.ConnectionState == ConnectionState.Disconnected)
                Connect();
            else if (_client.ConnectionState != ConnectionState.Disconnected)
                Disconnect();
        }
        private void TimeStamp_On(object sender, RoutedEventArgs e)
        {
            BotFrame.TimeStamp = true;
        }
        private void TimeStamp_Off(object sender, RoutedEventArgs e)
        {
            BotFrame.TimeStamp = false;
        }
        private void ConsoleToken_Click(object sender, RoutedEventArgs e)
        {
            TokenEntry tp = new TokenEntry();
            if (BotFrame.config == null)
                BotFrame.config = new Config();
            if (BotFrame.config.DiscordToken != null)
                tp.discordTokenBox.Text = BotFrame.config.DiscordToken;
            if (BotFrame.config.TwitchToken != null)
                tp.twitchTokenBox.Text = BotFrame.config.TwitchToken;
            if (BotFrame.config.TwitchClientId != null)
                tp.twitchClientIdBox.Text = BotFrame.config.TwitchClientId;
            tp.Owner = this;
            IsEnabled = false;
            tp.ShowDialog();
            IsEnabled = true;
            if (tp.DialogResult.HasValue && tp.DialogResult.Value)
            {
                BotFrame.config.DiscordToken = tp.discordTokenBox.Text;
                BotFrame.config.TwitchToken = tp.twitchTokenBox.Text;
                BotFrame.config.TwitchClientId = tp.twitchClientIdBox.Text;
                BotFrame.SaveFile("config");
                connectionButton.IsEnabled = false;
                if (BotFrame.config.DiscordToken != null || BotFrame.config.DiscordToken != "")
                    connectionButton.IsEnabled = true;
            }
        }
        private void ConsoleClear_Click(object sender, RoutedEventArgs e)
        {
            BotFrame.consoleClear();
        }
        //-----ServerManager-----
        private void ServerRefresh_Click(object sender, RoutedEventArgs e)
        {
            ServerList.Clear();
            BotFrame.LoadFile("servers");
        }
        private void ServerLoad_Click(object sender, RoutedEventArgs e)
        {
            BotFrame.LoadFile("servers");
        }
        private void ServerLoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".json";
            dlg.Filter = "JSON file (*.json)|*.json|All files (*.*)|*.*";
            var res = dlg.ShowDialog();
            if (res.HasValue && res.Value)
            {
                BotFrame.consoleOut("Loading User Info from file...");
                string json = File.ReadAllText(dlg.FileName);
                List<OldUserInfo> loadFile = new List<OldUserInfo>();
                loadFile = JsonConvert.DeserializeObject<List<OldUserInfo>>(json);
                foreach (OldUserInfo user in loadFile)
                {
                    UserInfo newUser = new UserInfo()
                    {
                        UserName = user.UserName,
                        UserId = user.UserId,
                        Birthday = user.Birthday,
                        Flagged = user.Flagged,
                        Status = user.Status,
                        Submitted = user.Submitted
                    };
                    var server = ServerList.FirstOrDefault(x => x.ServerId == user.ServerId);
                    if (server.UserInfoList == null)
                        server.UserInfoList = new List<UserInfo>();
                    server.UserInfoList.Add(newUser);
                }
                BotFrame.consoleOut($"Loaded {loadFile.Count} users!");
                BotFrame.SaveFile("servers");
            }
        }
        private void ServerSave_Click(object sender, RoutedEventArgs e)
        {
            BotFrame.SaveFile("servers");
        }
        private void ServerManage_Click(object sender, RoutedEventArgs e)
        {
            if (_client == null || serverListBox.SelectedIndex == -1)
                return;
            ServerManager sm = new ServerManager();
            sm.Owner = this;
            sm._client = _client;
            sm._server = ServerList[serverListBox.SelectedIndex];
            sm.Title = $"Server: {sm._server.ServerName}";
            var socGuild = _client.Guilds.FirstOrDefault(x => x.Id == sm._server.ServerId);
            if (_client.ConnectionState == ConnectionState.Connected && socGuild != null)
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(socGuild.IconUrl);
                bi.EndInit();
                sm.Icon = bi;
            }
            Visibility = Visibility.Hidden;
            sm.ShowDialog();
            Visibility = Visibility.Visible;
            sm = null;
        }
        //-----Connection----
        private async Task Connect()
        {
            if (loading)
            {
                MessageBox.Show(this, "The bot is loading a file and cannot connect safely, there may be a corruption issue within the file, please review system files and try to load again.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            connectionButton.Content = "Connecting...";
            if (BotFrame.config.DiscordToken == null || BotFrame.config.DiscordToken == "")
            {
                MessageBox.Show(this, "No token set, please use the Token button to set one!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                ExclusiveBulkDelete = true,
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true
            });
            _client.Log += Log;
            _client.Connected += _client_Connected;
            _client.Disconnected += _client_Disconnected;
            await _client.LoginAsync(TokenType.Bot, BotFrame.config.DiscordToken);
            await _client.StartAsync();
            _handler = new CommandHandler();
            await _handler.InitAsync(_client);
            _api.Settings.ClientId = BotFrame.config.TwitchClientId;
            _api.Settings.AccessToken = BotFrame.config.TwitchToken;
            await Task.Delay(2000);
            await ServerCheck();
            connectionButton.Content = "Connected!";
            serverTab.Visibility = Visibility.Visible;
            _timer.Start();
            await Task.Delay(-1);
        }

        private Task _client_Connected()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                connectionLight.Fill = Brushes.Green;
            });
            return Task.CompletedTask;
        }

        private Task _client_Disconnected(Exception arg)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                connectionLight.Fill = Brushes.Red;
            });
            return Task.CompletedTask;
        }

        private async Task Disconnect()
        {
            if (_client == null)
                return;
            connectionButton.Content = "Disconnecting...";
            await _client.StopAsync();
            await _client.LogoutAsync();
            _client.Dispose();
            connectionButton.Content = "Disconnected!";
            serverTab.Visibility = Visibility.Collapsed;
            _timer.Enabled = false;
        }
        public async Task Log(LogMessage msg)
        {
            BotFrame.consoleOut(msg.Message);
        }
        private async Task ServerCheck()
        {
            if (_client == null)
                return;
            foreach (var connectedGuild in _client.Guilds)
            {
                DiscordServer saved = ServerList.FirstOrDefault(x => x.ServerId == connectedGuild.Id);
                if (saved != null)
                {
                    if (!saved.Active)
                        saved.Active = true;
                }
                else
                {
                    DiscordServer newServer = new DiscordServer()
                    {
                        Active = true,
                        ServerName = connectedGuild.Name,
                        ServerId = connectedGuild.Id,
                        Prefix = PrefixChar.None,
                        ServerJoined = DateTime.Now.ToString()
                    };
                    ServerList.Add(newServer);
                }
            }
            foreach (var savedGuild in ServerList.Where(x => x.Active))
            {
                var check = _client.Guilds.FirstOrDefault(x => x.Id == savedGuild.ServerId);
                if (check == null)
                    savedGuild.Active = false;
            }
            BotFrame.SaveFile("servers");
            ServerList.Clear();
            BotFrame.LoadFile("servers");
        }
        public async Task StreamerHandler(DiscordServer server)
        {
            server.StreamerCheckElapsed = 0;
            var guild = _client.Guilds.FirstOrDefault(x => x.Id == server.ServerId);
            var streamRole = guild.Roles.FirstOrDefault(x => x.Id == server.StreamingRole);
            var channel = guild.Channels.FirstOrDefault(x => x.Id == server.StreamPostChannel) as ISocketMessageChannel;
            foreach (var streamer in server.StreamerList)
            {
                var user = guild.Users.FirstOrDefault(x => x.Id == streamer.DiscordId);
                var stream = await _api.V5.Streams.GetStreamByUserAsync((await _api.V5.Users.GetUserByNameAsync(streamer.TwitchName)).Matches[0].Id);
                if (stream.Stream == null)//not streaming
                {
                    if (streamer.Streaming)
                    {
                        streamer.Game = "";
                        streamer.Streaming = false;
                        if (server.StreamingRole != 0 && streamer.GiveRole && user.Roles.Contains(streamRole))
                            await user.RemoveRoleAsync(streamRole);
                        BotFrame.SaveFile("servers");
                    }
                    continue;
                }
                if (!streamer.Streaming)//not marked
                {
                    streamer.Streaming = true;
                    BotFrame.consoleOut($"{streamer.DiscordName} is now marked streaming in server: {guild.Name}");
                    if (server.StreamingRole != 0 && streamer.GiveRole && !user.Roles.Contains(streamRole))
                        await user.AddRoleAsync(streamRole);
                }
                if (stream.Stream.Channel.Game != streamer.Game)
                {
                    streamer.Game = stream.Stream.Channel.Game;
                    if (server.StreamPostChannel != 0 && streamer.AutoPost)
                    {
                        BotFrame.StreamPost(channel, user, stream.Stream, (int)streamer.Mention);
                    }
                    BotFrame.SaveFile("servers");
                }
            }
        }
    }
}
