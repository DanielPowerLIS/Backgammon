using System;

using Backgammon.Client.ChatReference;


namespace Backgammon.Client.Controllers.Chat
{
    internal class ChatCallback : IChatServiceCallback
    {
        public event Action<ChatMessage> MessageReceived;

        public event Action LobbyChatClosed;
        public void OnLobbyChatClosed()
        {
            Action lobbyChatClosedHandler = LobbyChatClosed;

            if (lobbyChatClosedHandler != null)
            {
                lobbyChatClosedHandler.Invoke();
            }

        }

        public void OnMessageReceived(ChatMessage message)
        {
            Action<ChatMessage> messageReceivedHandler = MessageReceived;

            if (messageReceivedHandler != null)
            {
                messageReceivedHandler.Invoke(message);
            }
        }
    }
}
