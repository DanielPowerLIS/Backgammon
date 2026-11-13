

using Backgammon.Server.Services.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Backgammon.Server.Tests.Services.Manager
{
    [TestClass]
    public class PlayerSessionManagerTests
    {
        [TestMethod]
        public void GetSession_UnknownId_ThrowsUnauthorizedAccessException()
        {
            var sessionManager = new PlayerSessionManager();
            string badSessionId = "invalid-session-id";
            bool wasCatched = false;

            try
            {
                sessionManager.GetSession(badSessionId, false);
            }
            catch (UnauthorizedAccessException)
            {
                wasCatched = true;
            }

            Assert.IsTrue(wasCatched);
        }

        [TestMethod]
        public void CreateSession_ValidPlayer_ReturnsStoredSession()
        { 
            var sessionManager = new PlayerSessionManager();
            var playerSession = sessionManager.CreateSession(1, "TestUser");

            try 
            {
                var storedSession = sessionManager.GetSession(playerSession.SessionId,
                    false);

                Assert.IsNotNull(storedSession);
                Assert.AreEqual(playerSession.PlayerId, storedSession.PlayerId);
                Assert.AreEqual(playerSession.Username, storedSession.Username);
                Assert.AreEqual(playerSession.SessionId, storedSession.SessionId);
            }
            finally
            {
                sessionManager.RemoveSession(playerSession.SessionId);
            }

        }

        [TestMethod]
        public void RemoveSession_ExistingSession_CannotBeRetrieved()
        {
            var sessionManager = new PlayerSessionManager();
            var createdSession = sessionManager.CreateSession(1, "TestUser");
            bool wasDeleted = false;


            try
            {
                sessionManager.RemoveSession(createdSession.SessionId);
                sessionManager.GetSession(createdSession.SessionId, false);
            }
            catch (UnauthorizedAccessException)
            {
                wasDeleted= true;
            }

            Assert.IsTrue(wasDeleted);

        }

    }
}
