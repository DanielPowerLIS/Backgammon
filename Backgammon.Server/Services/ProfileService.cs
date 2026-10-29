using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;

using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.SupportServices;
using Backgammon.Server.SupportServices.Logging;
using Backgammon.Server.Persistence.Repositories.InterfacesRepositories;
using AccountEntity = Backgammon.Server.Persistence.Data.Account;
using AvatarEntity = Backgammon.Server.Persistence.Data.Avatar;
using ProfileEntity = Backgammon.Server.Persistence.Data.Profile;
using SocialMediaEntity = Backgammon.Server.Persistence.Data.SocialMedia;
using SocialMediaLinkEntity = Backgammon.Server.Persistence.Data.SocialMedia;
using SocialMediaTypeEntity = Backgammon.Server.Persistence.Data.TypeSocialMedia;

namespace Backgammon.Server.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly PlayerSessionService _playerSessionService;

        private static readonly string ServiceUnavailableErrorCode = "ServiceUnavailable";
        private static readonly string ProfileNotFoundErrorCode = "ProfileNotFound";

        private static readonly int FacebookTypeId = 1;
        private static readonly int InstagramTypeId = 2;
        private static readonly int TikTokTypeId = 3;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(ProfileService));

        public ProfileService()
        {
            _profileRepository = new ProfileRepository();
            _playerSessionService = new PlayerSessionService();
        }

        public PlayerProfile GetPlayerProfile(string sessionId)
        {
            int profileId = _playerSessionService.GetProfileId(sessionId);
            ProfileEntity dbProfile = FetchProfileDatabaseRecord(profileId);

            if (dbProfile == null)
            {
                ApplicationLogger.LogWarning("Profile ID {0} not found.", profileId);
                throw CreateProfileNotFoundFault();
            }

            PlayerProfile playerProfile = MapToPlayerProfile(dbProfile);

            return playerProfile;
        }

        public List<Avatar> GetAvailableAvatars(string sessionId)
        {
            List<AvatarEntity> dbAvatars = FetchAvatarsFromDatabase();

            List<Avatar> mappedAvatars = dbAvatars
                .Select(MapToAvatarData)
                .ToList();

            return mappedAvatars;
        }

        public void ChangeAvatar(string sessionId, int idAvatar)
        {
            int profileId = _playerSessionService.GetProfileId(sessionId);

            ExecuteAvatarUpdate(profileId, idAvatar);
        }

        public void UpdateDescription(string sessionId, string description)
        {
            int profileId = _playerSessionService.GetProfileId(sessionId);

            ExecuteDescriptionUpdate(profileId, description);
        }

        public List<SocialMediaType> GetSocialMediaTypes(string sessionId)
        {
            List<SocialMediaTypeEntity> dbTypes = FetchSocialMediaTypes();

            List<SocialMediaType> mappedTypes = dbTypes
                .Select(MapToSocialMediaTypeData)
                .ToList();

            return mappedTypes;
        }

        public SocialMediaLink AddSocialMediaLink(
            string sessionId, int socialMediaTypeId, string url)
        {
            int profileId = _playerSessionService.GetProfileId(sessionId);

            SocialMediaLinkEntity dbLink =
                ExecuteAddSocialMediaLink(profileId, socialMediaTypeId, url);

            SocialMediaLink addedLink = MapToSocialMediaLinkData(dbLink);

            return addedLink;
        }

        public void RemoveSocialMediaLink(string sessionId, int socialMediaLinkId)
        {
            int profileId = _playerSessionService.GetProfileId(sessionId);

            ExecuteRemoveSocialMediaLink(profileId, socialMediaLinkId);
        }

        public SocialMediaLink UpdateSocialMediaLink(
            string sessionId, int socialMediaLinkId, string url)
        {
            int profileId = _playerSessionService.GetProfileId(sessionId);

            SocialMediaLinkEntity updatedDbLink =
                ExecuteUpdateSocialMediaLink(profileId, socialMediaLinkId, url);

            SocialMediaLink mappedLink = MapToSocialMediaLinkData(updatedDbLink);

            return mappedLink;
        }

        private ProfileEntity FetchProfileDatabaseRecord(int profileId)
        {
            try
            {
                ProfileEntity foundProfile = _profileRepository
                    .GetFullProfileAsync(profileId)
                    .GetAwaiter()
                    .GetResult();

                return foundProfile;
            }
            catch (Exception databaseException) when (IsDatabaseError(databaseException))
            {
                throw LogAndCreateServiceFault(databaseException, "Failed to load social media types.");
            }
        }

        private List<AvatarEntity> FetchAvatarsFromDatabase()
        {
            try
            {
                List<AvatarEntity> avatars = _profileRepository
                    .GetAvatarsAsync()
                    .GetAwaiter()
                    .GetResult();

                return avatars;
            }
            catch (Exception databaseException) when (IsDatabaseError(databaseException))
            {
                throw LogAndCreateServiceFault(databaseException, "Failed to load social media types.");
            }
        }

        private void ExecuteAvatarUpdate(int profileId, int idAvatar)
        {
            try
            {
                _profileRepository
                    .ChangeAvatarAsync(profileId, idAvatar)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception databaseException) when (IsDatabaseError(databaseException))
            {
                throw LogAndCreateServiceFault(databaseException, "Failed to load social media types.");
            }
        }

        private void ExecuteDescriptionUpdate(int profileId, string description)
        {
            try
            {
                _profileRepository
                    .UpdateDescriptionAsync(profileId, description)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception databaseException) when (IsDatabaseError(databaseException))
            {
                throw LogAndCreateServiceFault(databaseException, "Failed to load social media types.");
            }
        }

        private List<SocialMediaTypeEntity> FetchSocialMediaTypes()
        {
            try
            {
                List<SocialMediaTypeEntity> types = _profileRepository
                    .GetSocialMediaTypesAsync()
                    .GetAwaiter()
                    .GetResult();

                return types;
            }
            catch (Exception databaseException) when (IsDatabaseError(databaseException))
            {
                throw LogAndCreateServiceFault(databaseException, "Failed to load social media types.");
            }
        }

        private SocialMediaLinkEntity ExecuteAddSocialMediaLink(
            int profileId, int typeId, string url)
        {
            try
            {
                SocialMediaLinkEntity link = _profileRepository
                    .AddSocialMediaLinkAsync(profileId, typeId, url)
                    .GetAwaiter()
                    .GetResult();

                return link;
            }
            catch (Exception databaseException) when (IsDatabaseError(databaseException))
            {
                throw LogAndCreateServiceFault(databaseException, "Failed to load social media types.");
            }
        }

        private void ExecuteRemoveSocialMediaLink(int profileId, int linkId)
        {
            try
            {
                _profileRepository
                    .RemoveSocialMediaLinkAsync(profileId, linkId)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception databaseException) when (IsDatabaseError(databaseException))
            {
                throw LogAndCreateServiceFault(databaseException, "Failed to load social media types.");
            }
        }

        private SocialMediaLinkEntity ExecuteUpdateSocialMediaLink(
            int profileId, int linkId, string url)
        {
            try
            {
                SocialMediaLinkEntity link = _profileRepository
                    .UpdateSocialMediaLinkAsync(profileId, linkId, url)
                    .GetAwaiter()
                    .GetResult();

                return link;
            }
            catch (Exception databaseException) when (IsDatabaseError(databaseException))
            {
                throw LogAndCreateServiceFault(databaseException, "Failed to load social media types.");
            }
        }

        private PlayerProfile MapToPlayerProfile(ProfileEntity dbProfile)
        {
            var playerProfile = new PlayerProfile
            {
                Points = dbProfile.Points,
                Wins = dbProfile.Wins,
                Description = dbProfile.Description
            };

            AccountEntity primaryAccount = dbProfile.Accounts.FirstOrDefault();

            if (primaryAccount != null)
            {
                playerProfile.RegistrationDate = primaryAccount.RegistrationDate;
            }

            AssignSocialMediaLinks(dbProfile, playerProfile);

            if (dbProfile.Avatar != null)
            {
                playerProfile.Avatar = MapToAvatarData(dbProfile.Avatar);
            }

            return playerProfile;
        }

        private void AssignSocialMediaLinks(ProfileEntity dbProfile, PlayerProfile playerProfile)
        {
            foreach (SocialMediaEntity social in dbProfile.SocialMedias)
            {
                if (social.IdTypeSocialMediaFK == FacebookTypeId)
                {
                    playerProfile.FacebookLink = social.Link;
                }
                else if (social.IdTypeSocialMediaFK == InstagramTypeId)
                {
                    playerProfile.InstagramLink = social.Link;
                }
                else if (social.IdTypeSocialMediaFK == TikTokTypeId)
                {
                    playerProfile.TikTokLink = social.Link;
                }
            }
        }

        private Avatar MapToAvatarData(AvatarEntity dbAvatar)
        {
            if (dbAvatar == null)
            {
                var emptyAvatar = new Avatar();
                return emptyAvatar;
            }

            var dataAvatar = new Avatar
            {
                IdAvatar = dbAvatar.IdAvatarPK,
                ImageBytes = dbAvatar.AvailableAvatar
            };

            return dataAvatar;
        }

        private SocialMediaType MapToSocialMediaTypeData(SocialMediaTypeEntity dbType)
        {
            if (dbType == null)
            {
                var emptyType = new SocialMediaType();
                return emptyType;
            }

            var dataType = new SocialMediaType
            {
                IdType = dbType.IdTypeSocialMediaPK,
                Type = dbType.Type
            };

            return dataType;
        }

        private SocialMediaLink MapToSocialMediaLinkData(SocialMediaLinkEntity dbLink)
        {
            if (dbLink == null)
            {
                var emptyLink = new SocialMediaLink();
                return emptyLink;
            }

            var dataLink = new SocialMediaLink
            {
                IdSocialMedia = dbLink.IdSocialMediaPK,
                IdTypeSocialMedia = dbLink.IdTypeSocialMediaFK,
                Url = dbLink.Link
            };

            return dataLink;
        }

        private bool IsDatabaseError(Exception ex)
        {
            bool isDbError = ex is EntityException || ex is SqlException;

            return isDbError;
        }

        private FaultException<ProfileNotFoundFault> LogAndCreateServiceFault(
                Exception errorException,
                string logMessage)
        {
            ApplicationLogger.LogError(errorException, logMessage);

            var profileFault = new ProfileNotFoundFault { ErrorCode = ServiceUnavailableErrorCode };
            var faultException = new FaultException<ProfileNotFoundFault>(profileFault);

            return faultException;
        }

        private FaultException<ProfileNotFoundFault> CreateProfileNotFoundFault()
        {
            var profileFault = new ProfileNotFoundFault { ErrorCode = ProfileNotFoundErrorCode };
            var faultException = new FaultException<ProfileNotFoundFault>(profileFault);

            return faultException;
        }
    }
}