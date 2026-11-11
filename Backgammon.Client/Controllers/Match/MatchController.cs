using System.Threading.Tasks;

using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Lobby;
using Backgammon.Client.Controllers.Session;
using Backgammon.Client.MatchReference;

namespace Backgammon.Client.Controllers.Match
{
    internal class MatchController
    {
        private readonly MatchClient _matchClient;
        public MatchController()
        {
            _matchClient = new MatchClient();
        }
        public async Task<MatchSession> InitializeMatchAsync()
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;
            string roomId = LobbyContext.GetLobby().RoomId;


            MatchSession matchSession = await _matchClient.InitializeMatchAsync(sessionId, roomId);

            return matchSession;
        }

        public bool CanStartMatch()
        {
            int playerId = PlayerSessionContext.GetSession().PlayerId;
            var lobby = LobbyContext.GetLobby();

            bool isHost = lobby.HostPlayerId == playerId;
            bool hasGuest = lobby.GuestPlayerId != 0;
            bool canStartMatch = isHost && hasGuest;

            return canStartMatch;
        }

        public bool IsLobbyHost()
        {
            int playerId = PlayerSessionContext.GetSession().PlayerId;
            int hostPlayerId = LobbyContext.GetLobby().HostPlayerId;
            bool isLobbyHost = playerId == hostPlayerId;

            return isLobbyHost;
        }

        public async Task<MatchSession> JoinMatchAsync(string roomId, string matchId)
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;
            MatchSession matchSession =
                await _matchClient.JoinMatchAsync(sessionId, roomId, matchId);

            return matchSession;
        }
    }
}
