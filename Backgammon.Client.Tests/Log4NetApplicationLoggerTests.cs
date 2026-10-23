using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net;
using log4net.Appender;
using log4net.Core;

using Backgammon.Client.Settings.Logging;

namespace Backgammon.Client.Tests
{
    [TestClass]
    public class Log4NetApplicationLoggerTests
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(Log4NetApplicationLoggerTests));

        [TestMethod]
        public void LogInformation_ValidArguments_StoresFormattedEvent()
        {
            MemoryAppender memoryAppender = LogManager
                .GetRepository(typeof(Log4NetApplicationLogger).Assembly)
                .GetAppenders()
                .OfType<MemoryAppender>()
                .Single();

            string operationId = Guid.NewGuid().ToString();
            string expectedMessage = $"Test operation {operationId} completed with 42 points.";

            ApplicationLogger.LogInformation(
                "Test operation {0} completed with {1} points.", operationId, 42);

            LoggingEvent[] matchingEvents = memoryAppender.GetEvents()
                .Where(logEvent => logEvent.RenderedMessage == expectedMessage)
                .ToArray();

            Assert.AreEqual(1, matchingEvents.Length);
            Assert.AreEqual(Level.Info, matchingEvents[0].Level);
            Assert.AreEqual(
                typeof(Log4NetApplicationLoggerTests).FullName,
                matchingEvents[0].LoggerName);
        }
    }
}