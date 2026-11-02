using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    public interface IChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(ChatMessage message);

        [OperationContract(IsOneWay = true)]
        void OnLobbyChatClosed();
    }
}
