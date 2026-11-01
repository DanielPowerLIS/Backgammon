using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.Settings.Logging;
using Backgammon.Client.Comunication;
using Backgammon.Client.ProfileReference;

namespace Backgammon.Client.Controllers.Profile
{
    internal class ProfileController
    {
        private readonly ProfileClient _profileClient;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(ProfileController));

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
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The player profile request failed.");

                return null;
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The player profile request exceeded the time limit.");

                return null;
            }
        }

        public async Task<Avatar[]> GetAvailableAvatarsAsync(string sessionId)
        {
            try
            {
                return await _profileClient.GetAvailableAvatarsAsync(sessionId);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The available avatars request failed.");

                return null;
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The available avatars request exceeded the time limit.");

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
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The avatar change request failed.");

                return false;
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The avatar change request exceeded the time limit.");

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
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The profile description update request failed.");

                return false;
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The profile description update request exceeded the time limit.");

                return false;
            }
        }

        public async Task<SocialMediaType[]> GetSocialMediaTypesAsync(string sessionId)
        {
            try
            {
                return await _profileClient.GetSocialMediaTypesAsync(sessionId);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The social media types request failed.");

                return null;
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The social media types request exceeded the time limit.");

                return null;
            }
        }

        public async Task<SocialMediaLink> AddSocialMediaLinkAsync(string sessionId, int socialMediaTypeId, string url)
        {
            try
            {
                return await _profileClient.AddSocialMediaLinkAsync(sessionId, socialMediaTypeId, url);
            }
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The request to add a social media link failed.");

                return null;
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The request to add a social media link exceeded the time limit.");

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
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "The request to remove a social media link failed.");

                return false;
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "The request to remove a social media link exceeded the time limit.");

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