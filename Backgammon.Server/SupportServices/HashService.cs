using System;
using System.Security.Cryptography;
using System.Text;

namespace Backgammon.Server.SupportServices
{
    internal class HashService
    {
        public byte[] ComputeHash(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            byte[] hash;

            using (SHA256 algorithm = SHA256.Create())
            {
                hash = algorithm.ComputeHash(valueBytes);
            }

            return hash;
        }

        public bool VerifyHash(string value, byte[] expectedHash)
        {
            if (expectedHash == null)
            {
                throw new ArgumentNullException(nameof(expectedHash));
            }

            byte[] actualHash = ComputeHash(value);

            bool hashesMatch = HashesMatch(actualHash, expectedHash);

            return hashesMatch;
        }

        private bool HashesMatch(byte[] actualHash, byte[] expectedHash)
        {
            if (actualHash.Length != expectedHash.Length)
            {
                return false;
            }

            int difference = 0;

            for (int index = 0; index < actualHash.Length; index++)
            {
                difference |= actualHash[index] ^ expectedHash[index];
            }

            bool hashesMatch = difference == 0;

            return hashesMatch;
        }
    }
}