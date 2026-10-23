using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface IRegistrationService
    {
        [OperationContract]
        [FaultContract(typeof(RegistrationFault))]
        void RegisterAccount(Account accountInformation);
    }
}
