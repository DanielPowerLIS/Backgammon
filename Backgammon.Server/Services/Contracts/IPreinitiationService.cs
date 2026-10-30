using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract(CallbackContract = typeof(IPreinitiationCallback))]
    public interface IPreinitiationService
    {
        [OperationContract]
        Lobby CreateLobby(string sessionId);

        [OperationContract]
        bool JoinLobby(string sessionId, string roomId);
    }
}
