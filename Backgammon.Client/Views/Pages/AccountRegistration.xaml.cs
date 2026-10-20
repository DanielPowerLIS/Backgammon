using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using Backgammon.Client.Controllers.Registration;
using Backgammon.Client.Models;
using Backgammon.Client.RegistrationReference;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;

using RegistrationAccount = Backgammon.Client.RegistrationReference.Account;

namespace Backgammon.Client.Views.Pages
{
    public partial class AccountRegistration : Page
    {
        private readonly IApplicationLogger _applicationLogger;
        private readonly RegistrationController _registrationController;
        private static readonly string InvalidInputErrorCode = "InvalidInput";
        private static readonly string EmailAlreadyRegisteredErrorCode = "EmailAlreadyRegistered";
        private static readonly string UsernameAlreadyTakenErrorCode = "UsernameAlreadyTaken";
        private static readonly string InvalidFieldLengthErrorCode = "InvalidFieldLength";
        
        public AccountRegistration()
            : this(new Log4NetApplicationLogger(typeof(AccountRegistration)))
        {
        }

        public AccountRegistration(IApplicationLogger applicationLogger)
        {
            if (applicationLogger == null)
            {
                throw new ArgumentNullException(nameof(applicationLogger));
            }

            _applicationLogger = applicationLogger;
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
                IsEnabled = true;
                ShowRegistrationFault(registrationException);
            }
            catch (Exception connectionException) when (
                connectionException is CommunicationException ||
                connectionException is TimeoutException)
            {
                ShowConnectionError(connectionException);
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
            string resourcePrefix = "TextBlock_" + validationResult.ErrorCode;

            string errorTitle = Properties.Resources.ResourceManager.GetString(
                resourcePrefix + "Title", Properties.Resources.Culture)
                ?? Properties.Resources.TextBlock_RegistrationInvalidInputTitle;

            string errorMessage = Properties.Resources.ResourceManager.GetString(
                resourcePrefix + "Message", Properties.Resources.Culture)
                ?? Properties.Resources.TextBlock_RegistrationInvalidInputMessage;

            if (validationResult.ErrorCode == InvalidFieldLengthErrorCode)
            {
                errorMessage = string.Format(
                    errorMessage, RegistrationLimits.TextFieldMaximumLength);
            }

            IsEnabled = true;
            ShowRegistrationError(errorTitle, errorMessage);
            FocusRegistrationField(validationResult.FieldName);
        }

        private void ShowRegistrationError(string title, string message)
        {
            AccountDialog accountDialog = new AccountDialog(title, message);
            accountDialog.ShowOver(Window.GetWindow(this));
        }

        private void ShowConnectionError(Exception errorException)
        {
            _applicationLogger.LogError(
                errorException,
                "Registration service request failed.");

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

            ShowSaveError(registrationException);
        }

        private void ShowSaveError(Exception errorException)
        {
            _applicationLogger.LogError(
                errorException,
                "Account registration could not be completed.");

            ShowRegistrationError(
                Properties.Resources.TextBlock_RegistrationSaveErrorTitle,
                Properties.Resources.TextBlock_RegistrationSaveErrorMessage);
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
