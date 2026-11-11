

using Backgammon.Client.MatchReference;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Backgammon.Client.Comunication
{
    internal class MatchClient
    {
        private const string EndpointConfigurationName = "NetTcpBinding_IMatchService";

        public async Task<MatchSession> InitializeMatchAsync(string sessionId, string roomId)
        { 
            var serviceClient = new MatchServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationObject = serviceClient;

            try
            {
                MatchSession matchSession = await serviceClient.InitializeMatchAsync(sessionId, roomId);

                await ServiceChannelCloser.CloseAfterResponseAsync(communicationObject);

                return matchSession;
            }
            finally
            { 
                if (communicationObject.State != CommunicationState.Closed)
                {
                    communicationObject.Abort();
                }
            }
        }

        public async Task<MatchSession> JoinMatchAsync(string sessionId, string roomId, string matchId)
        {
            var serviceClient = new MatchServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationObject = serviceClient;

            try
            {
                MatchSession matchSession = await serviceClient.JoinMatchAsync(sessionId, roomId, matchId);

                await ServiceChannelCloser.CloseAfterResponseAsync(communicationObject);

                return matchSession;
            }
            finally
            {
                if (communicationObject.State != CommunicationState.Closed)
                {
                    communicationObject.Abort();
                }
            }
        }

    }
}
