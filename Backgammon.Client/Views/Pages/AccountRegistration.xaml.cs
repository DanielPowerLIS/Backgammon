using System;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Backgammon.Client.Controllers.Registration;
using Backgammon.Client.Models;
using Backgammon.Client.RegistrationReference;
using Backgammon.Client.Services.Logging;
using Backgammon.Client.Views.Dialog;

using RegistrationAccount = Backgammon.Client.RegistrationReference.Account;

namespace Backgammon.Client.Views.Pages
{
    public partial class AccountRegistration : Page
    {
        private static readonly int MinimumPasswordLength = 8;
        private readonly IApplicationLogger _applicationLogger;
        private readonly RegistrationController _registrationController;
        private static readonly string InvalidInputErrorCode = "InvalidInput";
        private static readonly string EmailAlreadyRegisteredErrorCode = "EmailAlreadyRegistered";
        private static readonly string UsernameAlreadyTakenErrorCode = "UsernameAlreadyTaken";
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

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;

            textBlockPasswordPlaceholder.Visibility =
                string.IsNullOrEmpty(passwordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private bool FocusFirstEmptyField()
        {
            TextBox[] requiredTextFields =
            {
                textBoxEmail, textBoxFirstName, textBoxPaternalLastName,
                textBoxMaternalLastName, textBoxUsername
            };

            foreach (TextBox textField in requiredTextFields)
            {
                if (string.IsNullOrWhiteSpace(textField.Text))
                {
                    textField.Focus();
                    return true;
                }
            }

            if (string.IsNullOrWhiteSpace(passwordBoxPassword.Password))
            {
                passwordBoxPassword.Focus();
                return true;
            }

            return false;
        }

        private bool HasValidPasswordFormat()
        {
            string enteredPassword = passwordBoxPassword.Password;
            bool hasMinimumLength = enteredPassword.Length >= MinimumPasswordLength;
            bool hasSpecialCharacter = enteredPassword.Any(
                character => char.IsPunctuation(character) || char.IsSymbol(character));

            return hasMinimumLength && hasSpecialCharacter;
        }

        private void ShowRegistrationError(string title, string message)
        {
            AccountDialog accountDialog = new AccountDialog(title, message);
            accountDialog.ShowOver(Window.GetWindow(this));
        }


        private RegistrationValidationResult ValidateLocalFields()
        {
            RegistrationValidationResult validationResult = ValidateRequiredFields();

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            validationResult = ValidateFieldLengths();
            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            validationResult = ValidateEmailFormat();
            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            return ValidatePasswordFormat();
        }

        private RegistrationValidationResult ValidateRequiredFields()
        {
            if (FocusFirstEmptyField())
            {
                return new RegistrationValidationResult
                {
                    IsValid = false,
                    ErrorTitle = Properties.Resources.TextBlock_RequiredFieldsTitle,
                    ErrorMessage = Properties.Resources.TextBlock_RequiredFieldsMessage
                };
            }

            return new RegistrationValidationResult { IsValid = true };
        }

        private RegistrationValidationResult ValidatePasswordFormat()
        {
            if (!HasValidPasswordFormat())
            {
                return new RegistrationValidationResult
                {
                    IsValid = false,
                    ErrorTitle = Properties.Resources.TextBlock_InvalidPasswordTitle,
                    ErrorMessage = Properties.Resources.TextBlock_InvalidPasswordMessage
                };
            }

            return new RegistrationValidationResult { IsValid = true };
        }

        private RegistrationValidationResult ValidateEmailFormat()
        {
            string emailAddress = textBoxEmail.Text.Trim();

            if (!HasValidEmailFormat(emailAddress))
            {
                textBoxEmail.Focus();
                return new RegistrationValidationResult
                {
                    IsValid = false,
                    ErrorTitle = Properties.Resources.TextBlock_InvalidEmailTitle,
                    ErrorMessage = Properties.Resources.TextBlock_InvalidEmailMessage
                };
            }

            return new RegistrationValidationResult { IsValid = true };
        }

        private bool HasValidEmailFormat(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress) || emailAddress.Any(char.IsWhiteSpace))
            {
                return false;
            }

            try
            {
                MailAddress parsedAddress = new MailAddress(emailAddress);
                return string.Equals(parsedAddress.Address, emailAddress, StringComparison.Ordinal) &&
                    Uri.CheckHostName(parsedAddress.Host) == UriHostNameType.Dns &&
                    !parsedAddress.Host.Contains("_") &&
                    parsedAddress.Host.Contains(".") &&
                    !parsedAddress.Host.EndsWith(".", StringComparison.Ordinal) &&
                    !parsedAddress.Host.Contains("..");
            }
            catch (FormatException)
            {
                return false;
            }
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

        private async void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            RegistrationValidationResult validationResult = ValidateLocalFields();

            if (!validationResult.IsValid)
            {
                ShowRegistrationError(
                    validationResult.ErrorTitle, validationResult.ErrorMessage);
                return;
            }

            buttonCreateAccount.IsEnabled = false;
            try
            {
                await RegisterAccountAsync();
            }
            finally
            {
                buttonCreateAccount.IsEnabled = true;
            }
        }

        private async Task RegisterAccountAsync()
        {
            try
            {
                RegistrationAccount accountInformation = BuildAccountInformation();

                await _registrationController.RegisterAccountAsync(accountInformation);

                ShowRegistrationSuccess();
            }
            catch (FaultException<RegistrationFault> registrationException)
            {
                ShowRegistrationFault(registrationException);
            }
            catch (Exception connectionException) when (
                connectionException is CommunicationException ||
                connectionException is TimeoutException)
            {
                ShowConnectionError(connectionException);
            }
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

        private void ShowRegistrationSuccess()
        {
            AccountDialog accountDialog = new AccountDialog(
                Properties.Resources.TextBlock_RegistrationSuccessTitle,
                Properties.Resources.TextBlock_RegistrationSuccessMessage);

            accountDialog.ShowOver(Window.GetWindow(this));
        }

        private void OnButtonSignInClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Login());
        }

        private TextBox[] GetRegistrationTextFields()
        {
            return new TextBox[]
            {
                textBoxEmail,
                textBoxFirstName,
                textBoxPaternalLastName,
                textBoxMaternalLastName,
                textBoxUsername
            };
        }

        private RegistrationValidationResult ValidateFieldLengths()
        {
            TextBox[] registrationFields = GetRegistrationTextFields();

            foreach (TextBox registrationField in registrationFields)
            {
                if (registrationField.Text.Trim().Length >
                    RegistrationLimits.TextFieldMaximumLength)
                {
                    return new RegistrationValidationResult
                    {
                        IsValid = false,
                        ErrorTitle = Properties.Resources.TextBlock_InvalidFieldLengthTitle,
                        ErrorMessage = string.Format(
                            Properties.Resources.TextBlock_InvalidFieldLengthMessage,
                            RegistrationLimits.TextFieldMaximumLength)
                    };
                }
            }

            return new RegistrationValidationResult { IsValid = true };
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
    }
}
