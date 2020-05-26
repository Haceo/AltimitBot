using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Altimit_v3
{
    public partial class ServerManager : Window, INotifyPropertyChanged
    {
        public DiscordSocketClient _client;
        public DiscordServer _server;
        public int Tab = 0;
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<Song> songList = new ObservableCollection<Song>();
        public ObservableCollection<Song> SongList
        {
            get { return songList; }
            set
            {
                songList = value;
                RaisePropertyChanged("SongList");
            }
        }
        private ObservableCollection<ReactionLockItem> lockList = new ObservableCollection<ReactionLockItem>();
        public ObservableCollection<ReactionLockItem> LockList
        {
            get { return lockList; }
            set
            {
                lockList = value;
                RaisePropertyChanged("LockList");
            }
        }
        private ObservableCollection<UserInfo> userList = new ObservableCollection<UserInfo>();
        public ObservableCollection<UserInfo> UserList
        {
            get { return userList; }
            set
            {
                userList = value;
                RaisePropertyChanged("UserList");
            }
        }
        public ServerManager()
        {
            InitializeComponent();
            DataContext = this;
            serverPrefixBox.ItemsSource = Enum.GetValues(typeof(PrefixChar));
        }
        private void WPF_Loaded(object sender, RoutedEventArgs e)
        {
            //-----DOB----------------------------------------
            if (_server.UserInfoList != null)
                foreach (var user in _server.UserInfoList)
                    UserList.Add(user);
            newUserRoleIdBox.Text = _server.NewUserRole.ToString();
            memberRoleIdBox.Text = _server.MemberRole.ToString();
            adminLogChanBox.Text = _server.AdminChannel.ToString();
            //-----Reaction Locks-----------------------------
            if (_server.ReactionLockList != null)
                foreach (var reactionLock in _server.ReactionLockList)
                    LockList.Add(reactionLock);
            //-----Music--------------------------------------
            if (_server.SongList != null)
                foreach (var song in _server.SongList)
                    SongList.Add(song);
            continuousCheckBox.IsChecked = _server.Continuous;
            loopOneCheckBox.IsChecked = _server.LoopOne;
            serverMaxTrackTimeBox.Text = _server.MaxLength.ToString();
            //-----Settings-----------------------------------
            if (_server.Prefix != PrefixChar.None)
            {
                serverPrefixCheckBox.IsChecked = true;
                serverPrefixBox.SelectedValue = _server.Prefix;
            }
            adminRoleBox.Text = _server.AdminRole.ToString();
            botchanBox.Text = _server.BotChannel.ToString();
            dobchanBox.Text = _server.DOBChannel.ToString();
            welcomeChanBox.Text = _server.WelcomeChannel.ToString();
            dobCheckBox.IsChecked = _server.UseWelcomeForDob;
            leaveCheckBox.IsChecked = _server.UseWelcomeForLeave;
        }
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //-----DOB----------------------------------
        private void DOBAdd_Click(object sender, RoutedEventArgs e)
        {
            User ue = new User();
            ue.Owner = this;
            ue.ShowDialog();
            if (ue.DialogResult.HasValue && ue.DialogResult.Value)
            {
                UserInfo newUser = new UserInfo()
                {
                    UserId = ulong.Parse(ue.userIdBox.Text),
                    UserName = ue.userNameBox.Text,
                    Birthday = ue.birthdayDatePicker.SelectedDate.Value,
                    Submitted = ue.submitedDatePicker.SelectedDate.Value,
                    Flagged = ue.isFlagged.IsChecked.Value,
                    Status = (UserStatus)ue.statusComboBox.SelectedItem
                };
                if (_server.UserInfoList == null)
                    _server.UserInfoList = new List<UserInfo>();
                _server.UserInfoList.Add(newUser);
                BotFrame.SaveFile("servers");
            }
        }
        private void DOBEdit_Click(object sender, RoutedEventArgs e)
        {
            User ue = new User();
            ue.Owner = this;
            ue._user = _server.UserInfoList[dobListBox.SelectedIndex];
            var user = _client.Guilds.FirstOrDefault(x => x.Id == _server.ServerId).Users.FirstOrDefault(y => y.Id == ue._user.UserId);
            if (_client.ConnectionState == Discord.ConnectionState.Connected && user != null)
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(user.GetAvatarUrl());
                bi.EndInit();
                ue.Icon = bi;
            }
            ue.Title = ue._user.UserName + " : " + ue._user.UserId;
            ue.ShowDialog();
            if (ue.DialogResult.HasValue && ue.DialogResult.Value)
            {
                ue._user.UserId = ulong.Parse(ue.userIdBox.Text);
                ue._user.UserName = ue.userNameBox.Text;
                ue._user.Birthday = ue.birthdayDatePicker.SelectedDate.Value;
                ue._user.Submitted = ue.submitedDatePicker.SelectedDate.Value;
                ue._user.Flagged = ue.isFlagged.IsChecked.Value;
                ue._user.Status = (UserStatus)ue.statusComboBox.SelectedItem;
                BotFrame.SaveFile("servers");
            }
        }
        private void DOBRemove_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to remove this item?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No)
                return;
            _server.UserInfoList.Remove(_server.UserInfoList[dobListBox.SelectedIndex]);
            UserList.Clear();
            foreach (var user in _server.UserInfoList)
                UserList.Add(user);
            BotFrame.SaveFile("servers");
        }
        //-----Reaction Locks-----------------------
        private void LockAdd_Click(object sender, RoutedEventArgs e)
        {
            ReactionLock rl = new ReactionLock();
            rl.Owner = this;
            rl.ShowDialog();
            if (rl.DialogResult.HasValue && rl.DialogResult.Value)
            {
                ReactionLockItem newLock = new ReactionLockItem()
                {
                    Channel = ulong.Parse(rl.channelIdBox.Text),
                    Emote = rl.emoteIdBox.Text,
                    Role = ulong.Parse(rl.awardRoleIdBox.Text),
                    Message = ulong.Parse(rl.messageIdBox.Text)
                };
                if (_server.ReactionLockList == null)
                    _server.ReactionLockList = new List<ReactionLockItem>();
                _server.ReactionLockList.Add(newLock);
                BotFrame.SaveFile("servers");
            }
        }
        private void LockEdit_Click(object sender, RoutedEventArgs e)
        {
            ReactionLock rl = new ReactionLock();
            rl.Owner = this;
            rl._lock = _server.ReactionLockList[reactionLockListBox.SelectedIndex];
            rl.ShowDialog();
            if (rl.DialogResult.HasValue && rl.DialogResult.Value)
            {
                rl._lock.Channel = ulong.Parse(rl.channelIdBox.Text);
                rl._lock.Emote = rl.emoteIdBox.Text;
                rl._lock.Role = ulong.Parse(rl.awardRoleIdBox.Text);
                rl._lock.Message = ulong.Parse(rl.messageIdBox.Text);
                BotFrame.SaveFile("servers");
            }
        }
        private void LockRemove_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to remove this item?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No)
                return;
            _server.ReactionLockList.Remove(_server.ReactionLockList[reactionLockListBox.SelectedIndex]);
            LockList.Clear();
            foreach (var reactionLock in _server.ReactionLockList)
                LockList.Add(reactionLock);
            BotFrame.SaveFile("servers");
        }
        //-----Music--------------------------------
        private void MusicRemove_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to remove this item?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No)
                return;
            File.Delete(_server.SongList[songListBox.SelectedIndex].Path);
            _server.SongList.Remove(_server.SongList[songListBox.SelectedIndex]);
            SongList.Clear();
            foreach (var song in _server.SongList)
                SongList.Add(song);
            BotFrame.SaveFile("servers");
        }
        //-----Server Setting-----------------------
        private void Prefix_Enable(object sender, RoutedEventArgs e)
        {
            serverPrefixBox.IsEnabled = true;
        }
        private void Prefix_Disable(object sender, RoutedEventArgs e)
        {
            serverPrefixBox.IsEnabled = false;
            serverPrefixBox.SelectedValue = PrefixChar.None;
        }
        //-----Global-------------------------------
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //-----DOB--------------------------------------------------------------------
            ulong newUserRole;
            ulong memberRole;
            ulong adminChan;
            bool res = ulong.TryParse(newUserRoleIdBox.Text, out newUserRole);
            if (res)
                _server.NewUserRole = newUserRole;
            else
                _server.NewUserRole = 0;
            res = ulong.TryParse(memberRoleIdBox.Text, out memberRole);
            if (res)
                _server.MemberRole = memberRole;
            else
                _server.MemberRole = 0;
            res = ulong.TryParse(adminLogChanBox.Text, out adminChan);
            if (res)
                _server.AdminChannel = adminChan;
            else
                _server.AdminChannel = 0;
            //-----Reaction Locks---------------------------------------------------------
            //---------Nothing on page to save--------------------------------------------
            //-----Music------------------------------------------------------------------
            _server.MaxLength = int.Parse(serverMaxTrackTimeBox.Text);
            if (continuousCheckBox.IsChecked == true)
                _server.Continuous = true;
            else
                _server.Continuous = false;
            if (loopOneCheckBox.IsChecked == true)
                _server.LoopOne = true;
            else
                _server.LoopOne = false;
            //-----Settings---------------------------------------------------------------
            ulong adminRole;
            ulong botChan;
            ulong dobChan;
            ulong welcomeChan;
            if (serverPrefixCheckBox.IsChecked == true)
                _server.Prefix = (PrefixChar)serverPrefixBox.SelectedItem;
            res = ulong.TryParse(adminRoleBox.Text, out adminRole);
            if (res)
                _server.AdminRole = adminRole;
            else
                _server.AdminRole = 0;
            res = ulong.TryParse(botchanBox.Text, out botChan);
            if (res)
                _server.BotChannel = botChan;
            else
                _server.BotChannel = 0;
            res = ulong.TryParse(dobchanBox.Text, out dobChan);
            if (res)
                _server.DOBChannel = dobChan;
            else
                _server.DOBChannel = 0;
            res = ulong.TryParse(welcomeChanBox.Text, out welcomeChan);
            if (res)
                _server.WelcomeChannel = welcomeChan;
            else
                _server.WelcomeChannel = 0;
            _server.UseWelcomeForDob = dobCheckBox.IsChecked.Value;
            _server.UseWelcomeForLeave = leaveCheckBox.IsChecked.Value;
            BotFrame.SaveFile("servers");
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void UpdateView(string view)
        {
            RaisePropertyChanged($"{view}List");
        }
    }
}
