using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services
{
    [ServiceContract]
    public interface ILeaderboardService
    {
        [OperationContract]
        Task<PlayerStanding[]> GetLeaderboardAsync();
    }

    [ServiceContract]
    public interface IAuthenticationService
    {
        [OperationContract]
        [FaultContract(typeof(AuthenticationFault))]
        Task<PlayerSession> SignInAsync(Credentials playerCredentials);
    }

    [ServiceContract]
    public interface ILogOutService
    {
        [OperationContract]
        void LogOut(string sessionId);
    }
}
