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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Backgammon.Client.Views.Pages
{

    public partial class ChangePassword : Page
    {
        public ChangePassword()
        {
            InitializeComponent();
        }

        private void OnCurrentPasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholder(
                (PasswordBox)sender,
                textBlockCurrentPasswordPlaceholder);
        }

        private void OnNewPasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholder(
                (PasswordBox)sender,
                textBlockNewPasswordPlaceholder);
        }

        private void OnConfirmPasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordPlaceholder(
                (PasswordBox)sender,
                textBlockConfirmPasswordPlaceholder);
        }

        private void UpdatePasswordPlaceholder(PasswordBox passwordBox, TextBlock placeholder)
        {
            bool isPasswordEmpty = string.IsNullOrEmpty(passwordBox.Password);

            if (isPasswordEmpty)
            {
                placeholder.Visibility = Visibility.Visible;
            }
            else
            {
                placeholder.Visibility = Visibility.Collapsed;
            }
        }
    }
}
