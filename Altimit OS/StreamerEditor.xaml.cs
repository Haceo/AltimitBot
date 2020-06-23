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
            DialogResult = true;
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
