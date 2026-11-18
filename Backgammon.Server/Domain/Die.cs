using System.Security.Cryptography;

namespace Backgammon.Server.Domain
{
    internal static class Die
    {
        private const int MinimumValue = 1;
        private const int FaceCount = 6;

        public static int Roll()
        {
            byte[] randomByte = new byte[1];

            int sampleRange = byte.MaxValue + 1;
            int sampleLimit = sampleRange - sampleRange % FaceCount;

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                do
                {
                    generator.GetBytes(randomByte);
                }
                while (randomByte[0] >= sampleLimit);
            }

            int dieValue = MinimumValue + randomByte[0] % FaceCount;

            return dieValue;
        }
    }
}