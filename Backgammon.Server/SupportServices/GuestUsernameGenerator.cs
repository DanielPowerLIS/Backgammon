using System.Security.Cryptography;

namespace Backgammon.Server.SupportServices
{
    internal class GuestUsernameGenerator
    {
        private static readonly string UsernamePrefix = "Guest";
        private static readonly string AllowedDigits = "0123456789";
        private static readonly int DigitCount = 5;

        public string Generate()
        {
            char[] digits = new char[DigitCount];
            byte[] randomByte = new byte[1];
            int sampleLimit = byte.MaxValue + 1
                - (byte.MaxValue + 1) % AllowedDigits.Length;

            using (RandomNumberGenerator generator =
                RandomNumberGenerator.Create())
            {
                for (int index = 0; index < digits.Length; index++)
                {
                    do
                    {
                        generator.GetBytes(randomByte);
                    }
                    while (randomByte[0] >= sampleLimit);

                    digits[index] = AllowedDigits[
                        randomByte[0] % AllowedDigits.Length];
                }
            }

            string guestUsername = UsernamePrefix + new string(digits);

            return guestUsername;
        }
    }
}   