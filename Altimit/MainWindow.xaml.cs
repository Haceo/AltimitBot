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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Altimit_v3
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static DiscordSocketClient _client;
        static CommandHandler _handler;
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
            BotFrame.LoadFile("config");
            BotFrame.LoadFile("servers");
            if (BotFrame.config.Token != null || BotFrame.config.Token == "")
                connectionButton.IsEnabled = true;
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
            TokenEntry te = new TokenEntry();
            if (BotFrame.config == null)
                BotFrame.config = new Config();
            if (BotFrame.config.Token != null)
                te.tokenBox.Text = BotFrame.config.Token;
            te.Owner = this;
            te.ShowDialog();
            if (te.DialogResult.HasValue && te.DialogResult.Value)
            {
                BotFrame.config.Token = te.tokenBox.Text;
                BotFrame.SaveFile("config");
                connectionButton.IsEnabled = false;
                if (BotFrame.config.Token != null || BotFrame.config.Token != "")
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
            UpdateView("Server");
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
                BotFrame.consoleOut($"Loaded {loadFile.Count.ToString()} users!");
                BotFrame.SaveFile("servers");
            }
        }
        private void ServerSave_Click(object sender, RoutedEventArgs e)
        {
            BotFrame.SaveFile("servers");
        }
        private void ServerManage_Click(object sender, RoutedEventArgs e)
        {
            if (_client == null)
                return;
            ServerManager sm = new ServerManager();
            sm.Owner = this;
            sm._client = _client;
            sm._server = ServerList[serverListBox.SelectedIndex];
            sm.Title = $"Server: {sm._server.ServerName}";
            if (_client.ConnectionState == ConnectionState.Connected)
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(_client.Guilds.FirstOrDefault(x => x.Id == sm._server.ServerId).IconUrl);
                bi.EndInit();
                sm.Icon = bi;
            }
            sm.ShowDialog();
            sm = null;
        }
        public void UpdateView(string view)
        {
            RaisePropertyChanged($"{view}List");
        }
        //-----Connection----
        private async Task Connect()
        {
            connectionButton.Content = "Connecting...";
            if (BotFrame.config.Token == null || BotFrame.config.Token == "")
            {
                MessageBox.Show(this, "No token set, please use the Token button to set one!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, BotFrame.config.Token);
            await _client.StartAsync();
            _handler = new CommandHandler();
            await _handler.InitAsync(_client);
            connectionLight.Fill = Brushes.Green;
            await Task.Delay(2000);
            await ServerCheck();
            connectionButton.Content = "Connected!";
            await Task.Delay(-1);
        }
        private async Task Disconnect()
        {
            if (_client == null)
                return;
            connectionButton.Content = "Disconnecting...";
            await _client.StopAsync();
            await _client.LogoutAsync();
            _client.Dispose();
            connectionLight.Fill = Brushes.Red;
            connectionButton.Content = "Disconnected!";
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
            UpdateView("Server");
            BotFrame.SaveFile("servers");
        }
    }
}
