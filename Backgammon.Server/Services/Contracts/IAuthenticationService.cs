using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface IAuthenticationService
    {
        [OperationContract]
        [FaultContract(typeof(AuthenticationFault))]
        Task<PlayerSession> SignInAsync(Credentials playerCredentials);
    }
}
