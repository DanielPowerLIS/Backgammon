using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon.Server.Services.ExceptionCode
{
    internal static class ErrorCodes
    {
        internal const string ServiceUnavailable = "ServiceUnavailable";
        internal const string SessionExpired = "SessionExpired";
        internal const string AccessDenied = "AccessDenied";
        internal const string InvalidRequest = "InvalidRequest";
        internal const string InvalidInput = "InvalidInput";
        internal const string MatchUnavailable = "MatchUnavailable";
        internal const string InvalidCredentials = "InvalidCredentials";
        internal const string LobbyUnavailable = "LobbyUnavailable";
        internal const string LobbyLeaveFailed = "LobbyLeaveFailed";
        internal const string EmailAlreadyRegistered = "EmailAlreadyRegistered";
        internal const string UsernameAlreadyTaken = "UsernameAlreadyTaken";
        internal const string LobbyJoinFailed = "LobbyJoinFailed";
        internal const string InvalidRoomCode = "InvalidRoomCode";
        internal const string ProfileNotFound = "ProfileNotFound";
        internal const string RequestNotFound = "RequestNotFound";
        internal const string RequestExpired = "RequestExpired";
        internal const string InvalidCode = "InvalidCode";
        internal const string AttemptsExhausted = "AttemptsExhausted";
    }
}
