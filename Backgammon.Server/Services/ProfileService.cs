using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

using Backgammon.Server.Persistence.Repositories;
using Backgammon.Server.Services.Contracts;
using Backgammon.Server.Services.Data;
using Backgammon.Server.Services.ExceptionCode;
using Backgammon.Server.Services.Manager;
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
        private readonly PlayerSessionManager _playerSessionManager;

        private static readonly string ProfileNotFoundErrorCode = "ProfileNotFound";

        private static readonly int FacebookTypeId = 1;
        private static readonly int InstagramTypeId = 2;
        private static readonly int TikTokTypeId = 3;

        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(ProfileService));

        public ProfileService()
        {
            _profileRepository = new ProfileRepository();
            _playerSessionManager = new PlayerSessionManager();
        }

        public PlayerProfile GetPlayerProfile(string sessionId)
        {
            int profileId = _playerSessionManager.GetProfileId(sessionId);
            ProfileEntity dbProfile = FetchProfileDatabaseRecord(profileId);

            if (dbProfile == null)
            {
                ApplicationLogger.LogWarning("Profile ID {0} not found.", profileId);
                throw CreatePlayerProfileFault(ProfileNotFoundErrorCode);
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
            int profileId = _playerSessionManager.GetProfileId(sessionId);

            ExecuteAvatarUpdate(profileId, idAvatar);
        }

        public void UpdateDescription(string sessionId, string description)
        {
            int profileId = _playerSessionManager.GetProfileId(sessionId);

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

        public SocialMediaLink AddSocialMediaLink(string sessionId, int socialMediaTypeId, string url)
        {
            int profileId = _playerSessionManager.GetProfileId(sessionId);

            SocialMediaLinkEntity dbLink =
                ExecuteAddSocialMediaLink(profileId, socialMediaTypeId, url);

            SocialMediaLink addedLink = MapToSocialMediaLinkData(dbLink);

            return addedLink;
        }

        public void RemoveSocialMediaLink(string sessionId, int socialMediaLinkId)
        {
            int profileId = _playerSessionManager.GetProfileId(sessionId);

            ExecuteRemoveSocialMediaLink(profileId, socialMediaLinkId);
        }

        public SocialMediaLink UpdateSocialMediaLink(
            string sessionId, int socialMediaLinkId, string url)
        {
            int profileId = _playerSessionManager.GetProfileId(sessionId);

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
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented the player profile from loading.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException,
                    "A SQL Server failure prevented the player profile from loading.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
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
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented the available avatars from loading.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException,
                    "A SQL Server failure prevented the available avatars from loading.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
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
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented the avatar from being changed.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException,
                    "A SQL Server failure prevented the avatar from being changed.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbUpdateException updateException)
            {
                ApplicationLogger.LogError(updateException,
                    "The avatar change could not be saved to the database.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbEntityValidationException validationException)
            {
                ApplicationLogger.LogError(validationException,
                    "Entity validation failed while saving the avatar change.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
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
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(
                    entityException,
                    "An Entity Framework failure prevented the profile description from being updated.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(
                    databaseException,
                    "A SQL Server failure prevented the profile description from being updated.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbUpdateException updateException)
            {
                ApplicationLogger.LogError(
                    updateException,
                    "The profile description could not be saved to the database.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbEntityValidationException validationException)
            {
                ApplicationLogger.LogError(
                    validationException,
                    "Entity validation failed while saving the profile description.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
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
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented the social media types from loading.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException,
                    "A SQL Server failure prevented the social media types from loading.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
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
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented the social media link from being added.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException,
                    "A SQL Server failure prevented the social media link from being added.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbUpdateException updateException)
            {
                ApplicationLogger.LogError(updateException,
                    "The new social media link could not be saved to the database.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbEntityValidationException validationException)
            {
                ApplicationLogger.LogError(validationException,
                    "Entity validation failed while saving the new social media link.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
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
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented the social media link from being removed.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException,
                    "A SQL Server failure prevented the social media link from being removed.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbUpdateException updateException)
            {
                ApplicationLogger.LogError(updateException,
                    "The social media link deletion could not be saved to the database.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
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
            catch (EntityException entityException)
            {
                ApplicationLogger.LogError(entityException,
                    "An Entity Framework failure prevented the social media link from being updated.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (SqlException databaseException)
            {
                ApplicationLogger.LogError(databaseException,
                    "A SQL Server failure prevented the social media link from being updated.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbUpdateException updateException)
            {
                ApplicationLogger.LogError(updateException,
                    "The social media link changes could not be saved to the database.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
            }
            catch (DbEntityValidationException validationException)
            {
                ApplicationLogger.LogError(validationException,
                    "Entity validation failed while saving the social media link changes.");

                throw CreatePlayerProfileFault(ErrorCodes.ServiceUnavailable);
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

        private FaultException<PlayerProfileFault> CreatePlayerProfileFault(
            string errorCode)
        {
            var profileFault = new PlayerProfileFault
            {
                ErrorCode = errorCode
            };

            var faultException =
                new FaultException<PlayerProfileFault>(profileFault);

            return faultException;
        }
    }
}
