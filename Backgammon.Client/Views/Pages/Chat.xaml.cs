using System;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;

using Backgammon.Client.ChatReference;
using Backgammon.Client.Controllers.Chat;

namespace Backgammon.Client.Views.Pages
{
    public partial class Chat : Window
    {
        private readonly ChatController _chatController;
        private bool _isSendingMessage;

        public Chat()
        {
            InitializeComponent();

            _chatController = new ChatController();
            _chatController.MessageReceived += OnMessageReceived;
            _chatController.LobbyChatClosed += OnLobbyChatClosed;

            Loaded += OnChatLoaded;
            Closed += OnChatClosed;
        }

        private void OnChatLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ChatMessage[] previousMessages = _chatController.JoinChat();

            foreach (ChatMessage message in previousMessages)
            {
                AddMessage(message);
            }
        }

        private async void OnSendMessage(object sender, RoutedEventArgs routedEventArgs)
        {
            await SendCurrentMessageAsync();
        }

        private async void OnMessageKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key != Key.Enter)
            {
                return;
            }

            keyEventArgs.Handled = true;
            await SendCurrentMessageAsync();
        }

        private async Task SendCurrentMessageAsync()
        {
            if (_isSendingMessage)
            {
                return;
            }

            string message = textBoxMessage.Text;

            _isSendingMessage = true;

            try
            {
                bool messageWasSent = await _chatController.SendMessageAsync(message);

                if (messageWasSent)
                {
                    textBoxMessage.Clear();
                    textBoxMessage.Focus();
                }
            }
            finally
            {
                _isSendingMessage = false;
            }
        }

        private void OnMessageReceived(ChatMessage message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Action<ChatMessage> displayMessageAction = AddMessage;
                Dispatcher.BeginInvoke(displayMessageAction, message);
                return;
            }

            AddMessage(message);
        }

        private void AddMessage(ChatMessage message)
        {
            if (message == null)
            {
                return;
            }

            string displayedMessage = message.SenderUsername + ": " + message.Message;

            listBoxMessages.Items.Add(displayedMessage);
            listBoxMessages.ScrollIntoView(displayedMessage);
        }

        private void OnLobbyChatClosed()
        {
            if (!Dispatcher.CheckAccess())
            {
                Action closeChatAction = Close;
                Dispatcher.BeginInvoke(closeChatAction);
                return;
            }

            Close();
        }

        private void OnChatClosed(object sender, EventArgs eventArgs)
        {
            _chatController.MessageReceived -= OnMessageReceived;
            _chatController.LobbyChatClosed -= OnLobbyChatClosed;
            _chatController.LeaveChat();
        }

        private void OnBackToLobby(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }
    }
}
