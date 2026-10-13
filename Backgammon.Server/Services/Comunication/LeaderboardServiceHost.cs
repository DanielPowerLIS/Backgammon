using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Server.Services;

namespace Backgammon.Server.Services.Comunication
{
    internal class LeaderboardServiceHost
    {
        public async Task RunAsync()
        {
            var serviceHost = new ServiceHost(typeof(LeaderboardService));

            try
            {
                await Task.Factory.FromAsync(
                    serviceHost.BeginOpen, serviceHost.EndOpen, null);

                Console.WriteLine("Press ENTER to stop the server.");
                await Console.In.ReadLineAsync();

                await Task.Factory.FromAsync(
                    serviceHost.BeginClose, serviceHost.EndClose, null);
            }
            finally
            {
                if (serviceHost.State != CommunicationState.Closed)
                {
                    serviceHost.Abort();
                }
            }
        }
    }
}
