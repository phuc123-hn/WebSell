using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    /// <summary>
    /// ApplicationSettings - Store system configuration
    /// </summary>
    public class ApplicationSetting
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Key bắt buộc")]
        [StringLength(100)]
        [Index(IsUnique = true)]
        public string Key { get; set; }

        [StringLength(1000)]
        public string Value { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Maintenance mode setting
    /// </summary>
    public static class ApplicationSettings
    {
        public const string MAINTENANCE_MODE = "MAINTENANCE_MODE"; // true/false
        public const string MAINTENANCE_MESSAGE = "MAINTENANCE_MESSAGE"; // Custom message
        public const string MAX_LOGIN_ATTEMPTS = "MAX_LOGIN_ATTEMPTS"; // Default: 5
        public const string LOCKOUT_DURATION_MINUTES = "LOCKOUT_DURATION"; // Default: 15
        public const string ENABLE_RECAPTCHA = "ENABLE_RECAPTCHA"; // true/false
        public const string RECAPTCHA_SITE_KEY = "RECAPTCHA_SITE_KEY";
        public const string RECAPTCHA_SECRET_KEY = "RECAPTCHA_SECRET_KEY";
    }
}
