using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.ExceptionCode;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.Services.Manager
{
    internal class VerificationManager
    {
        private static readonly TimeSpan RequestLifetime = TimeSpan.FromMinutes(5);

        private static readonly int MaximumAttempts = 5;

        private static readonly IApplicationLogger ApplicationLogger = 
            new Log4NetApplicationLogger(typeof(VerificationManager));

        private static readonly ConcurrentDictionary<string, VerificationRequest> PendingRequests = 
            new ConcurrentDictionary<string, VerificationRequest>();

        private readonly HashService _hashService;

        public VerificationManager()
        {
            _hashService = new HashService();
        }

        public int VerifyRequest(string requestId, string code)
        {
            VerificationRequest request = GetSavedRequest(requestId);

            lock (request.Queue)
            {
                GetSavedRequest(requestId);

                ValidateExpiration(request);
                ValidateCode(request, code);

                int accountId = request.AccountId;

                bool requestRemoved = RemoveRequest(requestId);

                if (!requestRemoved)
                {
                    ApplicationLogger.LogWarning(
                        "Verification could not be completed because the request is no longer available.");

                    throw CreateVerificationFault(ErrorCodes.RequestNotFound);
                }

                return accountId;
            }
        }

        private void ValidateExpiration(VerificationRequest request)
        {
            if (request.ExpiresAtUtc <= DateTime.UtcNow)
            {
                RemoveRequest(request.RequestId);

                ApplicationLogger.LogWarning("The verification request was rejected because it has expired.");

                throw CreateVerificationFault(ErrorCodes.RequestExpired);
            }
        }

        private void ValidateCode(VerificationRequest request, string code)
        {
            bool codeMatches = false;

            if (code != null)
            {
                codeMatches = _hashService.VerifyHash(code, request.CodeHash);
            }

            if (!codeMatches)
            {
                RejectCode(request);
            }
        }

        private void RejectCode(VerificationRequest request)
        {
            request.RemainingAttempts--;

            string errorCode = ErrorCodes.InvalidCode;

            if (request.RemainingAttempts <= 0)
            {
                RemoveRequest(request.RequestId);

                errorCode = ErrorCodes.AttemptsExhausted;
            }

            ApplicationLogger.LogWarning("The verification code was rejected. Remaining attempts: {0}",
                request.RemainingAttempts);

            throw CreateVerificationFault(errorCode);
        }

        private bool RemoveRequest(string requestId)
        {
            IDictionary<string, VerificationRequest> requests = PendingRequests;

            bool requestRemoved = requests.Remove(requestId);

            return requestRemoved;
        }

        public VerificationRequest CreateRequest(int accountId, string code)
        {
            if (accountId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(accountId));
            }

            VerificationRequest request = BuildRequest(accountId, code);

            SaveRequest(request);

            return request;
        }

        private void SaveRequest(VerificationRequest request)
        {
            bool requestStored = PendingRequests.TryAdd(request.RequestId, request);

            while (!requestStored)
            {
                request.RequestId = Guid.NewGuid().ToString("N");

                requestStored = PendingRequests.TryAdd(request.RequestId, request);
            }
        }

        private VerificationRequest BuildRequest(int accountId, string code)
        {
            string requestId = Guid.NewGuid().ToString("N");
            byte[] codeHash = _hashService.ComputeHash(code);
            DateTime expiresAtUtc = DateTime.UtcNow.Add(RequestLifetime);

            var request = new VerificationRequest
            {
                RequestId = requestId,
                AccountId = accountId,
                CodeHash = codeHash,
                ExpiresAtUtc = expiresAtUtc,
                RemainingAttempts = MaximumAttempts
            };

            return request;
        }

        private VerificationRequest GetSavedRequest(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                ApplicationLogger.LogWarning(
                    "The verification request was rejected because its identifier is missing.");

                throw CreateVerificationFault(ErrorCodes.RequestNotFound);
            }

            try
            {
                VerificationRequest request = PendingRequests[requestId];

                if (request == null)
                {
                    ApplicationLogger.LogWarning("The verification request is unavailable.");

                    throw CreateVerificationFault(ErrorCodes.RequestNotFound);
                }

                return request;
            }
            catch (KeyNotFoundException verificationException)
            {
                ApplicationLogger.LogWarning("The verification request was not found. Reason: {0}",
                    verificationException.Message);

                throw CreateVerificationFault(ErrorCodes.RequestNotFound);
            }
        }

        private FaultException<VerificationFault> CreateVerificationFault(string errorCode)
        {
            var verificationFault = new VerificationFault
            {
                ErrorCode = errorCode
            };

            var faultException = new FaultException<VerificationFault>(verificationFault);

            return faultException;
        }
    }
}
