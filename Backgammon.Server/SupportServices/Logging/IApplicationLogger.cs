using System;

namespace Backgammon.Server.SupportServices.Logging
{
    public interface IApplicationLogger
    {
        void LogInformation(
            string messageTemplate,
            params object[] argumentValues);

        void LogWarning(
            string messageTemplate,
            params object[] argumentValues);

        void LogError( 
            Exception errorException,
            string messageTemplate,
            params object[] argumentValues);

        void LogDebug(
            string messageTemplate,
            params object[] argumentValues);

        void LogFatal(
            Exception errorException,
            string messageTemplate,
            params object[] argumentValues);
    }
}
