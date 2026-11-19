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
        MatchPreparation RegisterPlayerReady(string sessionId, string roomId, string matchId);

        [OperationContract]
        [FaultContract(typeof(MatchFault))]
        MatchPreparation RollOpeningDie(string sessionId, PlayerOpeningRoll playerRoll);

        [OperationContract]
        [FaultContract(typeof(MatchFault))]
        MatchState RollDice(string sessionId, PlayerDiceRoll playerRoll);

        [OperationContract]
        [FaultContract(typeof(MatchFault))]
        MatchState MoveChecker(string sessionId, PlayerCheckerMove playerMove);

        [OperationContract]
        [FaultContract(typeof(MatchFault))]
        MatchPreparation GetMatchPreparation( string sessionId, string roomId, string matchId);
    }
}