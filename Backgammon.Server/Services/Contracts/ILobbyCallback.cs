using Backgammon.Server.Services.Data;
using System.ServiceModel;

namespace Backgammon.Server.Services.Contracts
{
    public interface ILobbyCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnLobbyUpdated(Lobby lobby);
    }
}
