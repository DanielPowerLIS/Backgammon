using Backgammon.Server.Persistence.Data;
using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Persistence.Repositories.InterfacesRepositories;
using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.Manager;
using Backgammon.Server.SupportServices.ExceptionCode;
using Backgammon.Server.SupportServices.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.ServiceModel;
using AccountDB = Backgammon.Server.Persistence.Data.Account;

namespace Backgammon.Server.Services
{
    public class FriendService : IFriendService
    {
        private readonly IFriendRepository _friendRepository;
        private readonly PlayerSessionManager _playerSessionManager;
        private static readonly IApplicationLogger ApplicationLogger =
         new Log4NetApplicationLogger(typeof(FriendService));

        public FriendService()
        {
            _friendRepository = new FriendRepository();
            _playerSessionManager = new PlayerSessionManager();
        }
        public List<Friend> GetFriends(string sessionId)
        {
            int profileId;

            try
            {
                profileId = _playerSessionManager.GetProfileId(sessionId);  
            }
            catch (FaultException<PlayerProfileFault> sessionFault)
            {
                ApplicationLogger.LogWarning("Friend retrieval was rejected. Code: {0}", sessionFault.Detail.ErrorCode);

                FaultException<FriendFault> friendFault = CreateFriendFault(ErrorCodes.SessionExpired);

                throw friendFault;
            }

            List<AccountDB> friendAccounts = FetchFriends(profileId);
            List<Friend> friends = MapFriends(friendAccounts);

            return friends;
        }

        public List<FriendRequestPending> GetFriendRequests(string sessionId)
        {
            int profileId;

            try
            {
                profileId = _playerSessionManager.GetProfileId(sessionId);
            }
            catch (FaultException<PlayerProfileFault> sessionFault)
            {
                ApplicationLogger.LogWarning("Friend request retrieval was rejected. Code: {0}", sessionFault.Detail.ErrorCode);

                FaultException<FriendFault> friendFault = CreateFriendFault(ErrorCodes.SessionExpired);

                throw friendFault;
            }

            List<FriendRequest> requests = FetchFriendRequests(profileId);
            List<FriendRequestPending> pendingRequests = MapFriendRequests(requests);

            return pendingRequests;
        }

        public void AcceptFriendRequest(string sessionId, int requestId)
        {
            int profileId = GetFriendProfileId(sessionId);

            ProcessRequest(profileId, requestId, true);
        }

        public void RejectFriendRequest(string sessionId, int requestId)
        {
            int profileId = GetFriendProfileId(sessionId);

            ProcessRequest(profileId, requestId, false);
        }

        public void SendFriendRequest(string sessionId, string identifier, bool byEmail)
        {
            int profileId = GetFriendProfileId(sessionId);

            try
            {
                _friendRepository.SendFriendRequestAsync(profileId, identifier, byEmail)
                    .GetAwaiter().GetResult();
            }
            catch (KeyNotFoundException accountException)
            {
                ApplicationLogger.LogWarning(
                    "Friend request sending was rejected. Code: {0}. Reason: {1}",
                    ErrorCodes.InvalidRequest, accountException.Message);

                throw CreateFriendFault(ErrorCodes.InvalidRequest);
            }
            catch (ArgumentException requestException)
            {
                ApplicationLogger.LogWarning(
                    "Friend request sending was rejected. Code: {0}. Reason: {1}",
                    ErrorCodes.InvalidRequest, requestException.Message);

                throw CreateFriendFault(ErrorCodes.InvalidRequest);
            }
            catch (DbUpdateException updateException)
            {
                ApplicationLogger.LogError(updateException, "The friend request could not be saved.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbEntityValidationException validationException)
            {
                ApplicationLogger.LogError(validationException, "Friend request entity validation failed.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException, "An Entity Framework failure prevented the friend request.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException, "A SQL failure prevented the friend request.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
        }

        public void RemoveFriend(string sessionId, int friendAccountId)
        {
            int profileId = GetFriendProfileId(sessionId);

            try
            {
                _friendRepository.RemoveFriendAsync(profileId, friendAccountId)
                    .GetAwaiter().GetResult();
            }
            catch (ArgumentException requestException)
            {
                ApplicationLogger.LogWarning(
                    "Friend removal was rejected. Code: {0}. Reason: {1}",
                    ErrorCodes.InvalidRequest, requestException.Message);

                throw CreateFriendFault(ErrorCodes.InvalidRequest);
            }
            catch (KeyNotFoundException friendshipException)
            {
                ApplicationLogger.LogWarning(
                    "Friend removal was rejected. Code: {0}. Reason: {1}",
                    ErrorCodes.FriendshipNotFound, friendshipException.Message);

                throw CreateFriendFault(ErrorCodes.FriendshipNotFound);
            }
            catch (DbUpdateException updateException)
            {
                ApplicationLogger.LogError(updateException, "The friendship removal could not be saved.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbEntityValidationException validationException)
            {
                ApplicationLogger.LogError(validationException, "Friendship entity validation failed.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented friendship removal.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException, "A SQL failure prevented friendship removal.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
        }

        private int GetFriendProfileId(string sessionId)
        {
            try
            {
                int profileId = _playerSessionManager.GetProfileId(sessionId);

                return profileId;
            }
            catch (FaultException<PlayerProfileFault> sessionFault)
            {
                ApplicationLogger.LogWarning(
                    "Friend request operation was rejected. Code: {0}",
                    sessionFault.Detail.ErrorCode);

                throw CreateFriendFault(ErrorCodes.SessionExpired);
            }
        }

        private void ProcessRequest(int profileId, int requestId, bool accept)
        {
            try
            {
                if (accept)
                {
                    _friendRepository.AcceptFriendRequestAsync(profileId, requestId)
                        .GetAwaiter().GetResult();
                }
                else
                {
                    _friendRepository.RejectFriendRequestAsync(profileId, requestId)
                        .GetAwaiter().GetResult();
                }
            }
            catch (KeyNotFoundException requestException)
            {
                ApplicationLogger.LogWarning(
                    "Friend request operation was rejected. Code: {0}. Reason: {1}",
                    ErrorCodes.RequestNotFound, requestException.Message);

                throw CreateFriendFault(ErrorCodes.RequestNotFound);
            }
            catch (ArgumentException requestException)
            {
                ApplicationLogger.LogWarning(
                    "Friend request operation was rejected. Code: {0}. Reason: {1}",
                    ErrorCodes.InvalidRequest, requestException.Message);

                throw CreateFriendFault(ErrorCodes.InvalidRequest);
            }
            catch (DbUpdateException updateException)
            {
                ApplicationLogger.LogError(updateException, "The friend request change could not be saved.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbEntityValidationException validationException)
            {
                ApplicationLogger.LogError(validationException, "Friend request entity validation failed.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException, "An Entity Framework failure affected the friend request.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException, "A SQL failure affected the friend request.");

                throw CreateFriendFault(ErrorCodes.ServiceUnavailable);
            }
        }

        private List<AccountDB> FetchFriends(int profileId)
        {
            try
            {
                List<AccountDB> friendAccounts = _friendRepository.GetFriendsAsync(profileId).GetAwaiter()
                    .GetResult();

                return friendAccounts;
            }
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(
                    entityException,
                    "An Entity Framework failure prevented friends from loading.");

                FaultException<FriendFault> friendFault =
                    CreateFriendFault(ErrorCodes.ServiceUnavailable);

                throw friendFault;
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(
                    databaseException,
                    "A SQL Server failure prevented friends from loading.");

                FaultException<FriendFault> friendFault =
                    CreateFriendFault(ErrorCodes.ServiceUnavailable);

                throw friendFault;
            }
        }

        private List<Friend> MapFriends(List<AccountDB> friendAccounts)
        {
            List<Friend> friends = new List<Friend>();

            foreach (AccountDB friendAccount in friendAccounts)
            {
                if (friendAccount.Profile != null)
                {
                    Friend friend = new Friend
                    {
                        AccountId = friendAccount.IdAccountPK,
                        Username = friendAccount.Profile.Username
                    };

                    friends.Add(friend);
                }
            }

            return friends;
        }

        private FaultException<FriendFault> CreateFriendFault(string errorCode)
        {
            var friendFaultDetail = new FriendFault
            {
                ErrorCode = errorCode
            };

            var friendFault = new FaultException<FriendFault>(friendFaultDetail);

            return friendFault;
        }

        private List<FriendRequest> FetchFriendRequests(int profileId)
        {
            try
            {
                List<FriendRequest> requests = _friendRepository.GetIncomingRequestsAsync(profileId).GetAwaiter().GetResult();

                return requests;
            }
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException, "An Entity Framework failure prevented friend requests from loading.");

                FaultException<FriendFault> friendFault = CreateFriendFault(ErrorCodes.ServiceUnavailable);

                throw friendFault;
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException, "A SQL Server failure prevented friend requests from loading.");

                FaultException<FriendFault> friendFault = CreateFriendFault(ErrorCodes.ServiceUnavailable);

                throw friendFault;
            }
        }

        private List<FriendRequestPending> MapFriendRequests(List<FriendRequest> requests)
        {
            List<FriendRequestPending> pendingRequests = new List<FriendRequestPending>();

            foreach (FriendRequest request in requests)
            {
                if (request.Account != null && request.Account.Profile != null)
                {
                    FriendRequestPending pendingRequest = new FriendRequestPending
                    {
                        RequestId = request.IdFriendRequestPK,
                        SenderUsername = request.Account.Profile.Username
                    };

                    pendingRequests.Add(pendingRequest);
                }
            }

            return pendingRequests;
        }
    }
}
