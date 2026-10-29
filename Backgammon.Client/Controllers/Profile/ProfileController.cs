using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Backgammon.Client.Comunication;
using Backgammon.Client.ProfileReference;

namespace Backgammon.Client.Controllers.Profile
{
    internal class ProfileController
    {
        private readonly ProfileClient _profileClient;

        public ProfileController()
        {
            _profileClient = new ProfileClient();
        }

        public async Task<PlayerProfile> GetPlayerProfileAsync(string sessionId)
        {
            try
            {
                return await _profileClient.GetPlayerProfileAsync(sessionId);
            }
            catch (CommunicationException)
            {
                return null;
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        public async Task<Avatar[]> GetAvailableAvatarsAsync(string sessionId)
        {
            try
            {
                return await _profileClient.GetAvailableAvatarsAsync(sessionId);
            }
            catch (CommunicationException)
            {
                return null;
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        public async Task<bool> ChangeAvatarAsync(string sessionId, int idAvatar)
        {
            try
            {
                await _profileClient.ChangeAvatarAsync(sessionId, idAvatar);
                return true;
            }
            catch (CommunicationException)
            {
                return false;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        public async Task<bool> UpdateDescriptionAsync(string sessionId, string description)
        {
            try
            {
                await _profileClient.UpdateDescriptionAsync(sessionId, description);
                return true;
            }
            catch (CommunicationException)
            {
                return false;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        public async Task<SocialMediaType[]> GetSocialMediaTypesAsync(string sessionId)
        {
            try
            {
                return await _profileClient.GetSocialMediaTypesAsync(sessionId);
            }
            catch (CommunicationException)
            {
                return null;
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        public async Task<SocialMediaLink> AddSocialMediaLinkAsync(string sessionId, int socialMediaTypeId, string url)
        {
            try
            {
                return await _profileClient.AddSocialMediaLinkAsync(sessionId, socialMediaTypeId, url);
            }
            catch (CommunicationException)
            {
                return null;
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        public async Task<bool> RemoveSocialMediaLinkAsync(string sessionId, int socialMediaLinkId)
        {
            try
            {
                await _profileClient.RemoveSocialMediaLinkAsync(sessionId, socialMediaLinkId);
                return true;
            }
            catch (CommunicationException)
            {
                return false;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        public async Task<SocialMediaLink> UpdateSocialMediaLinkAsync(
                string sessionId,
                int socialMediaLinkId,
                string url)
        {
            return await _profileClient.UpdateSocialMediaLinkAsync(sessionId, socialMediaLinkId, url);
        }



    }
}