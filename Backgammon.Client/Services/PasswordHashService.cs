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
        private static readonly int MaximumVerificationIterations = 2000000;
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

        public bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            string[] parts = storedHash.Split('$');
            if (!HasValidHashFormat(parts))
            {
                return false;
            }

            try
            {
                return VerifyParsedHash(password, parts);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private bool HasValidHashFormat(string[] parts)
        {
            if (parts.Length != 4 || parts[0] != "PBKDF2-SHA256")
            {
                return false;
            }

            return int.TryParse(parts[1], out int iterations) &&
                iterations > 0 &&
                iterations <= MaximumVerificationIterations;
        }

        private bool VerifyParsedHash(string password, string[] parts)
        {
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);

            if (salt.Length != SaltLength || expectedHash.Length != HashLength)
            {
                return false;
            }

            using (Rfc2898DeriveBytes derivation = new Rfc2898DeriveBytes(
                password, salt, int.Parse(parts[1]), HashAlgorithmName.SHA256))
            {
                byte[] actualHash = derivation.GetBytes(HashLength);
                return HashesMatch(actualHash, expectedHash);
            }
        }

        private bool HashesMatch(byte[] actualHash, byte[] expectedHash)
        {
            int difference = 0;

            for (int index = 0; index < expectedHash.Length; index++)
            {
                difference |= actualHash[index] ^ expectedHash[index];
            }

            return difference == 0;
        }

    }
}
