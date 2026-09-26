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
        private readonly BackgammonEntities _dataBaseContext;

        public ProfileRepository()
        {
            _dataBaseContext = new BackgammonEntities();
        }

        public List<Profile> GetAllProfiles()
        {
            return _dataBaseContext.Profiles.ToList();
        }

    }
}
