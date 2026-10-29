using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    public interface ILobbyCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnLobbyUpdated(Lobby lobby);
    }
}
