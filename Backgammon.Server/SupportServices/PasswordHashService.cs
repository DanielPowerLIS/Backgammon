using System;
using System.Security.Cryptography;

using Backgammon.Server.SupportServices.Logging;

namespace Backgammon.Server.SupportServices
{
    internal class PasswordHashService
    {
        private static readonly IApplicationLogger ApplicationLogger =
            new Log4NetApplicationLogger(typeof(PasswordHashService));

        private static readonly int MaximumVerificationIterations = 2000000;
        private static readonly int SaltLength = 16;
        private static readonly int HashLength = 32;
        private static readonly int IterationCount = 600000;

        public string HashPassword(string password)
        {
            byte[] salt = new byte[SaltLength];

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);
            }

            using (Rfc2898DeriveBytes derivation = new Rfc2898DeriveBytes(
                    password, salt, IterationCount, HashAlgorithmName.SHA256))
            {
                byte[] hash = derivation.GetBytes(HashLength);

                string encodedSalt = Convert.ToBase64String(salt);
                string encodedHash = Convert.ToBase64String(hash);
                string passwordHash = $"PBKDF2-SHA256${IterationCount}${encodedSalt}${encodedHash}";

                return passwordHash;
            }
        }

        public bool VerifyPassword(string password, string storedHash)
        {
            bool passwordVerified = false;

            if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(storedHash))
            {
                string[] parts = storedHash.Split('$');

                try
                {
                    bool validHashFormat = HasValidHashFormat(parts);

                    if (validHashFormat)
                    {
                        passwordVerified = VerifyParsedHash(password, parts);
                    }
                }
                catch (FormatException formatException)
                {
                    ApplicationLogger.LogError(
                        formatException,
                        "Password verification failed because the stored password hash has an invalid format.");
                }
                catch (OverflowException overflowException)
                {
                    ApplicationLogger.LogError(
                        overflowException,
                        "Password verification failed because the stored iteration count is out of range.");
                }
            }

            return passwordVerified;
        }

        private bool HasValidHashFormat(string[] parts)
        {
            bool validHashFormat = false;

            if (parts.Length == 4 && parts[0] == "PBKDF2-SHA256")
            {
                int iterations = int.Parse(parts[1]);

                validHashFormat = iterations > 0 && iterations <= MaximumVerificationIterations;
            }

            return validHashFormat;
        }

        private bool VerifyParsedHash(string password, string[] parts)
        {
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);

            Rfc2898DeriveBytes derivation = new Rfc2898DeriveBytes(password, salt, 
                int.Parse(parts[1]), HashAlgorithmName.SHA256);

            byte[] actualHash;

            try
            {
                actualHash = derivation.GetBytes(HashLength);
            }
            finally
            {
                derivation.Dispose();
            }

            bool passwordVerified = HashesMatch(actualHash, expectedHash);

            return passwordVerified;
        }

        private bool HashesMatch(byte[] actualHash, byte[] expectedHash)
        {
            int difference = 0;

            for (int index = 0; index < expectedHash.Length; index++)
            {
                difference |= actualHash[index] ^ expectedHash[index];
            }

            bool hashesMatch = difference == 0;

            return hashesMatch;
        }

    }
}
