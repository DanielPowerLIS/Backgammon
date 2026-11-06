using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.ChatReference;

namespace Backgammon.Client.Comunication
{
    internal class ChatClient
    {
        private const string EndpointConfigurationName = "NetTcpBinding_IChatService";

        private readonly ChatServiceClient _serviceClient;

        public ChatClient(InstanceContext callbackContext)
        {
            _serviceClient = new ChatServiceClient(callbackContext, EndpointConfigurationName);
        }

        public bool IsOpen
        {
            get
            {
                bool isOpen = _serviceClient.State == CommunicationState.Opened;

                return isOpen;
            }
        }

        public void Open()
        {
            _serviceClient.Open();
        }

        public ChatMessage[] JoinLobbyChat(string sessionId, string roomId)
        {
            ChatMessage[] previousMessages = _serviceClient.JoinLobbyChat(sessionId, roomId);

            return previousMessages;
        }

        public async Task SendMessageAsync(string sessionId, string roomId, string message)
        {
            await _serviceClient.SendMessageAsync(sessionId, roomId, message);
        }

        public void LeaveLobbyChat(string sessionId, string roomId)
        {
            _serviceClient.LeaveLobbyChat(sessionId, roomId);
        }

        public void Close()
        {
            if (_serviceClient.State == CommunicationState.Faulted)
            {
                _serviceClient.Abort();
                return;
            }

            _serviceClient.Close();
        }

        public void Abort()
        {
            _serviceClient.Abort();
        }
    }
}
