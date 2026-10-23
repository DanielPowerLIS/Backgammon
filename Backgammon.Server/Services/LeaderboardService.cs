using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.ServiceModel;

using Backgammon.Server.Persistence.Data;
using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.Contracts;
using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ProfileRepository _profileRepository;

        private static readonly string ServiceUnavailableErrorCode =
            "ServiceUnavailable";

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(LeaderboardService));

        public LeaderboardService()
        {
            _profileRepository = new ProfileRepository();
        }

        public async Task<PlayerStanding[]> GetLeaderboardAsync()
        {
            List<Profile> playerProfiles = await GetProfilesAsync();

            PlayerStanding[] leaderboardEntries = playerProfiles
                .OrderByDescending(playerProfile => playerProfile.Wins)
                .Select(CreatePlayerStanding)
                .ToArray();

            return leaderboardEntries;
        }

        private async Task<List<Profile>> GetProfilesAsync()
        {
            try
            {
                return await _profileRepository.GetAllProfilesAsync();
            }
            catch (Exception databaseException) when (
                databaseException is EntityException ||
                databaseException is SqlException)
            {
                ApplicationLogger.LogError(
                    databaseException,
                    "Database query failed while loading the leaderboard.");

                var leaderboardFault = new LeaderboardFault
                {
                    ErrorCode = ServiceUnavailableErrorCode
                };

                throw new FaultException<LeaderboardFault>(leaderboardFault);
            }
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
