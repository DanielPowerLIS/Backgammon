using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using Backgammon.Client.Controllers.Registration;
using Backgammon.Client.RegistrationReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Controllers.Data;

using RegistrationAccount = Backgammon.Client.RegistrationReference.Account;

namespace Backgammon.Client.Views.Pages
{
    public partial class AccountRegistration : Page
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(AccountRegistration));
        private readonly RegistrationController _registrationController;
        private static readonly string InvalidInputErrorCode = "InvalidInput";
        private static readonly string EmailAlreadyRegisteredErrorCode = "EmailAlreadyRegistered";
        private static readonly string UsernameAlreadyTakenErrorCode = "UsernameAlreadyTaken";
        private static readonly string InvalidFieldLengthErrorCode = "InvalidFieldLength";
        private static readonly string RequiredFieldsErrorCode = "RequiredFields";
        private static readonly string InvalidUsernameErrorCode = "InvalidUsername";
        private static readonly string InvalidPasswordErrorCode = "InvalidPassword";
        private static readonly string InvalidEmailErrorCode = "InvalidEmail";

        public AccountRegistration()
        {
            _registrationController = new RegistrationController();

            InitializeComponent();
        }

        private RegistrationAccount BuildAccountInformation()
        {
            var accountInformation = new RegistrationAccount
            {
                Email = textBoxEmail.Text.Trim(),
                Password = passwordBoxPassword.Password,
                Username = textBoxUsername.Text.Trim(),
                Name = textBoxFirstName.Text.Trim(),
                PaternalSurname = textBoxPaternalLastName.Text.Trim(),
                MaternalSurname = textBoxMaternalLastName.Text.Trim()
            };

            return accountInformation;
        }

        private void FocusRegistrationField(string fieldName)
        {
            var registrationControls = new Dictionary<string, Control>
            {
                { nameof(RegistrationAccount.Email), textBoxEmail },
                { nameof(RegistrationAccount.Name), textBoxFirstName },
                { nameof(RegistrationAccount.PaternalSurname), textBoxPaternalLastName },
                { nameof(RegistrationAccount.MaternalSurname), textBoxMaternalLastName },
                { nameof(RegistrationAccount.Username), textBoxUsername },
                { nameof(RegistrationAccount.Password), passwordBoxPassword }
            };

            if (registrationControls.TryGetValue(fieldName, out Control targetControl))
            {
                targetControl.Focus();
            }
        }

        private async void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;

            try
            {
                await RegisterAccountAsync();
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private async Task RegisterAccountAsync()
        {
            try
            {
                await SubmitRegistrationAsync();
            }
            catch (FaultException<RegistrationFault> registrationException)
            {
                string errorCode = registrationException.Detail.ErrorCode;

                if (errorCode == InvalidInputErrorCode ||
                    errorCode == EmailAlreadyRegisteredErrorCode ||
                    errorCode == UsernameAlreadyTakenErrorCode)
                {
                    ApplicationLogger.LogWarning("The account registration request was rejected. Code: {0}",
                        errorCode);
                }
                else
                {
                    ApplicationLogger.LogError(registrationException,
                        "The server could not complete account registration. Code: {0}",
                        errorCode);
                }

                IsEnabled = true;
                ShowRegistrationFault(registrationException);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(
                    communicationException,
                    "A communication failure prevented account registration from completing.");

                ShowConnectionError();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(
                    timeoutException,
                    "The account registration request exceeded the time limit.");

                ShowConnectionError();
            }
        }

        private async Task SubmitRegistrationAsync()
        {
            RegistrationAccount accountInformation = BuildAccountInformation();

            InputValidationResult validationResult =
                await _registrationController.RegisterAccountAsync(accountInformation);

            if (!validationResult.IsValid)
            {
                ShowLocalValidationError(validationResult);
                return;
            }

            ShowRegistrationSuccess();
        }

        private void ShowLocalValidationError(InputValidationResult validationResult)
        {
            string errorCode = validationResult.ErrorCode;
            IsEnabled = true;

            if (errorCode == RequiredFieldsErrorCode)
            {
                ShowRegistrationError(
                    Properties.Resources.TextBlock_RequiredFieldsTitle,
                    Properties.Resources.TextBlock_RequiredFieldsMessage);
            }
            else if (errorCode == InvalidFieldLengthErrorCode)
            {
                string errorTitle =
                    Properties.Resources.TextBlock_InvalidFieldLengthTitle;

                string messageTemplate =
                    Properties.Resources.TextBlock_InvalidFieldLengthMessage;

                string errorMessage = string.Format(
                    messageTemplate, RegistrationLimits.TextFieldMaximumLength);

                ShowRegistrationError(errorTitle, errorMessage);
            }
            else
            {
                ShowLocalFormatError(errorCode);
            }

            FocusRegistrationField(validationResult.FieldName);
        }

        private void ShowLocalFormatError(string errorCode)
        {
            string errorTitle =
                Properties.Resources.TextBlock_RegistrationInvalidInputTitle;

            string errorMessage =
                Properties.Resources.TextBlock_RegistrationInvalidInputMessage;

            if (errorCode == InvalidEmailErrorCode)
            {
                errorTitle = Properties.Resources.TextBlock_InvalidEmailTitle;
                errorMessage = Properties.Resources.TextBlock_InvalidEmailMessage;
            }
            else if (errorCode == InvalidUsernameErrorCode)
            {
                errorTitle = Properties.Resources.TextBlock_InvalidUsernameTitle;
                errorMessage = Properties.Resources.TextBlock_InvalidUsernameMessage;
            }
            else if (errorCode == InvalidPasswordErrorCode)
            {
                errorTitle = Properties.Resources.TextBlock_InvalidPasswordTitle;
                errorMessage = Properties.Resources.TextBlock_InvalidPasswordMessage;
            }

            ShowRegistrationError(errorTitle, errorMessage);
        }

        private void ShowRegistrationError(string title, string message)
        {
            AccountDialog accountDialog = new AccountDialog(title, message);
            accountDialog.ShowOver(Window.GetWindow(this));
        }

        private void ShowConnectionError()
        {
            ShowRegistrationError(
                Properties.Resources.TextBlock_RegistrationConnectionErrorTitle,
                Properties.Resources.TextBlock_RegistrationConnectionErrorMessage);
        }

        private void ShowRegistrationSuccess()
        {
            AccountDialog accountDialog = new AccountDialog(
                Properties.Resources.TextBlock_RegistrationSuccessTitle,
                Properties.Resources.TextBlock_RegistrationSuccessMessage);

            accountDialog.ShowOver(Window.GetWindow(this));
        }

        private void ShowRegistrationFault(FaultException<RegistrationFault> registrationException)
        {
            string errorCode = registrationException.Detail.ErrorCode;

            if (ShowAvailabilityError(errorCode))
            {
                return;
            }

            if (errorCode == InvalidInputErrorCode)
            {
                ShowRegistrationError(
                    Properties.Resources.TextBlock_RegistrationInvalidInputTitle,
                    Properties.Resources.TextBlock_RegistrationInvalidInputMessage);
                return;
            }

            ShowSaveError();
        }

        private bool ShowAvailabilityError(string errorCode)
        {
            if (errorCode == EmailAlreadyRegisteredErrorCode)
            {
                ShowRegistrationError(
                    Properties.Resources.TextBlock_EmailAlreadyUsedTitle,
                    Properties.Resources.TextBlock_EmailAlreadyUsedMessage);

                textBoxEmail.Focus();
                return true;
            }

            if (errorCode == UsernameAlreadyTakenErrorCode)
            {
                ShowRegistrationError(
                    Properties.Resources.TextBlock_UsernameAlreadyUsedTitle,
                    Properties.Resources.TextBlock_UsernameAlreadyUsedMessage);

                textBoxUsername.Focus();
                return true;
            }

            return false;
        }

        private void ShowSaveError()
        {
            ShowRegistrationError(
                Properties.Resources.TextBlock_RegistrationSaveErrorTitle,
                Properties.Resources.TextBlock_RegistrationSaveErrorMessage);
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;

            textBlockPasswordPlaceholder.Visibility =
                string.IsNullOrEmpty(passwordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void OnButtonSignInClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Login());
        }
    }
}
