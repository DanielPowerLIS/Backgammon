using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Backgammon.Server.Persistence.Data;

namespace Backgammon.Server.Persistence.Repositories
{
    public class ProfileRepository
    {
        public async Task<List<Profile>> GetAllProfilesAsync()
        {
            using (var databaseContext = new BackgammonEntities())
            {
                List<Profile> allProfiles = await databaseContext.Profiles
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

                return allProfiles;
            }
        }

        public async Task<Profile> GetFullProfileAsync(int profileId)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                Profile targetProfile = await databaseContext.Profiles
                    .Include(profile => profile.Avatar)
                    .Include(profile => profile.Accounts)
                    .Include(profile => profile.SocialMedias)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(profile => profile.IdProfilePK == profileId)
                    .ConfigureAwait(false);

                return targetProfile;
            }
        }

        public async Task<List<Avatar>> GetAvatarsAsync()
        {
            using (var databaseContext = new BackgammonEntities())
            {
                List<Avatar> availableAvatars = await databaseContext.Avatars
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

                return availableAvatars;
            }
        }

        public async Task ChangeAvatarAsync(int profileId, int idAvatar)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                Profile targetProfile = await databaseContext.Profiles
                    .FirstOrDefaultAsync(profile => profile.IdProfilePK == profileId)
                    .ConfigureAwait(false);

                if (targetProfile != null)
                {
                    targetProfile.IdAvatarFK = idAvatar;
                    await databaseContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task UpdateDescriptionAsync(int profileId, string description)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                Profile targetProfile = await databaseContext.Profiles
                    .FirstOrDefaultAsync(profile => profile.IdProfilePK == profileId)
                    .ConfigureAwait(false);

                if (targetProfile != null)
                {
                    targetProfile.Description = description;
                    await databaseContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<List<TypeSocialMedia>> GetSocialMediaTypesAsync()
        {
            using (var databaseContext = new BackgammonEntities())
            {
                List<TypeSocialMedia> socialMediaTypes = await databaseContext.TypeSocialMedias
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

                return socialMediaTypes;
            }
        }

        public async Task<SocialMedia> AddSocialMediaLinkAsync(int profileId, int typeId, string url)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                var newSocialMedia = new SocialMedia
                {
                    IdProfileFK = profileId,
                    IdTypeSocialMediaFK = typeId,
                    Link = url
                };

                databaseContext.SocialMedias.Add(newSocialMedia);
                await databaseContext.SaveChangesAsync().ConfigureAwait(false);

                return newSocialMedia;
            }
        }

        public async Task RemoveSocialMediaLinkAsync(int profileId, int linkId)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                SocialMedia targetLink = await databaseContext.SocialMedias
                    .FirstOrDefaultAsync(link => link.IdSocialMediaPK == linkId && link.IdProfileFK == profileId)
                    .ConfigureAwait(false);

                if (targetLink != null)
                {
                    databaseContext.SocialMedias.Remove(targetLink);
                    await databaseContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<SocialMedia> UpdateSocialMediaLinkAsync(int profileId, int linkId, string url)
        {
            using (var databaseContext = new BackgammonEntities())
            {
                SocialMedia targetLink = await databaseContext.SocialMedias
                    .FirstOrDefaultAsync(link => link.IdSocialMediaPK == linkId && link.IdProfileFK == profileId)
                    .ConfigureAwait(false);

                if (targetLink != null)
                {
                    targetLink.Link = url;
                    await databaseContext.SaveChangesAsync().ConfigureAwait(false);
                }

                return targetLink;
            }
        }
    }
}