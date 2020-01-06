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
    public partial class ServerList : Window
    {
        public ServerList()
        {
            InitializeComponent();
        }
        public void WPF_Loaded(object sender, EventArgs e)
        {
            serverList.ItemsSource = BotConfig.serverData;
        }
        private void Manage_Click(object sender, RoutedEventArgs e)
        {
            if (serverList.SelectedIndex == -1)
                return;
            Server_Manager manager = new Server_Manager();
            manager.server = BotConfig.serverData[serverList.SelectedIndex].ServerId;
            manager.Owner = this;
            manager.ShowDialog();
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
