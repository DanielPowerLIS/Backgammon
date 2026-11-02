using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Backgammon.Client.AuthenticationReference;
using Backgammon.Client.Controllers.Data;
using Backgammon.Client.Controllers.Verification;
using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Views.Dialog;

namespace Backgammon.Client.Views.Pages
{
    public partial class VerificationCode : Page
    {
        private readonly string _requestId;
        private readonly TextBox[] _codeTextBoxes;
        private readonly VerificationController _verificationController;

        private static readonly string InvalidCodeErrorCode = "InvalidCode";
        private static readonly string RequestExpiredErrorCode = "RequestExpired";
        private static readonly string AttemptsExhaustedErrorCode = "AttemptsExhausted";
        private static readonly string RequestNotFoundErrorCode = "RequestNotFound";
        private static readonly string InvalidCredentialsErrorCode = "InvalidCredentials";

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(VerificationCode));

        public VerificationCode(string requestId)
        {
            InitializeComponent();

            _requestId = requestId;
            _verificationController = new VerificationController();

            _codeTextBoxes = new[]
            {
                textBoxDigit1, textBoxDigit2, textBoxDigit3,
                textBoxDigit4, textBoxDigit5, textBoxDigit6
            };

            ConfigureCodeInputs();
            Loaded += OnPageLoaded;
        }


        private void ConfigureCodeInputs()
        {
            foreach (TextBox codeTextBox in _codeTextBoxes)
            {
                codeTextBox.PreviewTextInput += OnCodeTextInput;
                codeTextBox.PreviewKeyDown += OnCodeKeyDown;
                codeTextBox.GotKeyboardFocus += OnCodeKeyboardFocus;
                DataObject.AddPastingHandler(codeTextBox, OnCodePasting);
            }
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            FocusCodeInput(0);
        }

        private void OnCodeTextInput(object sender, TextCompositionEventArgs e)
        {
            var codeTextBox = (TextBox)sender;

            WriteCodeText(codeTextBox, e.Text);
            e.Handled = true;
        }

        private void WriteCodeText(TextBox codeTextBox, string text)
        {
            int index = Array.IndexOf(_codeTextBoxes, codeTextBox);

            foreach (char character in text)
            {
                if (index >= _codeTextBoxes.Length)
                {
                    break;
                }

                _codeTextBoxes[index].Text = character.ToString();
                index++;
            }

            int targetIndex = Math.Min(index, _codeTextBoxes.Length - 1);
            FocusCodeInput(targetIndex);
        }

        private void OnCodeKeyDown(object sender, KeyEventArgs e)
        {
            var codeTextBox = (TextBox)sender;

            if (e.Key == Key.Back)
            {
                DeletePreviousCharacter(codeTextBox);
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                codeTextBox.Clear();
                e.Handled = true;
            }
        }

        private void DeletePreviousCharacter(TextBox codeTextBox)
        {
            if (codeTextBox.Text.Length > 0)
            {
                codeTextBox.Clear();
                return;
            }

            int index = Array.IndexOf(_codeTextBoxes, codeTextBox);

            if (index > 0)
            {
                int previousIndex = index - 1;
                _codeTextBoxes[previousIndex].Clear();
                FocusCodeInput(previousIndex);
            }
        }

        private void OnCodeKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var codeTextBox = (TextBox)sender;
            codeTextBox.SelectAll();
        }

        private void OnCodePasting(object sender, DataObjectPastingEventArgs e)
        {
            bool containsText = e.DataObject.GetDataPresent(DataFormats.UnicodeText);

            if (containsText)
            {
                var codeTextBox = (TextBox)sender;
                string text = (string)e.DataObject.GetData(DataFormats.UnicodeText);
                WriteCodeText(codeTextBox, text);
            }

            e.CancelCommand();
        }

        private void FocusCodeInput(int index)
        {
            TextBox codeTextBox = _codeTextBoxes[index];
            codeTextBox.Focus();
            codeTextBox.SelectAll();
        }

        private async void OnVerifyCodeClick(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;

            try
            {
                await VerifyWithFeedbackAsync();
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private string GetCode()
        {
            string[] codeParts =
            {
                textBoxDigit1.Text,
                textBoxDigit2.Text,
                textBoxDigit3.Text,
                textBoxDigit4.Text,
                textBoxDigit5.Text,
                textBoxDigit6.Text
            };

            string code = string.Concat(codeParts);

            return code;
        }

        private async Task SubmitVerificationAsync()
        {
            string code = GetCode();

            InputValidationResult validationResult = await _verificationController.ConfirmVerificationAsync(
                    _requestId, code);

            if (!validationResult.IsValid)
            {
                ShowVerificationMessage(
                    Properties.Resources.TextBlock_VerificationInvalidFormatMessage);

                textBoxDigit1.Focus();
                return;
            }

            ShowSuccessfulVerification();
        }

        private async Task VerifyWithFeedbackAsync()
        {
            try
            {
                await SubmitVerificationAsync();
            }
            catch (FaultException<VerificationFault> verificationException)
            {
                string errorCode = verificationException.Detail.ErrorCode;

                ApplicationLogger.LogWarning("Verification was rejected. Code: {0}", errorCode);

                ShowVerificationError(errorCode);
            }
            catch (FaultException<AuthenticationFault> authenticationException)
            {
                ShowAuthenticationError(authenticationException);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "A communication failure prevented verification from completing.");

                ShowVerificationMessage(Properties.Resources.TextBlock_VerificationUnavailableMessage);
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The verification request exceeded the time limit.");

                ShowVerificationMessage(  Properties.Resources.TextBlock_VerificationUnavailableMessage);
            }
        }

        private void ShowAuthenticationError(FaultException<AuthenticationFault> authenticationException)
        {
            string errorCode = authenticationException.Detail.ErrorCode;

            if (errorCode == InvalidCredentialsErrorCode)
            {
                ApplicationLogger.LogWarning("Verified sign-in was rejected. Code: {0}",
                    errorCode);

                ShowVerificationMessage(
                    Properties.Resources.TextBlock_LoginInvalidCredentialsMessage);

                NavigationService.Navigate(new Login());
                return;
            }

            ApplicationLogger.LogError(authenticationException,
                "The server could not complete verified sign-in. Code: {0}",
                errorCode);

            ShowVerificationMessage( Properties.Resources.TextBlock_VerificationUnavailableMessage);
        }

        private void ShowVerificationError(string errorCode)
        {
            string message = GetVerificationMessage(errorCode);

            ShowVerificationMessage(message);

            bool requiresNewRequest = errorCode == RequestExpiredErrorCode ||
                errorCode == AttemptsExhaustedErrorCode ||
                errorCode == RequestNotFoundErrorCode;

            if (requiresNewRequest)
            {
                NavigationService.Navigate(new Login());
            }
        }

        private string GetVerificationMessage(string errorCode)
        {
            if (errorCode == InvalidCodeErrorCode)
            {
                string invalidCodeMessage = Properties.Resources.TextBlock_VerificationInvalidCodeMessage;

                return invalidCodeMessage;
            }

            string requestMessage = GetRequestErrorMessage(errorCode);

            return requestMessage;
        }

        private string GetRequestErrorMessage(string errorCode)
        {
            string message = Properties.Resources.TextBlock_VerificationUnavailableMessage;

            if (errorCode == RequestExpiredErrorCode)
            {
                message = Properties.Resources.TextBlock_VerificationExpiredMessage;
            }
            else if (errorCode == AttemptsExhaustedErrorCode)
            {
                message =
                    Properties.Resources.TextBlock_VerificationAttemptsExhaustedMessage;
            }
            else if (errorCode == RequestNotFoundErrorCode)
            {
                message =
                    Properties.Resources.TextBlock_VerificationRequestNotFoundMessage;
            }

            return message;
        }

        private void ShowVerificationMessage(string message)
        {
            IsEnabled = true;

            var dialog = new AccountDialog(
                Properties.Resources.TextBlock_VerificationErrorTitle,
                message);

            Window owner = Window.GetWindow(this);

            dialog.ShowOver(owner);
        }

        private void ShowSuccessfulVerification()
        {
            IsEnabled = true;

            var dialog = new AccountDialog(
                Properties.Resources.TextBlock_LoginSuccessTitle,
                Properties.Resources.TextBlock_LoginSuccessMessage);

            Window owner = Window.GetWindow(this);

            dialog.ShowOver(owner);

            NavigationService.Navigate(new MainMenu());
        }
    }
}