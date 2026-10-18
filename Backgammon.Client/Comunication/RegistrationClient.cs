using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.RegistrationReference;

namespace Backgammon.Client.Comunication
{
    internal class RegistrationClient
    {
        private static readonly string EndpointConfigurationName =
            "NetTcpBinding_IRegistrationService";

        public async Task RegisterAccountAsync(Account accountInformation)
        {
            var serviceClient = new RegistrationServiceClient(EndpointConfigurationName);

            ICommunicationObject communicationChannel = serviceClient;

            try
            {
                await serviceClient.RegisterAccountAsync(accountInformation);

                await Task.Factory.FromAsync(
                    communicationChannel.BeginClose,
                    communicationChannel.EndClose,
                    null);
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
