using System;
using System.Security.Cryptography;
using System.Text;

namespace web1.Infrastructure.Security
{
    /// <summary>
    /// Crypto Service - Hash OTP và so sánh an toàn
    /// Tránh Plain Text OTP trong database
    /// </summary>
    public static class CryptoService
    {
        /// <summary>
        /// Hash OTP b?ng SHA256
        /// </summary>
        public static string HashOtp(string otp)
        {
            if (string.IsNullOrEmpty(otp))
                throw new ArgumentException("OTP cannot be null or empty");

            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Xác minh OTP v?i hash (So sánh an toàn - constant-time comparison)
        /// </summary>
        public static bool VerifyOtp(string plainOtp, string hashFromDb)
        {
            if (string.IsNullOrEmpty(plainOtp) || string.IsNullOrEmpty(hashFromDb))
                return false;

            string plainHash = HashOtp(plainOtp);
            
            // Constant-time comparison ?? tránh timing attacks
            return SlowEquals(plainHash, hashFromDb);
        }

        /// <summary>
        /// Constant-time string comparison - ch?ng timing attack
        /// </summary>
        private static bool SlowEquals(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return false;

            if (a.Length != b.Length)
                return false;

            int differences = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    differences++;
            }

            return differences == 0;
        }

        /// <summary>
        /// Generate secure random OTP (6 digits)
        /// </summary>
        public static string GenerateOtp()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[4];
                rng.GetBytes(tokenData);
                int randomNumber = Math.Abs(BitConverter.ToInt32(tokenData, 0)) % 1000000;
                return randomNumber.ToString("D6");
            }
        }
    }
}
