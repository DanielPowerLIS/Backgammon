using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

using Backgammon.Server.Persistence.Data;

namespace Backgammon.Server.Persistence.Repositories
{
    public class ProfileRepository
    {
        public async Task<List<Profile>> GetAllProfilesAsync()
        {
            using (var databaseContext = new BackgammonEntities())
            {
                return await databaseContext.Profiles
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
        }
    }
}