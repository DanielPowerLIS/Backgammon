using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract(CallbackContract = typeof(IGameCallback))]
    public interface IGameService
    {
        [OperationContract]
        [FaultContract(typeof(MatchFault))]
        MatchState GetMatchState(string sessionId, string roomId, string matchId);

        [OperationContract]
        [FaultContract(typeof(MatchFault))]
        MatchState RegisterPlayerReady(string sessionId, string roomId, string matchId);
    }
}