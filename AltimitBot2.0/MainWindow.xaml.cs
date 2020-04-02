using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Discord;
using Discord.WebSocket;

namespace AltimitBot2._0
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static DiscordSocketClient _client;
        static CommandHandler _handler;
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
                    consoleOutputBox.ScrollToEnd();
                });
            }
        }
        private int _serverCount;
        public int ServerCount
        {
            get { return _serverCount; }
            set
            {
                _serverCount = value;
                RaisePropertyChanged("ServerCount");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            BotConfig._this = this;
            CommandHandler._this = this;
            BotConfig.LoadConfig();
            BotConfig.LoadServerData();
            BotConfig.LoadUserData();
            BotConfig.LoadPlaylist();
            BotConfig.LoadLocks();
            BotConfig.LoadTimeouts();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            StartAsync();
            serverSep.Visibility = Visibility.Visible;
            serverLabel.Visibility = Visibility.Visible;
            serverCount.Visibility = Visibility.Visible;
        }
        public async Task StartAsync()
        {
            if (BotConfig.botConfig.token == "" | BotConfig.botConfig.token == null | BotConfig.botConfig.cmdPrefix == "" | BotConfig.botConfig.cmdPrefix == null)
            {
                MessageBox.Show("No token or prefix has been given please use File>Settings...", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, BotConfig.botConfig.token);
            await _client.StartAsync();
            connectionLight.Fill = Brushes.Green;
            _handler = new CommandHandler();
            await _handler.InitAsync(_client);

            await Task.Delay(-1);
        }

        public async Task Log(LogMessage msg)
        {
            ConsoleString = ConsoleString + DateTime.Now + ": " + msg.Message + Environment.NewLine;
            return;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings sp = new Settings();
            sp.Owner = this;
            sp.ShowDialog();
            if (sp.DialogResult.HasValue && sp.DialogResult.Value)
            {
                //do shit
            }
        }
        private void ServerManager_Click(object sender, RoutedEventArgs e)
        {
            ServerList mp = new ServerList();
            mp.Owner = this;
            mp.ShowDialog();
        }
        private void Server_Save(object sender, RoutedEventArgs e)
        {
            BotConfig.SaveServerData();
        }
        private void User_Save(object sender, RoutedEventArgs e)
        {
            BotConfig.SaveUserData();
        }
        private void All_Save(object sender, RoutedEventArgs e)
        {
            BotConfig.SaveServerData();
            BotConfig.SaveUserData();
        }
        private void Server_Load(object sender, RoutedEventArgs e)
        {
            BotConfig.LoadServerData();
        }
        private void User_Load(object sender, RoutedEventArgs e)
        {
            BotConfig.LoadUserData();
        }
        private void All_Load(object sender, RoutedEventArgs e)
        {
            BotConfig.LoadInfo();
        }
    }
}
