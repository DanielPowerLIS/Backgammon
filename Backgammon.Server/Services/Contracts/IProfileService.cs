using Backgammon.Server.Services.Data;
using System.Collections.Generic;
using System.ServiceModel;

namespace Backgammon.Server.Services.Contracts
{
    [ServiceContract]
    public interface IProfileService
    {
        [OperationContract]
        List<Avatar> GetAvailableAvatars(string sessionId);

        [OperationContract]
        void ChangeAvatar(string sessionId, int idAvatar);

        [OperationContract]
        void UpdateDescription(string sessionId, string description);

        [OperationContract]
        List<SocialMediaType> GetSocialMediaTypes(string sessionId);

        [OperationContract]
        [FaultContract(typeof(SocialMediaLinkAlreadyExistsFault))]
        SocialMediaLink AddSocialMediaLink(string sessionId, int socialMediaTypeId, string url);

        [OperationContract]
        void RemoveSocialMediaLink(string sessionId, int socialMediaLinkId);

        [OperationContract]
        SocialMediaLink UpdateSocialMediaLink(string sessionId, int socialMediaLinkId, string url);


    }
}
