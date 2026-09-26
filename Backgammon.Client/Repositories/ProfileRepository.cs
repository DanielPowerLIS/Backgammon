using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backgammon.Client.Data;


namespace Backgammon.Client.Repositories
{
    public class ProfileRepository
    {
        private readonly BackgammonEntities _databaseContext;

        public ProfileRepository()
        {
            _databaseContext = new BackgammonEntities();
        }

        public List<Profile> GetAllProfiles()
        {
            return _databaseContext.Profiles.ToList();
        }

    }
}
