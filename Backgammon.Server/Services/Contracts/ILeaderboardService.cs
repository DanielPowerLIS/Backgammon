using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface ILeaderboardService
    {
        [OperationContract]
        Task<PlayerStanding[]> GetLeaderboardAsync();
    }

}
