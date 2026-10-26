using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface IGuestAccessService
    {
        [OperationContract]
        [FaultContract(typeof(GuestAccessFault))]
        PlayerSession AccessAsGuest();
    }
}
