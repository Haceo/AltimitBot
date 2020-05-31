using System.Windows;

namespace Altimit_OS
{
    public partial class TokenEntry : Window
    {
        public TokenEntry()
        {
            InitializeComponent();
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
