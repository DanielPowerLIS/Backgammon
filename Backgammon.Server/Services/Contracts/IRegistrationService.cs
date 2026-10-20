using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface IRegistrationService
    {
        [OperationContract]
        [FaultContract(typeof(RegistrationFault))]
        Task RegisterAccountAsync(Account accountInformation);
    }
}
