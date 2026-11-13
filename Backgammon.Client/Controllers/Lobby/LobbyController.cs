using System;
using System.Threading.Tasks;

using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Session;
using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Controllers.Lobby
{
    internal class LobbyController
    {

        public event Action LobbyUpdated;

        public event Action<bool> LobbyClosed;

        private readonly LobbyClient _lobbyClient;

        public LobbyController()
        {
            _lobbyClient = new LobbyClient();
        }

        public async Task LeaveLobbyAsync()
        {
            string sessionId = PlayerSessionContext.GetSession().SessionId;

            CurrentLobby lobby = LobbyContext.GetLobby();
            string roomId = lobby.RoomId;

            await _lobbyClient.LeaveLobbyAsync(sessionId, roomId);

            LobbyContext.ClearLobby();
        }

        public void StartListening()
        {
            LobbyContext.LobbyUpdated -= OnLobbyUpdated;
            LobbyContext.LobbyUpdated += OnLobbyUpdated;

            LobbyContext.LobbyClosed -= OnLobbyClosed;
            LobbyContext.LobbyClosed += OnLobbyClosed;
        }

        public void StopListening()
        {
            LobbyContext.LobbyUpdated -= OnLobbyUpdated;
            LobbyContext.LobbyClosed -= OnLobbyClosed;
        }

        public string GetHostUsername()
        {
            CurrentLobby lobby = LobbyContext.GetLobby();
            string hostUsername = lobby.HostUsername;

            return hostUsername;
        }

        public string GetGuestUsername()
        {
            CurrentLobby lobby = LobbyContext.GetLobby();
            string guestUsername = lobby.GuestUsername;

            return guestUsername;
        }

        public string GetRoomCode()
        {
            CurrentLobby lobby = LobbyContext.GetLobby();
            string roomCode = lobby.RoomId;

            return roomCode;
        }

        public void ClearLobby()
        {
            LobbyContext.ClearLobby();
        }

        private void OnLobbyUpdated()
        {
            Action lobbyUpdatedHandler = LobbyUpdated;

            if (lobbyUpdatedHandler != null)
            {
                lobbyUpdatedHandler.Invoke();
            }
        }

        private void OnLobbyClosed(bool wasKicked)
        {
            Action<bool> lobbyClosedHandler = LobbyClosed;

            if (lobbyClosedHandler != null)
            {
                lobbyClosedHandler.Invoke(wasKicked);
            }
        }
    }
}