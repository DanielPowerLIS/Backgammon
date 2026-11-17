using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.FriendReference;

namespace Backgammon.Client.Comunication
{
    internal class FriendClient
    {
        private static readonly string EndpointConfigurationName = "NetTcpBinding_IFriendService";

        public async Task<Friend[]> GetFriendsAsync(string sessionId)
        {
            var serviceClient = new FriendServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                Friend[] friends = await serviceClient.GetFriendsAsync(sessionId);

                await ServiceChannelCloser.CloseAfterResponseAsync(communicationChannel);

                return friends;
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