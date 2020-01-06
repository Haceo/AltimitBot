using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AltimitBot2._0
{
    public partial class UserEditor : Window
    {
        public ulong user = 0;
        public ulong server = 0;
        bool loaded = false;

        public UserInfo _user;
        public UserEditor()
        {
            InitializeComponent();
        }
        public void WPF_Loaded(object sender, EventArgs e)
        {
            statusBox.ItemsSource = Enum.GetValues(typeof(userStatus));
            if (user != 0)
            {
                loaded = true;
                Title = "User Editor - " + BotConfig.userData.FirstOrDefault(x => x.UserId == user && x.ServerId == server).UserName;
                _user = BotConfig.userData.FirstOrDefault(x => x.UserId == user && x.ServerId == server);
                serverNameBox.Text = _user.ServerName;
                serverIdBox.Text = _user.ServerId.ToString();
                userNameBox.Text = _user.UserName;
                userIdBox.Text = _user.UserId.ToString();
                serverNameBox.IsReadOnly = true;
                serverIdBox.IsReadOnly = true;
                userNameBox.IsReadOnly = true;
                userIdBox.IsReadOnly = true;
                birthdayPicker.SelectedDate = _user.Birthday;
                submittedPicker.SelectedDate = _user.Submitted;
                flaggedCheck.IsChecked = _user.Flagged;
                statusBox.SelectedItem = _user.Status;
            }
            else
            {
                Title = "User Editor - New User";
                ServerInfo _server = BotConfig.serverData.FirstOrDefault(x => x.ServerId == server);
                serverNameBox.Text = _server.ServerName;
                serverIdBox.Text = _server.ServerId.ToString();
                serverNameBox.IsReadOnly = true;
                serverIdBox.IsReadOnly = true;
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                _user.Birthday = birthdayPicker.SelectedDate.Value;
                _user.Submitted = submittedPicker.SelectedDate.Value;
                _user.Flagged = flaggedCheck.IsChecked.Value;
                _user.Status = (userStatus)statusBox.SelectedItem;
            }
            else
            {
                _user.UserName = userNameBox.Text;
                _user.UserId = ulong.Parse(userIdBox.Text);
                _user.Birthday = birthdayPicker.SelectedDate.Value;
                _user.Submitted = submittedPicker.SelectedDate.Value;
                _user.Flagged = flaggedCheck.IsChecked.Value;
                _user.Status = (userStatus)statusBox.SelectedItem;
                BotConfig.userData.Add(_user);
            }
            DialogResult = true;
            this.Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
