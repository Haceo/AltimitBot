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
    public partial class Server_Manager : Window
    {
        public ulong server;
        public Server_Manager()
        {
            InitializeComponent();
        }
        public void WPF_Loaded(object sender, EventArgs e)
        {
            Title = "Server Manager - " + BotConfig.serverData.FirstOrDefault(x => x.ServerId == server).ServerName;
            userList.ItemsSource = BotConfig.userData.Where(x => x.ServerId == server);
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (userList.SelectedIndex == -1)
                return;
            UserEditor editor = new UserEditor();
            editor.user = BotConfig.userData.FirstOrDefault(x => x.ServerId == server && x.UserId == ((UserInfo)userList.SelectedValue).UserId).UserId;
            editor.server = server;
            editor.Owner = this;
            editor.ShowDialog();
            if (editor.DialogResult.HasValue && editor.DialogResult.Value)
                userList.Items.Refresh();
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            UserEditor editor = new UserEditor();
            editor.server = server;
            editor.Owner = this;
            editor.ShowDialog();
            if (editor.DialogResult.HasValue && editor.DialogResult.Value)
                userList.Items.Refresh();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to delete user " + BotConfig.userData.FirstOrDefault(x => x.UserId == ((UserInfo)userList.SelectedValue).UserId && x.ServerId == server).UserName + "?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (res == MessageBoxResult.OK)
                BotConfig.userData.Remove(BotConfig.userData.FirstOrDefault(x => x.UserId == ((UserInfo)userList.SelectedValue).UserId && x.ServerId == server));
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            userList.Items.Refresh();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
