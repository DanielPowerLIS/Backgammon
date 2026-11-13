

using Backgammon.Server.SupportServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Backgammon.Server.Tests.SupportServices
{
    [TestClass]
    public class GuestUsernameGeneratorTests
    {
        private const int PrefixLength = 5;
        private const int DigitCount = 5;

        [TestMethod]
        public void Generate_ValidCall_ReturnsUsernameWithGuestPrefix()
        {
            var usernameGenerator = new GuestUsernameGenerator();

            string username = usernameGenerator.Generate();

            Assert.IsTrue(username.StartsWith("Guest"));
        }

        [TestMethod]
        public void Generate_ValidCall_ReturnsFiveDigitsAfterPrefix()
        {
            var usernameGenerator = new GuestUsernameGenerator();

            string username = usernameGenerator.Generate();
            bool containsOnlyDigits = username.Length == PrefixLength + DigitCount;

            for (int index = PrefixLength; index < username.Length; index++)
            {
                if (username[index] < '0' || username[index] > '9')
                {
                    containsOnlyDigits = false;
                }
            }

            Assert.IsTrue(containsOnlyDigits);
        }
    }
}
