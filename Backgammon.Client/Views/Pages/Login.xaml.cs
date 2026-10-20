using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Controllers.Authentication;
using Backgammon.Client.Services.Logging;
using Backgammon.Client.Utils;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Models;

namespace Backgammon.Client.Views.Pages
{

    public partial class Login : Page
    {
        private static readonly string InvalidInputErrorCode = "InvalidInput";
        private static readonly string InvalidCredentialsErrorCode = "InvalidCredentials";

        private readonly AuthenticationController _authenticationController;
        private readonly IApplicationLogger _applicationLogger;

        public Login()
        {
            InitializeComponent();

            _authenticationController = new AuthenticationController();
            _applicationLogger = new Log4NetApplicationLogger(typeof(Login));
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
            IsEnabled = false;

            try
            {
                await SignInWithFeedbackAsync();
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private async Task SubmitSignInAsync()
        {
            InputValidationResult validationResult =
                await _authenticationController.SignInAsync(
                    textBoxUsernameOrEmail.Text,
                    passwordBoxPassword.Password);

            if (!validationResult.IsValid)
            {
                IsEnabled = true;
                ShowLoginDialog(
                    Properties.Resources.TextBlock_RequiredFieldsTitle,
                    Properties.Resources.TextBlock_RequiredFieldsMessage);

                Control targetControl =
                    validationResult.FieldName == nameof(Credentials.Password)
                        ? (Control)passwordBoxPassword : textBoxUsernameOrEmail;
                targetControl.Focus();
                return;
            }

            ShowSuccessfulLogin();
        }

        private async Task SignInWithFeedbackAsync()
        {
            try
            {
                await SubmitSignInAsync();
            }
            catch (FaultException<AuthenticationFault> authenticationException)
            {
                ShowAuthenticationError(authenticationException);
            }
            catch (Exception connectionException) when (
                connectionException is CommunicationException ||
                connectionException is TimeoutException)
            {
                ShowConnectionError(connectionException);
            }
        }

        private void ShowAuthenticationError(FaultException<AuthenticationFault> authenticationException)
        {
            string errorCode = authenticationException.Detail.ErrorCode;

            if (errorCode == InvalidInputErrorCode)
            {
                ShowLoginDialog(
                    Properties.Resources.TextBlock_RequiredFieldsTitle,
                    Properties.Resources.TextBlock_RequiredFieldsMessage);
                return;
            }

            if (errorCode == InvalidCredentialsErrorCode)
            {
                ShowLoginDialog(
                    Properties.Resources.TextBlock_LoginInvalidCredentialsTitle,
                    Properties.Resources.TextBlock_LoginInvalidCredentialsMessage);
                return;
            }

            ShowConnectionError(authenticationException);
        }

        private void ShowSuccessfulLogin()
        {
            ShowLoginDialog(
                Properties.Resources.TextBlock_LoginSuccessTitle,
                Properties.Resources.TextBlock_LoginSuccessMessage);

            IsEnabled = true;

            NavigationService.Navigate(new MainMenu());
        }

        private void ShowConnectionError(Exception errorException)
        {
            _applicationLogger.LogError(
                errorException,
                "Sign-in service request failed.");

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
            NavigationService.Navigate(new Home());
        }

        private void OnButtonSignUpClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AccountRegistration());
        }
    }
}
