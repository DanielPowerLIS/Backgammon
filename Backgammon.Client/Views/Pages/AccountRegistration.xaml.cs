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
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data.Entity.Infrastructure;
using System.Security.Cryptography;

using Backgammon.Client.Repositories;
using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Services;
using AccountEntity = Backgammon.Client.Data.Account;
using ProfileEntity = Backgammon.Client.Data.Profile;

namespace Backgammon.Client.Views.Pages
{
    public partial class AccountRegistration : Page
    {
        private static readonly int MinimumPasswordLength = 8;
        private static readonly int DefaultAvatarId = 1;
        private static readonly int OfflineStateId = 3;
        public AccountRegistration()
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

        
        private bool ValidateLocalFields()
        {
            if (FocusFirstEmptyField())
            {
                ShowRegistrationError(
                    Properties.Resources.TextBlock_RequiredFieldsTitle,
                    Properties.Resources.TextBlock_RequiredFieldsMessage);

                FocusFirstEmptyField();
                return false;
            }

            if (!HasValidPasswordFormat())
            {
                ShowRegistrationError(
                    Properties.Resources.TextBlock_InvalidPasswordTitle,
                    Properties.Resources.TextBlock_InvalidPasswordMessage);

                passwordBoxPassword.Focus();
                return false;
            }

            return true;
        }

        private async Task<bool> CheckEmailAvailabilityAsync()
        {
            RegistrationRepository registrationRepository = new RegistrationRepository();
            string emailAddress = textBoxEmail.Text.Trim();
            bool emailRegistered =
                await registrationRepository.IsEmailRegisteredAsync(emailAddress);

            if (emailRegistered)
            {
                ShowRegistrationError(
                    Properties.Resources.TextBlock_EmailAlreadyUsedTitle,
                    Properties.Resources.TextBlock_EmailAlreadyUsedMessage);

                textBoxEmail.Focus();
                return false;
            }

            return true;
        }

        private async Task<bool> CheckUsernameAvailabilityAsync()
        {
            RegistrationRepository registrationRepository = new RegistrationRepository();
            string username = textBoxUsername.Text.Trim();
            bool usernameRegistered =
                await registrationRepository.IsUsernameRegisteredAsync(username);

            if (usernameRegistered)
            {
                ShowRegistrationError(
                    Properties.Resources.TextBlock_UsernameAlreadyUsedTitle,
                    Properties.Resources.TextBlock_UsernameAlreadyUsedMessage);

                textBoxUsername.Focus();
                return false;
            }

            return true;
        }

        private void ShowDatabaseError(Exception exception)
        {
            Trace.TraceError(exception.ToString());

            ShowRegistrationError(
                Properties.Resources.TextBlock_RegistrationConnectionErrorTitle,
                Properties.Resources.TextBlock_RegistrationConnectionErrorMessage);
        }

        private async void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateLocalFields())
            {
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
                await SaveValidatedAccountAsync();
            }
            catch (EntityException exception)
            {
                ShowDatabaseError(exception);
            }
            catch (SqlException exception)
            {
                ShowDatabaseError(exception);
            }
            catch (Exception exception) when (
                exception is DbUpdateException ||
                exception is CryptographicException ||
                exception is InvalidOperationException)
            {
                ShowSaveError(exception);
            }
        }

        private async Task SaveValidatedAccountAsync()
        {
            if (!await CheckEmailAvailabilityAsync())
            {
                return;
            }

            if (!await CheckUsernameAvailabilityAsync())
            {
                return;
            }

            AccountEntity account = await BuildAccountAsync();
            RegistrationRepository repository = new RegistrationRepository();

            await repository.CreateAccountAsync(account);
            ShowRegistrationSuccess();
        }

        private async Task<AccountEntity> BuildAccountAsync()
        {
            string enteredPassword = passwordBoxPassword.Password;
            PasswordHashService hashService = new PasswordHashService();
            string passwordHash = await Task.Run(
                () => hashService.HashPassword(enteredPassword));

            ProfileEntity profile = new ProfileEntity
            {
                Username = textBoxUsername.Text.Trim(),
                Wins = 0,
                Points = 0,
                IdAvatarFK = DefaultAvatarId,
                IdAvailableStateFK = OfflineStateId
            };

            return new AccountEntity
            {
                Email = textBoxEmail.Text.Trim(),
                Password = passwordHash,
                RegistrationDate = DateTime.UtcNow,
                Name = textBoxFirstName.Text.Trim(),
                PaternalSurname = textBoxPaternalLastName.Text.Trim(),
                MaternalSurname = textBoxMaternalLastName.Text.Trim(),
                Profile = profile
            };
        }

        private void ShowSaveError(Exception exception)
        {
            Trace.TraceError(exception.ToString());

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
    }
}
