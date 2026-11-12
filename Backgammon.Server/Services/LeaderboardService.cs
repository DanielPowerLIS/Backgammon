using System.Collections.Generic;
using System.Linq;
using System;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.ServiceModel;

using Backgammon.Server.Persistence.Data;
using Backgammon.Server.Persistence.Repositories.InterfacesRepositories;
using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.Contracts;
using Backgammon.Server.SupportServices.Logging;
using Backgammon.Server.Services.ExceptionCode;

namespace Backgammon.Server.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IProfileRepository _profileRepository;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(LeaderboardService));

        public LeaderboardService()
        {
            _profileRepository = new ProfileRepository();
        }

        public PlayerStanding[] GetLeaderboard()
        {
            List<Profile> playerProfiles = GetProfiles();

            PlayerStanding[] leaderboardEntries = playerProfiles
                .OrderByDescending(playerProfile => playerProfile.Wins)
                .Select(CreatePlayerStanding)
                .ToArray();

            return leaderboardEntries;
        }

        private List<Profile> GetProfiles()
        {
            try
            {
                return _profileRepository.GetAllProfilesAsync().GetAwaiter().GetResult();
            }
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented the leaderboard from loading.");

                throw CreateLeaderboardFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException,
                    "A SQL Server failure prevented the leaderboard from loading.");

                throw CreateLeaderboardFault(ErrorCodes.ServiceUnavailable);
            }
        }

        private FaultException<LeaderboardFault> CreateLeaderboardFault(string errorCode)
        {
            var leaderboardFault = new LeaderboardFault
            {
                ErrorCode = errorCode
            };

            var faultException = new FaultException<LeaderboardFault>(leaderboardFault);

            return faultException;
        }

        private PlayerStanding CreatePlayerStanding(Profile playerProfile)
        {
            var leaderboardEntry = new PlayerStanding
            {
                Username = playerProfile.Username,
                Wins = playerProfile.Wins,
                Points = playerProfile.Points
            };

            return leaderboardEntry;
        }
    }
}
