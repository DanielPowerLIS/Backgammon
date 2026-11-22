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

        public async Task<FriendRequestPending[]> GetFriendRequestsAsync(string sessionId)
        {
            var serviceClient = new FriendServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                FriendRequestPending[] friendRequests = await serviceClient.GetFriendRequestsAsync(sessionId);

                await ServiceChannelCloser.CloseAfterResponseAsync(communicationChannel);

                return friendRequests;
            }
            finally
            {
                if (communicationChannel.State != CommunicationState.Closed)
                {
                    communicationChannel.Abort();
                }
            }
        }

        public async Task AcceptFriendRequestAsync(string sessionId, int requestId)
        {
            var serviceClient = new FriendServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                await serviceClient.AcceptFriendRequestAsync(sessionId, requestId);
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

        public async Task RejectFriendRequestAsync(string sessionId, int requestId)
        {
            var serviceClient = new FriendServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                await serviceClient.RejectFriendRequestAsync(sessionId, requestId);
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

        public async Task SendFriendRequestAsync(string sessionId, string identifier, bool byEmail)
        {
            var serviceClient = new FriendServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                await serviceClient.SendFriendRequestAsync(sessionId, identifier, byEmail);
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