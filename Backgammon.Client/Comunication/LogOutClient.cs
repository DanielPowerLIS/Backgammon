using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.LogOutReference;

namespace Backgammon.Client.Comunication
{
    internal class LogOutClient
    {
        private static readonly string EndpointConfigurationName =
            "NetTcpBinding_ILogOutService";

        public async Task LogOutAsync(string sessionId)
        {
            var serviceClient = new LogOutServiceClient(EndpointConfigurationName);

            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                await serviceClient.LogOutAsync(sessionId);

                await ServiceChannelCloser.CloseAfterResponseAsync(communicationChannel);
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