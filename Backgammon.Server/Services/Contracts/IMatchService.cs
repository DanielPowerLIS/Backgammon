

using Backgammon.Server.Services.Data;
using System.ServiceModel;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface IMatchService
    {
        [OperationContract]
        [FaultContract(typeof(MatchFault))]
        MatchSession InitializeMatch(string sessionId, string roomId);

        [OperationContract]
        [FaultContract(typeof(MatchFault))]
        MatchSession JoinMatch(string sessionId, string roomId, string matchId);


        [OperationContract]
        void MoveChecker(int fromPosition, int toPosition);


    }
}
