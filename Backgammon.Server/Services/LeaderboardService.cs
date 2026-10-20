using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Backgammon.Server.Persistence.Data;
using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.Contracts;

namespace Backgammon.Server.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ProfileRepository _profileRepository;

        public LeaderboardService()
        {
            _profileRepository = new ProfileRepository();
        }

        public async Task<PlayerStanding[]> GetLeaderboardAsync()
        {
            List<Profile> playerProfiles =
                await _profileRepository.GetAllProfilesAsync();

            PlayerStanding[] leaderboardEntries = playerProfiles
                .OrderByDescending(playerProfile => playerProfile.Wins)
                .Select(CreatePlayerStanding)
                .ToArray();

            return leaderboardEntries;
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
