using System;
using System.ComponentModel.DataAnnotations;

namespace web1.Models
{
    /// <summary>
    /// OTP Token - L?u OTP ?ã ???c hash SHA256 (B?o m?t)
    /// Tách riêng bi?t kh?i RateLimit và BlockList
    /// </summary>
    public class OtpToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(64)] // SHA256 hash = 64 hex characters
        public string OtpHash { get; set; } // Hash SHA256 c?a OTP

        [Required]
        [StringLength(50)]
        public string OtpType { get; set; } // Register, ResetPassword, PaymentVerification, etc.

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // Pending, Verified, Expired

        public int? UserId { get; set; }

        // Th?i gian h?t h?n
        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? VerifiedAt { get; set; }

        // S? l?n th? OTP sai (?? ch?ng brute force)
        [Range(0, 10)]
        public int FailedAttempts { get; set; } = 0;
    }
}
