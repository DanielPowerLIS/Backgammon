using System.ServiceModel;
using System.Threading.Tasks;
using System.Linq;

using Backgammon.Client.Controllers.Data;
using Backgammon.Client.Comunication;
using Backgammon.Client.Controllers.Callbacks;
using Backgammon.Client.Controllers.Lobby;
using Backgammon.Client.Controllers.Session;
using CurrentLobby = Backgammon.Client.PreinitiationReference.Lobby;

namespace Backgammon.Client.Controllers.Preinitiation
{
    internal class PreinitiationController
    {
        private readonly PreinitiationClient _preinitiationClient;
        private readonly PreinitiationCallback _preinitiationCallback;

        private static readonly int RoomCodeLength = 6;

        private static readonly string AllowedRoomCodeCharacters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private static readonly string InvalidRoomCodeErrorCode = "InvalidRoomCode";

        public PreinitiationController()
        {
            _preinitiationCallback = new PreinitiationCallback();
            InstanceContext context = new InstanceContext(_preinitiationCallback);
            _preinitiationClient = new PreinitiationClient(context);
        }

        public PreinitiationCallback Callback
        {
            get 
            {
                return _preinitiationCallback;
            }
        }

        public bool CreateLobby() 
        {
            bool lobbyCreated = false;

            string sessionId = PlayerSessionContext.GetSession().SessionId;

            CurrentLobby newLobby = _preinitiationClient.CreateLobby(sessionId);

            LobbyContext.StoreLobby(newLobby);

            lobbyCreated = true;

            return lobbyCreated;
        }

        public async Task<InputValidationResult> JoinLobbyAsync(string roomId)
        {
            InputValidationResult validationResult = ValidateRoomCode(roomId);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            string sessionId = PlayerSessionContext.GetSession().SessionId;

            CurrentLobby joinedLobby = await _preinitiationClient.JoinLobbyAsync(sessionId, roomId);

            LobbyContext.StoreLobby(joinedLobby);

            return validationResult;
        }

        private InputValidationResult ValidateRoomCode(string roomId)
        {
            bool hasValidLength = roomId != null && roomId.Length == RoomCodeLength;
            bool isValid = false;

            if (hasValidLength)
            {
                isValid = HasOnlyAllowedCharacters(roomId);
            }

            var validationResult = new InputValidationResult
            {
                IsValid = isValid
            };

            if (!isValid)
            {
                validationResult.ErrorCode = InvalidRoomCodeErrorCode;
                validationResult.FieldName = nameof(CurrentLobby.RoomId);
            }

            return validationResult;
        }

        private bool HasOnlyAllowedCharacters(string roomId)
        {
            foreach (char character in roomId)
            {
                string characterText = character.ToString();
                bool isAllowed = AllowedRoomCodeCharacters.Contains(characterText);

                if (!isAllowed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
