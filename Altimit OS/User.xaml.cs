using System;
using System.Windows;

namespace Altimit_OS
{
    public partial class User : Window
    {
        public UserInfo _user;
        public User()
        {
            InitializeComponent();
            statusComboBox.ItemsSource = Enum.GetValues(typeof(UserStatus));
        }
        private void WPF_Loaded(object sender, RoutedEventArgs e)
        {
            if (_user != null)
            {
                userIdBox.Text = _user.UserId.ToString();
                userNameBox.Text = _user.UserName;
                birthdayDatePicker.SelectedDate = _user.Birthday;
                submitedDatePicker.SelectedDate = _user.Submitted;
                isFlagged.IsChecked = _user.Flagged;
                statusComboBox.SelectedItem = _user.Status;
            }
            else
            {
                submitedDatePicker.SelectedDate = DateTime.Now;
                statusComboBox.SelectedItem = UserStatus.NA;
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (userIdBox.Text.Trim() == "0" || userIdBox.Text.Trim() == "")
            {
                MessageBox.Show("You must at least enter the discord user ID to create an entry, please try again!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
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
