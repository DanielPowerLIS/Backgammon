using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.ChatReference;
using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Lobby;
using Backgammon.Client.Controllers.Session;
using Backgammon.Client.Settings.Logging;
using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;
using CurrentPlayerSession = Backgammon.Client.AuthenticationReference.PlayerSession;

namespace Backgammon.Client.Controllers.Chat
{
    internal class ChatController
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(ChatController));

        private readonly ChatCallback _chatCallback;

        private ChatClient _chatClient;
        private string _sessionId;
        private string _roomId;
        private bool _isJoined;

        public event Action<ChatMessage> MessageReceived;

        public event Action LobbyChatClosed;

        public ChatController()
        {
            _chatCallback = new ChatCallback();
            _chatCallback.MessageReceived += OnMessageReceived;
            _chatCallback.LobbyChatClosed += OnLobbyChatClosed;
        }

        public ChatMessage[] JoinChat()
        {
            ChatMessage[] previousMessages = new ChatMessage[0];

            if (_chatClient != null)
            {
                ApplicationLogger.LogWarning("A chat connection already exists for this controller.");

                return previousMessages;
            }

            bool hasLobbyConnectionData = StoreLobbyConnectionData();

            if (!hasLobbyConnectionData)
            {
                return previousMessages;
            }

            var callbackContext = new InstanceContext(_chatCallback);
            _chatClient = new ChatClient(callbackContext);

            try
            {
                _chatClient.Open();

                previousMessages = _chatClient.JoinLobbyChat(_sessionId, _roomId);

                _isJoined = true;

                return previousMessages;
            }
            catch (FaultException<ChatFault> chatFault)
            {
                ApplicationLogger.LogWarning(
                    "Joining the chat was rejected with code {0}.",
                    chatFault.Detail.ErrorCode);

                AbortServiceClient();

                return previousMessages;
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException, "The request to join the lobby chat failed.");

                AbortServiceClient();
                return previousMessages;
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The request to join the lobby chat exceeded the time limit.");

                AbortServiceClient();
                return previousMessages;
            }
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            if (String.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            bool isJoined = IsJoined();

            if (!isJoined)
            {
                return false;
            }

            try
            {
                await _chatClient.SendMessageAsync(_sessionId, _roomId, message);

                return true;
            }
            catch (FaultException<ChatFault> chatFault)
            {
                ApplicationLogger.LogWarning(
                    "Sending the chat message was rejected with code {0}.",
                    chatFault.Detail.ErrorCode);

                AbortServiceClient();

                return false;
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException, "The chat message could not be sent.");

                AbortServiceClient();
                return false;
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException, "Sending the chat message exceeded the time limit.");

                AbortServiceClient();
                return false;
            }
        }

        public void LeaveChat()
        {
            if (_chatClient == null)
            {
                return;
            }

            try
            {
                if (_isJoined && _chatClient.IsOpen)
                {
                    _chatClient.LeaveLobbyChat(_sessionId, _roomId);
                }
            }
            catch (FaultException<ChatFault> chatFault)
            {
                ApplicationLogger.LogWarning(
                    "Leaving the chat was rejected with code {0}.",
                    chatFault.Detail.ErrorCode);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException, "The request to leave the lobby chat failed.");
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException, "Leaving the lobby chat exceeded the time limit.");
            }
            finally
            {
                CloseServiceClient();
                _isJoined = false;
            }
        }

        private bool StoreLobbyConnectionData()
        {
            CurrentPlayerSession playerSession;
            CurrentLobby lobby;

            try
            {
                playerSession = PlayerSessionContext.GetSession();
                lobby = LobbyContext.GetLobby();
            }
            catch (NullReferenceException nullReferenceException)
            {
                ApplicationLogger.LogError(nullReferenceException, "The player session or lobby is unavailable for chat.");

                return false;
            }

            if (String.IsNullOrWhiteSpace(playerSession.SessionId))
            {
                ApplicationLogger.LogWarning("The player session does not contain a session ID.");

                return false;
            }

            if (String.IsNullOrWhiteSpace(lobby.RoomId))
            {
                ApplicationLogger.LogWarning("The current lobby does not contain a room ID.");

                return false;
            }

            _sessionId = playerSession.SessionId;
            _roomId = lobby.RoomId;

            return true;
        }

        private bool IsJoined()
        {
            bool isJoined = _isJoined && _chatClient != null && _chatClient.IsOpen;

            return isJoined;
        }

        private void OnMessageReceived(ChatMessage message)
        {
            Action<ChatMessage> messageReceivedHandler = MessageReceived;

            if (messageReceivedHandler != null)
            {
                messageReceivedHandler.Invoke(message);
            }
        }

        private void OnLobbyChatClosed()
        {
            Action lobbyChatClosedHandler = LobbyChatClosed;

            if (lobbyChatClosedHandler != null)
            {
                lobbyChatClosedHandler.Invoke();
            }
        }

        private void CloseServiceClient()
        {
            ChatClient chatClient = _chatClient;
            _chatClient = null;

            if (chatClient == null)
            {
                return;
            }

            try
            {
                chatClient.Close();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException, "The chat channel could not close normally.");

                chatClient.Abort();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException, "Closing the chat channel exceeded the time limit.");

                chatClient.Abort();
            }
        }

        private void AbortServiceClient()
        {
            if (_chatClient == null)
            {
                return;
            }

            _chatClient.Abort();
            _chatClient = null;
            _isJoined = false;
        }
    }
}
