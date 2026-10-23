using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface IAuthenticationService
    {
        [OperationContract]
        [FaultContract(typeof(AuthenticationFault))]
        PlayerSession SignIn(Credentials playerCredentials);
    }
}
