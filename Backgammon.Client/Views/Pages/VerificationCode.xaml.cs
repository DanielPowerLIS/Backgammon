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
                ShowInvalidFormatMessage();
                FocusCodeInput(0);
            }
            else
            {
                ShowSuccessfulVerification();
            }
        }

        private async Task VerifyWithFeedbackAsync()
        {
            try
            {
                await SubmitVerificationAsync();
            }
            catch (FaultException<VerificationFault> verificationException)
            {
                ApplicationLogger.LogWarning("Verification was rejected. Code: {0}",
                    verificationException.Detail.ErrorCode);

                ShowVerificationUnavailableMessage();
            }
            catch (FaultException<AuthenticationFault> authenticationException)
            {
                ApplicationLogger.LogWarning("Authentication was rejected. Code: {0}",
                    authenticationException.Detail.ErrorCode);

                ShowVerificationUnavailableMessage();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "A communication failure prevented verification from completing.");

                ShowVerificationUnavailableMessage();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The verification request exceeded the time limit.");

                ShowVerificationUnavailableMessage();
            }
        }

        private void ShowInvalidFormatMessage()
        {
            var dialog = new AccountDialog(
                Properties.Resources.TextBlock_VerificationErrorTitle,
                Properties.Resources.TextBlock_VerificationInvalidFormatMessage);

            Window owner = Window.GetWindow(this);
            dialog.ShowOver(owner);
        }

        private void ShowVerificationUnavailableMessage()
        {
            var dialog = new AccountDialog(
                Properties.Resources.TextBlock_VerificationErrorTitle,
                Properties.Resources.TextBlock_VerificationUnavailableMessage);

            Window owner = Window.GetWindow(this);
            dialog.ShowOver(owner);
        }

        private void ShowSuccessfulVerification()
        {
            var dialog = new AccountDialog(
                Properties.Resources.TextBlock_LoginSuccessTitle,
                Properties.Resources.TextBlock_LoginSuccessMessage);

            Window owner = Window.GetWindow(this);

            dialog.ShowOver(owner);

            NavigationService.Navigate(new MainMenu());
        }
    }
}
