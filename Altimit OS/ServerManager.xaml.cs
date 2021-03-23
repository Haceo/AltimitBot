using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Altimit_OS
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
        private ObservableCollection<ReactionLock> lockList = new ObservableCollection<ReactionLock>();
        public ObservableCollection<ReactionLock> LockList
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
        private ObservableCollection<Streamer> streamerList = new ObservableCollection<Streamer>();
        public ObservableCollection<Streamer> StreamerList
        {
            get { return streamerList; }
            set
            {
                streamerList = value;
                RaisePropertyChanged("StreamerList");
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
            underageRoleIdBox.Text = _server.UnderageRole.ToString();
            birthdayChannelBox.Text = _server.BirthdayChannel.ToString();
            adminLogChanBox.Text = _server.AdminChannel.ToString();
            //-----Streamers----------------------------------
            if (_server.StreamerList != null)
                foreach (var streamer in _server.StreamerList)
                    StreamerList.Add(streamer);
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
            blacklistChanBox.Text = _server.BlacklistChannel.ToString();
            blacklistCheckbox.IsChecked = _server.UseBlacklist;
            userUpdateCheckbox.IsChecked = _server.UserUpdate;
            intervalSlider.Value = _server.StreamerCheckInterval;
            streamPostChannelBox.Text = _server.StreamPostChannel.ToString();
            streamerRoleBox.Text = _server.StreamingRole.ToString();
            streamEnableCheckBox.IsChecked = _server.StreamEnable;
        }
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //-----DOB----------------------------------
        private void DOBAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_server.UserInfoList == null)
                _server.UserInfoList = new List<UserInfo>();
            User ue = new User();
            ue.Owner = this;
            IsEnabled = false;
            ue.ShowDialog();
            IsEnabled = true;
            if (ue.DialogResult.HasValue && ue.DialogResult.Value)
            {
                if (_server.UserInfoList.FirstOrDefault(x => x.UserId == ulong.Parse(ue.userIdBox.Text)) != null)
                {
                    MessageBox.Show("You are trying to add a user that already has an entry, please select the user entry and edit it instead!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                UserInfo newUser = new UserInfo()
                {
                    UserId = ulong.Parse(ue.userIdBox.Text),
                    UserName = ue.userNameBox.Text,
                    Flagged = ue.isFlagged.IsChecked.Value,
                    Status = (UserStatus)ue.statusComboBox.SelectedItem
                };

                if (ue.birthdayDatePicker.SelectedDate.Value != null)
                    newUser.Birthday = ue.birthdayDatePicker.SelectedDate.Value;
                else
                    newUser.Birthday = DateTime.Now;
                if (ue.submitedDatePicker.SelectedDate.Value != null)
                    newUser.Submitted = ue.submitedDatePicker.SelectedDate.Value;
                else
                    newUser.Submitted = DateTime.Now;
                _server.UserInfoList.Add(newUser);
                UpdateView("dob");
                BotFrame.SaveFile("servers");
            }
        }
        private void DOBEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dobListBox.SelectedIndex == -1)
                return;
            User ue = new User();
            ue.Owner = this;
            ue._user = _server.UserInfoList[dobListBox.SelectedIndex];
            var user = _client.Guilds.FirstOrDefault(x => x.Id == _server.ServerId).Users.FirstOrDefault(y => y.Id == ue._user.UserId);
            if (_client.ConnectionState == Discord.ConnectionState.Connected && user != null)
            {
                try
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(user.GetAvatarUrl());
                    bi.EndInit();
                    ue.Icon = bi;
                }
                catch (Exception ex)
                {
                    BotFrame.consoleOut(ex.Message);
                }
            }
            ue.Title = ue._user.UserName + " : " + ue._user.UserId;
            IsEnabled = false;
            ue.ShowDialog();
            IsEnabled = true;
            if (ue.DialogResult.HasValue && ue.DialogResult.Value)
            {
                ue._user.UserId = ulong.Parse(ue.userIdBox.Text);
                ue._user.UserName = ue.userNameBox.Text;
                ue._user.Birthday = ue.birthdayDatePicker.SelectedDate.Value;
                ue._user.Submitted = ue.submitedDatePicker.SelectedDate.Value;
                ue._user.Flagged = ue.isFlagged.IsChecked.Value;
                ue._user.Status = (UserStatus)ue.statusComboBox.SelectedItem;
                UpdateView("dob");
                BotFrame.SaveFile("servers");
            }
        }
        private void DOBRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dobListBox.SelectedIndex == -1)
                return;
            IsEnabled = false;
            var res = MessageBox.Show("Are you sure you want to remove this item?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            IsEnabled = true;
            if (res == MessageBoxResult.No)
                return;
            _server.UserInfoList.Remove(_server.UserInfoList[dobListBox.SelectedIndex]);
            UserList.Clear();
            foreach (var user in _server.UserInfoList)
                UserList.Add(user);
            UpdateView("dob");
            BotFrame.SaveFile("servers");
        }
        //-----Streamers----------------------------
        private void StreamersAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_server.StreamerList == null)
                _server.StreamerList = new List<Streamer>();
            StreamerEditor se = new StreamerEditor();
            se.Owner = this;
            se.Title = "New Streamer";
            se.mentionLevelComboBox.SelectedItem = MentionLevel.None;
            IsEnabled = false;
            se.ShowDialog();
            IsEnabled = true;
            if (se.DialogResult.HasValue && se.DialogResult.Value)
            {
                if (_server.StreamerList.FirstOrDefault(x => x.DiscordId == ulong.Parse(se.discordIdBox.Text)) != null)
                {
                    var res = MessageBox.Show("You are trying to add a user that already has an entry, are you trying to add a second channel for the user?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.No)
                        return;
                }
                if (_server.StreamerList.FirstOrDefault(x => x.TwitchName == se.twitchNameBox.Text) != null)
                {
                    MessageBox.Show("You cannot add multiples of the same channel!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Streamer newStreamer = new Streamer()
                {
                    Streaming = false,
                    DiscordId = ulong.Parse(se.discordIdBox.Text),
                    DiscordName = _client.Guilds.FirstOrDefault(x => x.Id == _server.ServerId).Users.FirstOrDefault(y => y.Id == ulong.Parse(se.discordIdBox.Text)).ToString(),
                    Mention = (MentionLevel)se.mentionLevelComboBox.SelectedItem,
                    GiveRole = se.giveRoleCheckBox.IsChecked.Value,
                    AutoPost = se.autoPostCheckBox.IsChecked.Value,
                    TwitchName = se.twitchNameBox.Text
                };
                _server.StreamerList.Add(newStreamer);
                UpdateView("streamer");
                BotFrame.SaveFile("servers");
            }
        }
        private void StreamersEdit_Click(object sender, RoutedEventArgs e)
        {
            if (streamerListBox.SelectedIndex == -1)
                return;
            StreamerEditor se = new StreamerEditor();
            se.Owner = this;
            se._streamer = _server.StreamerList[streamerListBox.SelectedIndex];
            se.Title = $"Edit Streamer {se._streamer.DiscordName}";
            IsEnabled = false;
            se.ShowDialog();
            IsEnabled = true;
            if (se.DialogResult.HasValue && se.DialogResult.Value)
            {
                se._streamer.DiscordId = ulong.Parse(se.discordIdBox.Text);
                se._streamer.DiscordName = _client.Guilds.FirstOrDefault(x => x.Id == _server.ServerId).Users.FirstOrDefault(y => y.Id == ulong.Parse(se.discordIdBox.Text)).ToString();
                se._streamer.TwitchName = se.twitchNameBox.Text;
                se._streamer.Mention = (MentionLevel)se.mentionLevelComboBox.SelectedItem;
                se._streamer.GiveRole = se.giveRoleCheckBox.IsChecked.Value;
                se._streamer.AutoPost = se.autoPostCheckBox.IsChecked.Value;
                UpdateView("streamers");
                BotFrame.SaveFile("servers");
            }
        }
        private void StreamersDelete_Click(object sender, RoutedEventArgs e)
        {
            if (streamerListBox.SelectedIndex == -1)
                return;
            IsEnabled = false;
            var res = MessageBox.Show("Are you sure you want to remove this item?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            IsEnabled = true;
            if (res == MessageBoxResult.No)
                return;
            _server.StreamerList.Remove(_server.StreamerList[streamerListBox.SelectedIndex]);
            UpdateView("streamers");
            BotFrame.SaveFile("servers");
        }
        //-----Reaction Locks-----------------------
        private void ReactionLocksAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_server.ReactionLockList == null)
                _server.ReactionLockList = new List<ReactionLock>();
            ReactionLockEditor rl = new ReactionLockEditor();
            rl.Owner = this;
            rl.Title = "New Reaction Lock";
            IsEnabled = false;
            rl.ShowDialog();
            IsEnabled = true;
            if (rl.DialogResult.HasValue && rl.DialogResult.Value)
            {
                if (_server.ReactionLockList.FirstOrDefault(x => x.ChannelId == ulong.Parse(rl.channelBox.Text) && x.MessageId == ulong.Parse(rl.messageBox.Text)) != null)
                {
                    MessageBox.Show("You are trying to add a reaction lock that already has an entry, please select the reaction lock entry and edit it instead!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                ReactionLock newLock = new ReactionLock()
                {
                    ChannelId = ulong.Parse(rl.channelBox.Text),
                    MessageId = ulong.Parse(rl.messageBox.Text),
                    Emote = rl.emoteBox.Text,
                    GiveRole = ulong.Parse(rl.giveRoleBox.Text)
                };
                if (rl.takeRoleBox.Text != "")
                    newLock.TakeRole = ulong.Parse(rl.takeRoleBox.Text);
                _server.ReactionLockList.Add(newLock);
                UpdateView("reactionlock");
                BotFrame.SaveFile("servers");
            }
        }
        private void ReactionLocksEdit_Click(object sender, RoutedEventArgs e)
        {
            if (reactionLockListBox.SelectedIndex == -1)
                return;
            ReactionLockEditor rl = new ReactionLockEditor();
            rl.Owner = this;
            rl._lock = _server.ReactionLockList[reactionLockListBox.SelectedIndex];
            rl.Title = $"Edit lock {rl._lock.MessageId}";
            IsEnabled = false;
            rl.ShowDialog();
            IsEnabled = true;
            if (rl.DialogResult.HasValue && rl.DialogResult.Value)
            {
                rl._lock.ChannelId = ulong.Parse(rl.channelBox.Text);
                rl._lock.MessageId = ulong.Parse(rl.messageBox.Text);
                rl._lock.Emote = rl.emoteBox.Text;
                rl._lock.GiveRole = ulong.Parse(rl.giveRoleBox.Text);
                if (rl.takeRoleBox.Text != "" || rl.takeRoleBox.Text != "0")
                    rl._lock.TakeRole = ulong.Parse(rl.takeRoleBox.Text);
                UpdateView("reactionlock");
                BotFrame.SaveFile("servers");
            }
        }
        private void ReactionLocksDelete_Click(object sender, RoutedEventArgs e)
        {
            if (reactionLockListBox.SelectedIndex == -1)
                return;
            IsEnabled = false;
            var res = MessageBox.Show("Are you sure you want to remove this item?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            IsEnabled = true;
            if (res == MessageBoxResult.No)
                return;
            _server.ReactionLockList.Remove(_server.ReactionLockList[reactionLockListBox.SelectedIndex]);
            UpdateView("reactionlock");
            BotFrame.SaveFile("servers");
        }
        //-----Music--------------------------------
        private void MusicRemove_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            var res = MessageBox.Show("Are you sure you want to remove this item?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            IsEnabled = true;
            if (res == MessageBoxResult.No)
                return;
            File.Delete(_server.SongList[songListBox.SelectedIndex].Path);
            _server.SongList.Remove(_server.SongList[songListBox.SelectedIndex]);
            SongList.Clear();
            foreach (var song in _server.SongList)
                SongList.Add(song);
            UpdateView("music");
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
            if (ulong.TryParse(newUserRoleIdBox.Text, out ulong newUserRole))
                _server.NewUserRole = newUserRole;
            else
                _server.NewUserRole = 0;
            if (ulong.TryParse(memberRoleIdBox.Text, out ulong memberRole))
                _server.MemberRole = memberRole;
            else
                _server.MemberRole = 0;
            if (ulong.TryParse(underageRoleIdBox.Text, out ulong underageRole))
                _server.UnderageRole = underageRole;
            else
                _server.UnderageRole = 0;
            if (ulong.TryParse(birthdayChannelBox.Text, out ulong birthdayChannel))
                _server.BirthdayChannel = birthdayChannel;
            else
                _server.BirthdayChannel = 0;
            if (ulong.TryParse(adminLogChanBox.Text, out ulong adminChan))
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
            if (serverPrefixCheckBox.IsChecked == true)
                _server.Prefix = (PrefixChar)serverPrefixBox.SelectedItem;
            if (ulong.TryParse(adminRoleBox.Text, out ulong adminRole))
                _server.AdminRole = adminRole;
            else
                _server.AdminRole = 0;
            if (ulong.TryParse(botchanBox.Text, out ulong botChan))
                _server.BotChannel = botChan;
            else
                _server.BotChannel = 0;
            if (ulong.TryParse(dobchanBox.Text, out ulong dobChan))
                _server.DOBChannel = dobChan;
            else
                _server.DOBChannel = 0;
            if (ulong.TryParse(welcomeChanBox.Text, out ulong welcomeChan))
                _server.WelcomeChannel = welcomeChan;
            else
                _server.WelcomeChannel = 0;
            _server.UseWelcomeForDob = dobCheckBox.IsChecked.Value;
            _server.UseWelcomeForLeave = leaveCheckBox.IsChecked.Value;
            if (ulong.TryParse(blacklistChanBox.Text, out ulong blacklistChan))
                _server.BlacklistChannel = blacklistChan;
            else
                _server.BlacklistChannel = 0;
            _server.UseBlacklist = blacklistCheckbox.IsChecked.Value;
            _server.UserUpdate = userUpdateCheckbox.IsChecked.Value;
            _server.StreamerCheckInterval = intervalSlider.Value;
            if (ulong.TryParse(streamPostChannelBox.Text, out ulong postChannel))
                _server.StreamPostChannel = postChannel;
            else
                _server.StreamPostChannel = 0;
            if (ulong.TryParse(streamerRoleBox.Text, out ulong streamerRole))
                _server.StreamingRole = streamerRole;
            else
                _server.StreamingRole = 0;
            _server.StreamEnable = streamEnableCheckBox.IsChecked.Value;
            BotFrame.SaveFile("servers");
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void UpdateView(string view)
        {
            switch (view)
            {
                case "dob":
                    UserList.Clear();
                    foreach (var user in _server.UserInfoList)
                        UserList.Add(user);
                    break;
                case "streamers":
                    StreamerList.Clear();
                    foreach (var streamer in _server.StreamerList)
                        StreamerList.Add(streamer);
                    break;
                case "reactionlocks":
                    LockList.Clear();
                    foreach (var lockItem in _server.ReactionLockList)
                        LockList.Add(lockItem);
                    break;
                case "music":
                    SongList.Clear();
                    foreach (var song in _server.SongList)
                        SongList.Add(song);
                    break;
            }
        }
    }
}
