using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract(CallbackContract = typeof(IChatCallback))]
    public interface IChatService
    {
        [OperationContract]
        [FaultContract(typeof(ChatFault))]
        List<ChatMessage> JoinLobbyChat(string sessionId, string roomId);

        [OperationContract]
        [FaultContract(typeof(ChatFault))]
        void SendMessage(string sessionId, string roomId, string message);

        [OperationContract]
        [FaultContract(typeof(ChatFault))]
        void LeaveLobbyChat(string sessionId, string roomId);
    }
}
