using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.GuestAccessReference;

namespace Backgammon.Client.Comunication
{
    internal class GuestAccessClient
    {
        private static readonly string EndpointConfigurationName =
            "NetTcpBinding_IGuestAccessService";

        public async Task<PlayerSession> AccessAsGuestAsync()
        {
            var serviceClient =
                new GuestAccessServiceClient(EndpointConfigurationName);

            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                PlayerSession guestSession =
                    await serviceClient.AccessAsGuestAsync();

                await ServiceChannelCloser.CloseAfterResponseAsync(
                    communicationChannel);

                return guestSession;
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