using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập bắt buộc")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Tên đăng nhập phải từ 6 đến 50 ký tự")]
        [Index(IsUnique = true)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu bắt buộc")]
        [DataType(DataType.Password)]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Email bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255)]
        [Index(IsUnique = true)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại bắt buộc")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải có 10 số bắt đầu bằng 0")]
        [StringLength(10)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Họ và tên bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ và tên không quá 100 ký tự")]
        public string FullName { get; set; }

        public bool IsVerified { get; set; } = false;

        [StringLength(10)]
        public string OTP { get; set; }

        public DateTime OTPExpire { get; set; }

        [StringLength(20)]
        public string Role { get; set; } = "Customer";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int SpinCount { get; set; } = 0;

        public virtual ICollection<Order> Orders { get; set; }
    }
}
