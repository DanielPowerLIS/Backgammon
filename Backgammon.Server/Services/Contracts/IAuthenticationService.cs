using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface IAuthenticationService
    {
        [OperationContract]
        [FaultContract(typeof(AuthenticationFault))]
        VerificationRequest SignIn(Credentials playerCredentials);

        [OperationContract]
        [FaultContract(typeof(VerificationFault))]
        [FaultContract(typeof(AuthenticationFault))]
        PlayerSession ConfirmVerification(string requestId, string code);
    }
}
