using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backgammon.Client.Data;
using Backgammon.Client.Repositories;


namespace Backgammon.Client.Services
{
    public class LeaderboardService
    {
        private readonly ProfileRepository _profileRepository;

        public LeaderboardService()
        {
            _profileRepository = new ProfileRepository();
        }

        public List<Profile> GetLeaderboard()
        { 
            List<Profile> profiles = _profileRepository.GetAllProfiles();

            return profiles.OrderByDescending(profile => profile.Wins).ToList();
        }
    }
}
