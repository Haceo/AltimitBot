using System;
using System.Windows;

namespace Altimit_OS
{
    public partial class StreamerEditor : Window
    {
        public Streamer _streamer;
        public StreamerEditor()
        {
            InitializeComponent();
            mentionLevelComboBox.ItemsSource = Enum.GetValues(typeof(MentionLevel));
        }
        private void WPF_Loaded(object sender, RoutedEventArgs e)
        {
            if (_streamer != null)
            {
                discordIdBox.Text = _streamer.DiscordId.ToString();
                twitchNameBox.Text = _streamer.TwitchName;
                mentionLevelComboBox.SelectedValue = _streamer.Mention;
                giveRoleCheckBox.IsChecked = _streamer.GiveRole;
                autoPostCheckBox.IsChecked = _streamer.AutoPost;
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (discordIdBox.Text.Trim() == "0" || discordIdBox.Text.Trim() == ""
                || twitchNameBox.Text.Trim() == "")
            {
                MessageBox.Show("You have left the discord ID or twitch name blank, please make sure to enter valid info and try again!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
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
