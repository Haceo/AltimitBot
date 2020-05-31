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
            DialogResult = true;
            this.Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
