using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Persistence.Repositories.InterfacesRepositories;
using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Persistence.Data;
using Backgammon.Server.Services.Manager;
using Backgammon.Server.SupportServices.ExceptionCode;
using Backgammon.Server.SupportServices.Logging;

using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.ServiceModel;
using Backgammon.Server.Services.Data;
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
    }
}
