using System.Windows;

namespace Altimit_OS
{
    public partial class ReactionLockEditor : Window
    {
        public ReactionLock _lock;
        public ReactionLockEditor()
        {
            InitializeComponent();
        }
        private void WPF_Loaded(object sender, RoutedEventArgs e)
        {
            if (_lock != null)
            {
                channelBox.Text = _lock.ChannelId.ToString();
                messageBox.Text = _lock.MessageId.ToString();
                emoteBox.Text = _lock.Emote.ToString();
                giveRoleBox.Text = _lock.GiveRole.ToString();
                takeRoleBox.Text = _lock.TakeRole.ToString();
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
