using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract(CallbackContract = typeof(IPreinitiationCallback))]
    public interface IPreinitiationService
    {
        [OperationContract]
        [FaultContract(typeof(PreinitiationFault))]
        Lobby CreateLobby(string sessionId);

        [OperationContract]
        [FaultContract(typeof(PreinitiationFault))]
        Lobby JoinLobby(string sessionId, string roomId);
    }
}
