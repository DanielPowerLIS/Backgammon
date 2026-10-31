using System.Collections.Generic;
using System.ServiceModel;

using Backgammon.Server.Services.Data;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface IProfileService
    {
        [OperationContract]
        [FaultContract(typeof(PlayerProfileFault))]
        PlayerProfile GetPlayerProfile(string sessionId);

        [OperationContract]
        [FaultContract(typeof(PlayerProfileFault))]
        List<Avatar> GetAvailableAvatars(string sessionId);

        [OperationContract]
        [FaultContract(typeof(PlayerProfileFault))]
        void ChangeAvatar(string sessionId, int idAvatar);

        [OperationContract]
        [FaultContract(typeof(PlayerProfileFault))]
        void UpdateDescription(string sessionId, string description);

        [OperationContract]
        [FaultContract(typeof(PlayerProfileFault))]
        List<SocialMediaType> GetSocialMediaTypes(string sessionId);

        [OperationContract]
        [FaultContract(typeof(PlayerProfileFault))]
        SocialMediaLink AddSocialMediaLink(string sessionId, int socialMediaTypeId, string url);

        [OperationContract]
        [FaultContract(typeof(PlayerProfileFault))]
        void RemoveSocialMediaLink(string sessionId, int socialMediaLinkId);

        [OperationContract]
        [FaultContract(typeof(PlayerProfileFault))]
        SocialMediaLink UpdateSocialMediaLink(string sessionId, int socialMediaLinkId, string url);

    }
}
