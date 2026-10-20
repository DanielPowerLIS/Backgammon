using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.AuthenticationReference;

namespace Backgammon.Client.Comunication
{
    internal class AuthenticationClient
    {
        private static readonly string EndpointConfigurationName =
            "NetTcpBinding_IAuthenticationService";

        public async Task<PlayerSession> SignInAsync(Credentials playerCredentials)
        {
            var serviceClient = new AuthenticationServiceClient(EndpointConfigurationName);

            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                PlayerSession playerSession =
                    await serviceClient.SignInAsync(playerCredentials);

                await ServiceChannelCloser.CloseAfterResponseAsync(communicationChannel);

                return playerSession;
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