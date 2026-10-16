using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Server.Services.Responses;

namespace Backgammon.Server.Services
{
    [ServiceContract(Name = "LeaderboardService")]
    public interface ILeaderboardService
    {
        [OperationContract]
        Task<LeaderboardEntryDto[]> GetLeaderboardAsync();
    }
}
