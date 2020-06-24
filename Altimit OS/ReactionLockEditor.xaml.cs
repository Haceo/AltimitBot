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
            if (channelBox.Text.Trim() == "0" || channelBox.Text.Trim() == ""
                || messageBox.Text.Trim() == "0" || messageBox.Text.Trim() == ""
                || giveRoleBox.Text.Trim() == "0" || giveRoleBox.Text.Trim() == ""
                || emoteBox.Text.Trim() == "")
            {
                MessageBox.Show("You have not filled out all required fields, please check the marked fields and try again!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
