using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.LeaderboardReference;

namespace Backgammon.Client.Comunication
{
    internal class LeaderboardClient
    {
        private static readonly string EndpointConfigurationName =
            "NetTcpBinding_ILeaderboardService";

        public async Task<PlayerStanding[]> GetLeaderboardAsync()
        {
            var serviceClient = new LeaderboardServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                PlayerStanding[] leaderboardEntries =
                    await serviceClient.GetLeaderboardAsync();

                await ServiceChannelCloser.CloseAfterResponseAsync(communicationChannel);
                return leaderboardEntries;
            }
            finally
            {
                if (communicationChannel.State != CommunicationState.Closed)
                {
                    communicationChannel.Abort();
                }
            }
        }
    }
}
