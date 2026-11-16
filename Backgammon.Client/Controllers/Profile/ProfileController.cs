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
            PlayerProfile playerProfile =
                await _profileClient.GetPlayerProfileAsync(sessionId);

            return playerProfile;
        }

        public async Task<Avatar[]> GetAvailableAvatarsAsync(string sessionId)
        {
            Avatar[] avatars = await _profileClient.GetAvailableAvatarsAsync(sessionId);

            return avatars;
        }

        public async Task<bool> ChangeAvatarAsync(string sessionId, int idAvatar)
        {
            await _profileClient.ChangeAvatarAsync(sessionId, idAvatar);

            bool changeCompleted = true;

            return changeCompleted;
        }

        public async Task<bool> UpdateDescriptionAsync(string sessionId, string description)
        {
            await _profileClient.UpdateDescriptionAsync(sessionId, description);

            bool updateCompleted = true;

            return updateCompleted;
        }

        public async Task<SocialMediaType[]> GetSocialMediaTypesAsync(string sessionId)
        {
            SocialMediaType[] socialMediaTypes =
                await _profileClient.GetSocialMediaTypesAsync(sessionId);

            return socialMediaTypes;
        }

        public async Task<SocialMediaLink> AddSocialMediaLinkAsync(
            string sessionId,
            int socialMediaTypeId,
            string url)
        {
            SocialMediaLink socialMediaLink =
                await _profileClient.AddSocialMediaLinkAsync(
                    sessionId,
                    socialMediaTypeId,
                    url);

            return socialMediaLink;
        }

        public async Task<bool> RemoveSocialMediaLinkAsync(
            string sessionId,
            int socialMediaLinkId)
        {
            await _profileClient.RemoveSocialMediaLinkAsync(
                sessionId,
                socialMediaLinkId);

            bool removalCompleted = true;

            return removalCompleted;
        }

        public async Task<SocialMediaLink> UpdateSocialMediaLinkAsync(
            string sessionId,
            int socialMediaLinkId,
            string url)
        {
            SocialMediaLink socialMediaLink =
                await _profileClient.UpdateSocialMediaLinkAsync(
                    sessionId,
                    socialMediaLinkId,
                    url);

            return socialMediaLink;
        }
    }
}