using System;
using System.Security.Cryptography;
using System.Text;

namespace web1.Infrastructure.Security
{
    /// <summary>
    /// Password hashing service using PBKDF2
    /// More secure than SHA256 alone
    /// </summary>
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class PasswordHasherService : IPasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 20; // 160 bits
        private const int Iterations = 10000; // PBKDF2 iterations

        /// <summary>
        /// Hash password with salt using PBKDF2
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty");

            // Generate random salt
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);

                // Hash password with salt
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(HashSize);

                    // Combine salt and hash: salt (16 bytes) + hash (20 bytes)
                    byte[] hashWithSalt = new byte[SaltSize + HashSize];
                    Array.Copy(salt, 0, hashWithSalt, 0, SaltSize);
                    Array.Copy(hash, 0, hashWithSalt, SaltSize, HashSize);

                    // Return as base64 string
                    return Convert.ToBase64String(hashWithSalt);
                }
            }
        }

        /// <summary>
        /// Verify password against stored hash
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                // Extract salt and hash from stored value
                byte[] hashWithSalt = Convert.FromBase64String(hash);

                if (hashWithSalt.Length != SaltSize + HashSize)
                    return false;

                byte[] salt = new byte[SaltSize];
                Array.Copy(hashWithSalt, 0, salt, 0, SaltSize);

                // Hash input password with extracted salt
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] computedHash = pbkdf2.GetBytes(HashSize);

                    // Compare hashes
                    for (int i = 0; i < HashSize; i++)
                    {
                        if (hashWithSalt[SaltSize + i] != computedHash[i])
                            return false;
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
