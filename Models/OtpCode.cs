using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    /// <summary>
    /// Model ?? theo dõi các yêu c?u nh?y c?m (??ng ký, thanh toán)
    /// Dùng cho Rate Limiting và Anti-Abuse System
    /// </summary>
    public class RateLimitToken
    {
        [Key]
        public int Id { get; set; }

        // ??nh danh - có th? là IP, Device ID, ho?c Email
        [Required]
        [StringLength(255)]
        [Index(IsUnique = false)]
        public string Identifier { get; set; }

        // Lo?i hành ??ng: Register, Payment, PasswordReset, Login, API
        [Required]
        [StringLength(50)]
        public string ActionType { get; set; }

        // User ID n?u ?ã xác th?c
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // IP Address c?a yêu c?u
        [StringLength(45)]
        public string IpAddress { get; set; }

        // Device Fingerprint (?? phát hi?n bot)
        [StringLength(256)]
        public string DeviceFingerprint { get; set; }

        // User Agent
        [StringLength(500)]
        public string UserAgent { get; set; }

        // S? l?n th? trong time window
        public int AttemptCount { get; set; } = 0;

        // Th?i gian t?o token
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Th?i gian h?t h?n
        public DateTime ExpiresAt { get; set; }

        // Tr?ng thái: Normal, Suspicious, Blocked
        [StringLength(50)]
        public string Status { get; set; } = "Normal";

        // Risk Score (0-100): dùng ?? quy?t ??nh có c?n CAPTCHA hay OTP hay không
        public int RiskScore { get; set; } = 0;

        // Có b?t bu?c CAPTCHA hay không
        public bool RequireCaptcha { get; set; } = false;

        // Lý do b? block (n?u có)
        [StringLength(500)]
        public string BlockReason { get; set; }
    }
}
