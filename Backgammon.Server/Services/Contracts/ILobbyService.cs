using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract(CallbackContract = typeof(ILobbyCallback))]
    public interface ILobbyService
    {
        [OperationContract]
        Lobby CreateLobby(int idUser);

        [OperationContract]
        bool JoinLobby(int idUser, string roomId);
    }
}
