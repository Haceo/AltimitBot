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
using Xceed.Wpf.Toolkit;

namespace AltimitBot2._0
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void WPF_Loaded(object sender, EventArgs e)
        {
            if (BotConfig.botConfig.token != "" | BotConfig.botConfig.token != null)
            {
                tokenBox.Text = BotConfig.botConfig.token;
            }
            if (BotConfig.botConfig.cmdPrefix != "" | BotConfig.botConfig.cmdPrefix != null)
            {
                prefixBox.Text = BotConfig.botConfig.token;
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (tokenBox.Text == null | tokenBox.Text == "")
            {
                System.Windows.MessageBox.Show("Error!", "No token was entered please enter one before saving.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (prefixBox.Text == null | prefixBox.Text == "")
            {
                BotConfig.botConfig.token = tokenBox.Text;
                BotConfig.botConfig.cmdPrefix = "$";
                System.Windows.MessageBox.Show("Warning!", "No prefix has been selected, Altimit has defaulted to $ as the command prefix.", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if ((tokenBox.Text != null | tokenBox.Text != "") && (prefixBox.Text != null | prefixBox.Text != ""))
            {
                BotConfig.botConfig.token = tokenBox.Text;
                BotConfig.botConfig.cmdPrefix = prefixBox.Text;
                BotConfig.SaveConfig();
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
