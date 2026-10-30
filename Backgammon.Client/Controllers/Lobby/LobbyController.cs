using System;

using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Controllers.Lobby
{
    internal class LobbyController
    {

        public event Action LobbyUpdated;

        public void StartListening()
        {
            LobbyContext.LobbyUpdated -= OnLobbyUpdated;
            LobbyContext.LobbyUpdated += OnLobbyUpdated;
        }

        public void StopListening()
        {
            LobbyContext.LobbyUpdated -= OnLobbyUpdated;
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

        private void OnLobbyUpdated()
        {
            Action lobbyUpdatedHandler = LobbyUpdated;

            if (lobbyUpdatedHandler != null)
            {
                lobbyUpdatedHandler.Invoke();
            }
        }
    }
}