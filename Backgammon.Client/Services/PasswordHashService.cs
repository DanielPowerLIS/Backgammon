using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon.Client.Services
{
    internal class PasswordHashService
    {

        private static readonly int SaltLength = 16;
        private static readonly int HashLength = 32;
        private static readonly int IterationCount = 600000;

        public string HashPassword(string password)
        {
            byte[] salt = new byte[SaltLength];

            using (RandomNumberGenerator generator =
                RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);
            }

            byte[] hash;
            using (Rfc2898DeriveBytes derivation =
                new Rfc2898DeriveBytes(
                    password, salt, IterationCount, HashAlgorithmName.SHA256))
            {
                hash = derivation.GetBytes(HashLength);
            }

            string encodedSalt = Convert.ToBase64String(salt);
            string encodedHash = Convert.ToBase64String(hash);

            return $"PBKDF2-SHA256${IterationCount}${encodedSalt}${encodedHash}";
        }

    }
}
