using Backgammon.Client.Repositories;
using Backgammon.Client.Utils;
using Backgammon.Client.Views.Dialog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Diagnostics;
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

    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }
        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;

            textBlockPasswordPlaceholder.Visibility =
                string.IsNullOrEmpty(passwordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private async void OnSignInClick(object sender, RoutedEventArgs e)
        {
            if (!HasCompleteCredentials())
            {
                return;
            }

            buttonSignIn.IsEnabled = false;

            try
            {
                LoginRepository repository = new LoginRepository();
                LoginResult result = await repository.ValidateCredentialsAsync(
                    textBoxUsernameOrEmail.Text.Trim(),
                    passwordBoxPassword.Password);

                ShowLoginResult(result);
            }
            catch (EntityException exception)
            {
                ShowConnectionError(exception);
            }
            catch (SqlException exception)
            {
                ShowConnectionError(exception);
            }
            finally
            {
                buttonSignIn.IsEnabled = true;
            }
        }

        private bool HasCompleteCredentials()
        {
            if (!string.IsNullOrWhiteSpace(textBoxUsernameOrEmail.Text) &&
                !string.IsNullOrEmpty(passwordBoxPassword.Password))
            {
                return true;
            }

            ShowLoginDialog(
                Properties.Resources.TextBlock_RequiredFieldsTitle,
                Properties.Resources.TextBlock_RequiredFieldsMessage);
            return false;
        }

        private void ShowLoginResult(LoginResult result)
        {
            switch (result)
            {
                case LoginResult.UserNotFound:
                    ShowLoginDialog(
                        Properties.Resources.TextBlock_LoginUserNotFoundTitle,
                        Properties.Resources.TextBlock_LoginUserNotFoundMessage);
                    break;

                case LoginResult.IncorrectPassword:
                    ShowLoginDialog(
                        Properties.Resources.TextBlock_LoginIncorrectPasswordTitle,
                        Properties.Resources.TextBlock_LoginIncorrectPasswordMessage);
                    break;

                case LoginResult.Success:
                    ShowLoginDialog(
                        Properties.Resources.TextBlock_LoginSuccessTitle,
                        Properties.Resources.TextBlock_LoginSuccessMessage);
                    NavigationService.Navigate(new MainMenu());
                    break;
            }
        }

        private void ShowConnectionError(Exception exception)
        {
            Trace.TraceError("Login database error: {0}", exception);

            ShowLoginDialog(
                Properties.Resources.TextBlock_LoginConnectionErrorTitle,
                Properties.Resources.TextBlock_LoginConnectionErrorMessage);
        }

        private void ShowLoginDialog(string title, string message)
        {
            AccountDialog dialog = new AccountDialog(title, message);
            dialog.ShowOver(Window.GetWindow(this));
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) 
            { 
                NavigationService.GoBack();
            }
        }
    }
}
