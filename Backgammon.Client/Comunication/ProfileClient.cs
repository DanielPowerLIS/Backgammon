using System.ServiceModel;
using System.Threading.Tasks;
using Backgammon.Client.ProfileReference;

namespace Backgammon.Client.Comunication
{
    internal class ProfileClient
    {
        private static readonly string EndpointConfigurationName =
            "NetTcpBinding_IProfileService";

        public async Task<PlayerProfile> GetPlayerProfileAsync(string sessionId)
        {
            var serviceClient = new ProfileServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                PlayerProfile playerProfile = await serviceClient.GetPlayerProfileAsync(sessionId);
                await ServiceChannelCloser.CloseAfterResponseAsync(communicationChannel);

                return playerProfile;
            }
            finally
            {
                if (communicationChannel.State != CommunicationState.Closed)
                {
                    communicationChannel.Abort();
                }
            }
        }

        public async Task<Avatar[]> GetAvailableAvatarsAsync(string sessionId)
        {
            var serviceClient = new ProfileServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                var avatars = await serviceClient.GetAvailableAvatarsAsync(sessionId);
                await ServiceChannelCloser.CloseAfterResponseAsync(communicationChannel);

                return avatars;
            }
            finally
            {
                if (communicationChannel.State != CommunicationState.Closed)
                {
                    communicationChannel.Abort();
                }
            }
        }

        public async Task ChangeAvatarAsync(string sessionId, int idAvatar)
        {
            var serviceClient = new ProfileServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                await serviceClient.ChangeAvatarAsync(sessionId, idAvatar);
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

        public async Task UpdateDescriptionAsync(string sessionId, string description)
        {
            var serviceClient = new ProfileServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                await serviceClient.UpdateDescriptionAsync(sessionId, description);
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

        public async Task<SocialMediaType[]> GetSocialMediaTypesAsync(string sessionId)
        {
            var serviceClient = new ProfileServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                var types = await serviceClient.GetSocialMediaTypesAsync(sessionId);
                await ServiceChannelCloser.CloseAfterResponseAsync(communicationChannel);

                return types;
            }
            finally
            {
                if (communicationChannel.State != CommunicationState.Closed)
                {
                    communicationChannel.Abort();
                }
            }
        }

        public async Task<SocialMediaLink> AddSocialMediaLinkAsync(string sessionId, int socialMediaTypeId, string url)
        {
            var serviceClient = new ProfileServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                var addedLink = await serviceClient.AddSocialMediaLinkAsync(sessionId, socialMediaTypeId, url);
                await ServiceChannelCloser.CloseAfterResponseAsync(communicationChannel);

                return addedLink;
            }
            finally
            {
                if (communicationChannel.State != CommunicationState.Closed)
                {
                    communicationChannel.Abort();
                }
            }
        }

        public async Task RemoveSocialMediaLinkAsync(string sessionId, int socialMediaLinkId)
        {
            var serviceClient = new ProfileServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                await serviceClient.RemoveSocialMediaLinkAsync(sessionId, socialMediaLinkId);
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

        public async Task<SocialMediaLink> UpdateSocialMediaLinkAsync(string sessionId, int socialMediaLinkId, string url)
        {
            var serviceClient = new ProfileServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                var updatedLink = await serviceClient.UpdateSocialMediaLinkAsync(sessionId, socialMediaLinkId, url);
                await ServiceChannelCloser.CloseAfterResponseAsync(communicationChannel);

                return updatedLink;
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