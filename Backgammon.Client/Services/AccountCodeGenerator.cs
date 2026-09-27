using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon.Client.Services
{
    internal class AccountCodeGenerator
    {

        private static readonly string AllowedCharacters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly int CodeLength = 8;

        public string Generate()
        {
            char[] code = new char[CodeLength];
            byte[] randomByte = new byte[1];
            int sampleLimit = byte.MaxValue + 1
                - (byte.MaxValue + 1) % AllowedCharacters.Length;

            using (RandomNumberGenerator generator =
                RandomNumberGenerator.Create())
            {
                for (int index = 0; index < code.Length; index++)
                {
                    do
                    {
                        generator.GetBytes(randomByte);
                    }
                    while (randomByte[0] >= sampleLimit);

                    code[index] = AllowedCharacters[
                        randomByte[0] % AllowedCharacters.Length];
                }
            }

            return new string(code);
        }

    }
}
