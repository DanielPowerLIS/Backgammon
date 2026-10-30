using System.Security.Cryptography;

namespace Backgammon.Server.SupportServices
{
    internal class CodeGenerator
    {

        private static readonly string AllowedCharacters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public string Generate(int codeLength)
        {
            char[] code = new char[codeLength];
            byte[] randomByte = new byte[1];
            int sampleLimit = byte.MaxValue + 1 - (byte.MaxValue + 1) % AllowedCharacters.Length;

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                for (int index = 0; index < code.Length; index++)
                {
                    do
                    {
                        generator.GetBytes(randomByte);
                    }
                    while (randomByte[0] >= sampleLimit);

                    code[index] = AllowedCharacters[randomByte[0] % AllowedCharacters.Length];
                }
            }
            string codeString = new string(code);

            return codeString;
        }

    }
}
