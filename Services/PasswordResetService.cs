using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using web1.Data;
using web1.Models;
using web1.Infrastructure.Logging;
using web1.Infrastructure.Security;

namespace web1.Services
{
    public interface IPasswordResetService
    {
        string GenerateResetToken();
        bool CreateResetToken(int userId, out string token);
        bool VerifyResetToken(string token, out int userId);
        bool ResetPassword(string token, string newPassword, IPasswordHasher passwordHasher);
        bool InvalidateAllTokens(int userId);
        bool IsTokenExpired(string token);
    }

    public class PasswordResetService : IPasswordResetService
    {
        private readonly ApplicationDbContext _db;
        private readonly FileLogger _logger = new FileLogger();
        private const int TOKEN_EXPIRY_HOURS = 24;

        public PasswordResetService(ApplicationDbContext db = null)
        {
            _db = db ?? new ApplicationDbContext();
        }

        /// <summary>
        /// Generate secure reset token (128-bit GUID converted to base64)
        /// </summary>
        public string GenerateResetToken()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32]; // 256 bits = 32 bytes
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }

        /// <summary>
        /// Create password reset token and save to database
        /// </summary>
        public bool CreateResetToken(int userId, out string token)
        {
            token = null;

            try
            {
                var user = _db.Users.Find(userId);
                if (user == null)
                {
                    _logger.LogWarning($"CreateResetToken: User {userId} not found");
                    return false;
                }

                // Invalidate old tokens
                InvalidateAllTokens(userId);

                token = GenerateResetToken();

                var resetToken = new PasswordResetToken
                {
                    UserId = userId,
                    Token = token,
                    ExpiryTime = DateTime.UtcNow.AddHours(TOKEN_EXPIRY_HOURS),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                _db.PasswordResetTokens.Add(resetToken);
                _db.SaveChanges();

                _logger.LogInfo($"CreateResetToken: Token created for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating reset token for user {userId}", ex);
                return false;
            }
        }

        /// <summary>
        /// Verify if reset token is valid
        /// </summary>
        public bool VerifyResetToken(string token, out int userId)
        {
            userId = 0;

            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("VerifyResetToken: Token is empty");
                    return false;
                }

                var resetToken = _db.PasswordResetTokens
                    .FirstOrDefault(rt => rt.Token == token && !rt.IsUsed);

                if (resetToken == null)
                {
                    _logger.LogWarning("VerifyResetToken: Token not found or already used");
                    return false;
                }

                // Check expiry
                if (DateTime.UtcNow > resetToken.ExpiryTime)
                {
                    _logger.LogWarning($"VerifyResetToken: Token expired");
                    return false;
                }

                userId = resetToken.UserId;
                _logger.LogInfo($"VerifyResetToken: Token verified for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying reset token", ex);
                return false;
            }
        }

        /// <summary>
        /// Reset user password using valid token
        /// </summary>
        public bool ResetPassword(string token, string newPassword, IPasswordHasher passwordHasher)
        {
            try
            {
                if (!VerifyResetToken(token, out int userId))
                {
                    _logger.LogWarning("ResetPassword: Invalid or expired token");
                    return false;
                }

                var user = _db.Users.Find(userId);
                if (user == null)
                {
                    _logger.LogWarning($"ResetPassword: User {userId} not found");
                    return false;
                }

                // Hash new password
                user.PasswordHash = passwordHasher.HashPassword(newPassword);

                // Mark token as used
                var resetToken = _db.PasswordResetTokens.FirstOrDefault(rt => rt.Token == token);
                if (resetToken != null)
                {
                    resetToken.IsUsed = true;
                    resetToken.UsedAt = DateTime.UtcNow;
                }

                _db.SaveChanges();

                _logger.LogInfo($"ResetPassword: Password reset successfully for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resetting password", ex);
                return false;
            }
        }

        /// <summary>
        /// Invalidate all reset tokens for a user
        /// </summary>
        public bool InvalidateAllTokens(int userId)
        {
            try
            {
                var userTokens = _db.PasswordResetTokens
                    .Where(rt => rt.UserId == userId && !rt.IsUsed)
                    .ToList();

                foreach (var token in userTokens)
                {
                    token.IsUsed = true;
                }

                _db.SaveChanges();

                _logger.LogInfo($"InvalidateAllTokens: All tokens invalidated for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error invalidating tokens for user {userId}", ex);
                return false;
            }
        }

        /// <summary>
        /// Check if token is expired
        /// </summary>
        public bool IsTokenExpired(string token)
        {
            try
            {
                var resetToken = _db.PasswordResetTokens
                    .FirstOrDefault(rt => rt.Token == token);

                if (resetToken == null)
                    return true;

                return DateTime.UtcNow > resetToken.ExpiryTime;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking token expiry", ex);
                return true;
            }
        }
    }
}
