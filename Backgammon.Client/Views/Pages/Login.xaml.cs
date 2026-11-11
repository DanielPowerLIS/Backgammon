using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Controllers.Authentication;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Controllers.Data;

namespace Backgammon.Client.Views.Pages
{

    public partial class Login : Page
    {
        private static readonly string InvalidInputErrorCode = "InvalidInput";
        private static readonly string InvalidCredentialsErrorCode = "InvalidCredentials";

        private readonly AuthenticationController _authenticationController;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(Login));

        public Login()
        {
            InitializeComponent();

            _authenticationController = new AuthenticationController();
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            bool isPasswordEmpty = string.IsNullOrEmpty(passwordBox.Password);

            if (isPasswordEmpty)
            {
                textBlockPasswordPlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                textBlockPasswordPlaceholder.Visibility = Visibility.Collapsed;
            }
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
            InputValidationResult validationResult = await _authenticationController.SignInAsync(
                    textBoxUsernameOrEmail.Text,
                    passwordBoxPassword.Password);

            if (!validationResult.IsValid)
            {
                IsEnabled = true;
                ShowLoginDialog(
                    Properties.Resources.TextBlock_RequiredFieldsTitle,
                    Properties.Resources.TextBlock_RequiredFieldsMessage);

                Control targetControl;

                if (validationResult.FieldName == nameof(Credentials.Password))
                {
                    targetControl = (Control)passwordBoxPassword;
                }
                else
                {
                    targetControl = textBoxUsernameOrEmail;
                }

                targetControl.Focus();

                return;
            }

            OpenVerificationPage();
        }

        private async Task SignInWithFeedbackAsync()
        {
            try
            {
                await SubmitSignInAsync();
            }
            catch (FaultException<AuthenticationFault> authenticationException)
            {
                string errorCode = authenticationException.Detail.ErrorCode;

                if (errorCode == InvalidInputErrorCode ||
                    errorCode == InvalidCredentialsErrorCode)
                {
                    ApplicationLogger.LogWarning(
                        "The sign-in request was rejected. Code: {0}",
                        errorCode);
                }
                else
                {
                    ApplicationLogger.LogError(
                        authenticationException,
                        "The server could not complete sign-in. Code: {0}",
                        errorCode);
                }

                ShowAuthenticationError(authenticationException);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "A communication failure prevented sign-in from completing.");

                ShowConnectionError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The sign-in request exceeded the time limit.");

                ShowConnectionError();
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

            ShowConnectionError();
        }

        private void OpenVerificationPage()
        {
            string requestId = _authenticationController.RequestId;

            var verificationPage = new VerificationCode(requestId);

            IsEnabled = true;
            
            NavigationService.Navigate(verificationPage);
        }

        private void ShowConnectionError()
        {
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
