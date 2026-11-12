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
    }
}
