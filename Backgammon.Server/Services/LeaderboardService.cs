using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Backgammon.Server.Persistence.Data;
using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Services.Responses;

namespace Backgammon.Server.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ProfileRepository _profileRepository;

        public LeaderboardService()
        {
            _profileRepository = new ProfileRepository();
        }

        public async Task<LeaderboardEntry[]> GetLeaderboardAsync()
        {
            List<Profile> playerProfiles =
                await _profileRepository.GetAllProfilesAsync();

            return playerProfiles
                .OrderByDescending(playerProfile => playerProfile.Wins)
                .Select(CreateLeaderboardEntry)
                .ToArray();
        }

        private LeaderboardEntry CreateLeaderboardEntry(Profile playerProfile)
        {
            return new LeaderboardEntry
            {
                Username = playerProfile.Username,
                Wins = playerProfile.Wins,
                Points = playerProfile.Points
            };
        }
    }
}