using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract(CallbackContract = typeof(IChatCallback))]
    public interface IChatService
    {
        [OperationContract]
        List<ChatMessage> JoinLobbyChat(string sessionId, string roomId);

        [OperationContract]
        void SendMessage(string sessionId, string roomId, string message);

        [OperationContract]
        void LeaveLobbyChat(string sessionId, string roomId);
    }
}
