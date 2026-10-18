using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Server.Services.Responses;

namespace Backgammon.Server.Services
{
    [ServiceContract]
    public interface ILeaderboardService
    {
        [OperationContract]
        Task<PlayerStanding[]> GetLeaderboardAsync();
    }
}
