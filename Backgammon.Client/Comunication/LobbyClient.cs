using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.LobbyReference;

namespace Backgammon.Client.Comunication
{
    internal class LobbyClient
    {
        private static readonly string EndpointConfigurationName =
            "NetTcpBinding_ILobbyService";

        public async Task LeaveLobbyAsync(string sessionId, string roomId)
        {
            var serviceClient =
                new LobbyServiceClient(EndpointConfigurationName);

            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                await serviceClient.LeaveLobbyAsync(sessionId, roomId);

                await ServiceChannelCloser.CloseAfterResponseAsync(
                    communicationChannel);
            }
            finally
            {
                if (communicationChannel.State != CommunicationState.Closed)
                {
                    communicationChannel.Abort();
                }
            }
        }
    }
}