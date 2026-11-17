using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    public interface IGameCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMatchStateUpdated(MatchState matchState);
    }
}