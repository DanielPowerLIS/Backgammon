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
            catch (CommunicationException communicationException)
            {
                ApplicationLogger.LogError(communicationException,
                    "Service response received, but a communication failure prevented the channel from closing.");
            }
            catch (TimeoutException timeoutException)
            {
                ApplicationLogger.LogError(timeoutException,
                    "Service response received, but channel closure exceeded the time limit.");
            }
        }
    }
}