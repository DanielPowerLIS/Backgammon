using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.ChatReference;
using Backgammon.Client.Controllers.Lobby;
using Backgammon.Client.Controllers.Session;
using Backgammon.Client.Settings.Logging;
using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;
using CurrentPlayerSession = Backgammon.Client.AuthenticationReference.PlayerSession;

namespace Backgammon.Client.Controllers.Chat
{
    internal class ChatController
    {
        private const string EndpointConfigurationName = "NetTcpBinding_IChatService";

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(ChatController));

        private readonly ChatCallback _chatCallback;

        private ChatServiceClient _serviceClient;
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

            if (_serviceClient != null)
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

            _serviceClient = new ChatServiceClient(callbackContext, EndpointConfigurationName);

            try
            {
                _serviceClient.Open();

                previousMessages = _serviceClient.JoinLobbyChat(_sessionId, _roomId);

                _isJoined = true;

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
                await _serviceClient.SendMessageAsync(_sessionId, _roomId, message);

                return true;
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
            if (_serviceClient == null)
            {
                return;
            }

            try
            {
                if (_isJoined && _serviceClient.State == CommunicationState.Opened)
                {
                    _serviceClient.LeaveLobbyChat(_sessionId, _roomId);
                }
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
            catch (InvalidOperationException contextException)
            {
                ApplicationLogger.LogError(contextException, "The player session or lobby is unavailable for chat.");

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
            bool isJoined = _isJoined && _serviceClient != null && _serviceClient.State == CommunicationState.Opened;

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
            ChatServiceClient serviceClient = _serviceClient;
            _serviceClient = null;

            if (serviceClient == null)
            {
                return;
            }

            if (serviceClient.State == CommunicationState.Faulted)
            {
                serviceClient.Abort();
                return;
            }

            try
            {
                serviceClient.Close();
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException, "The chat channel could not close normally.");

                serviceClient.Abort();
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException, "Closing the chat channel exceeded the time limit.");

                serviceClient.Abort();
            }
        }

        private void AbortServiceClient()
        {
            if (_serviceClient == null)
            {
                return;
            }

            _serviceClient.Abort();
            _serviceClient = null;
            _isJoined = false;
        }
    }
}
