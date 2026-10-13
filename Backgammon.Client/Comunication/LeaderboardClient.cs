using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.LeaderboardReference;

namespace Backgammon.Client.Comunication
{
    internal class LeaderboardClient
    {
        private static readonly string EndpointConfigurationName =
            "NetTcpBinding_LeaderboardService";

        public async Task<LeaderboardEntry[]> GetLeaderboardAsync()
        {
            var serviceClient =
                new LeaderboardServiceClient(EndpointConfigurationName);
            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                LeaderboardEntry[] leaderboardEntries =
                    await serviceClient.GetLeaderboardAsync();

                await Task.Factory.FromAsync(
                    communicationChannel.BeginClose,
                    communicationChannel.EndClose, null);

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