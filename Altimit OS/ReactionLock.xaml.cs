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

namespace Altimit_OS
{
    public partial class ReactionLock : Window
    {
        public ReactionLockItem _lock;
        public ReactionLock()
        {
            InitializeComponent();
        }
        private void WPF_Loaded(object sender, RoutedEventArgs e)
        {
            if (_lock != null)
            {
                channelIdBox.Text = _lock.Channel.ToString();
                emoteIdBox.Text = _lock.Emote;
                awardRoleIdBox.Text = _lock.Role.ToString();
                messageIdBox.Text = _lock.Message.ToString();
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
