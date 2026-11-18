using System;
using System.ComponentModel.DataAnnotations;

namespace web1.Models
{
    /// <summary>
    /// Rate Limit Log - L?u thông tin attempt cho Rate Limiting
    /// Tách riêng bi?t kh?i OTP và BlockList
    /// </summary>
    public class RateLimitLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Identifier { get; set; } // Email, IP, ho?c Device ID

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } // Login, Register, RequestOtp, ResetPassword, etc.

        [StringLength(15)]
        public string IpAddress { get; set; }

        [StringLength(500)]
        public string UserAgent { get; set; }

        [StringLength(100)]
        public string DeviceFingerprint { get; set; }

        public int? UserId { get; set; }

        // Risk score t?i th?i ?i?m ghi nh?n
        [Range(0, 100)]
        public int RiskScore { get; set; } = 0;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; } // ?? t? ??ng cleanup
    }
}
