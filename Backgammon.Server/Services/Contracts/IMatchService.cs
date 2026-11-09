

using System.ServiceModel;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface IMatchService
    {
        [OperationContract]
        void InitializeMatch(string sessionId, string roomId);



    }
}
