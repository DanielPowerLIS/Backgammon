using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Backgammon.Client.Settings.Logging;

namespace Backgammon.Client.Comunication
{
    internal static class ServiceChannelCloser
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(ServiceChannelCloser));

        public static async Task CloseAfterResponseAsync(
            ICommunicationObject communicationChannel)
        {
            try
            {
                await Task.Factory.FromAsync(
                    communicationChannel.BeginClose,
                    communicationChannel.EndClose,
                    null);
            }
            catch (Exception closeException) when (
                closeException is CommunicationException ||
                closeException is TimeoutException)
            {
                ApplicationLogger.LogError(
                    closeException,
                    "Service response received, but channel closure failed.");
            }
        }
    }
}