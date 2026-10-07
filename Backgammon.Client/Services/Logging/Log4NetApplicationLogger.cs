using System;
using System.Globalization;

using log4net;

namespace Backgammon.Client.Services.Logging
{
    public sealed class Log4NetApplicationLogger : IApplicationLogger
    {
        private readonly ILog _applicationLogger;

        public Log4NetApplicationLogger(Type sourceType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            _applicationLogger = LogManager.GetLogger(sourceType);
        }

        public void LogInformation(
            string messageTemplate,
            params object[] argumentValues)
        {
            _applicationLogger.InfoFormat(
                CultureInfo.InvariantCulture,
                messageTemplate,
                argumentValues);
        }

        public void LogWarning(
            string messageTemplate,
            params object[] argumentValues)
        {
            _applicationLogger.WarnFormat(
                CultureInfo.InvariantCulture,
                messageTemplate,
                argumentValues);
        }

        public void LogError(
            Exception errorException,
            string messageTemplate,
            params object[] argumentValues)
        {
            if (!_applicationLogger.IsErrorEnabled)
            {
                return;
            }

            string formattedMessage = string.Format(
                CultureInfo.InvariantCulture,
                messageTemplate,
                argumentValues);

            _applicationLogger.Error(formattedMessage, errorException);
        }
    }
}
