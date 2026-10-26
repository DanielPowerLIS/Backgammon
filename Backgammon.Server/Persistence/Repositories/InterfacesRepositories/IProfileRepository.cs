using System.Collections.Generic;
using System.Threading.Tasks;

using Backgammon.Server.Persistence.Data;

namespace Backgammon.Server.Persistence.Repositories.InterfacesRepositories
{
    public interface IProfileRepository
    {
        Task<List<Profile>> GetAllProfilesAsync();

        Task<Profile> GetFullProfileAsync(int profileId);

        Task<List<Avatar>> GetAvatarsAsync();

        Task ChangeAvatarAsync(int profileId, int idAvatar);

        Task UpdateDescriptionAsync(int profileId, string description);

        Task<List<TypeSocialMedia>> GetSocialMediaTypesAsync();

        Task<SocialMedia> AddSocialMediaLinkAsync(
            int profileId, int typeId, string url);

        Task RemoveSocialMediaLinkAsync(int profileId, int linkId);

        Task<SocialMedia> UpdateSocialMediaLinkAsync(
            int profileId, int linkId, string url);
    }
}