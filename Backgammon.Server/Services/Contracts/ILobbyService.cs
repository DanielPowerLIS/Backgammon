using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface ILobbyService
    {
        [OperationContract]
        [FaultContract(typeof(LobbyFault))]
        void LeaveLobby(string sessionId, string roomId);
    }
}