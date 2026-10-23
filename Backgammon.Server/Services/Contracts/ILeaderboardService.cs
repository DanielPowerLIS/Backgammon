using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface ILeaderboardService
    {
        [OperationContract]
        [FaultContract(typeof(LeaderboardFault))]
        PlayerStanding[] GetLeaderboard();


    }

}
