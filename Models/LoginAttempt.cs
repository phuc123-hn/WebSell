using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    [Table("LoginAttempts")]
    public class LoginAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string IpAddress { get; set; }

        [Required]
        public bool IsSuccessful { get; set; }

        [Required]
        public DateTime AttemptTime { get; set; }

        // Failed attempt reason
        public string FailureReason { get; set; }
    }

    [Table("AccountLockouts")]
    public class AccountLockout
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public DateTime LockedUntil { get; set; }

        [Required]
        public int FailedAttempts { get; set; }

        [Required]
        public DateTime LockTime { get; set; }

        public string Reason { get; set; }
    }
}
