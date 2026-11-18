using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required(ErrorMessage = "Nội dung tin nhắn bắt buộc")]
        [StringLength(5000, ErrorMessage = "Tin nhắn không quá 5000 ký tự")]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// ?ã ??c hay ch?a (dùng cho admin inbox)
        /// </summary>
        public bool IsRead { get; set; } = false;

        // Note: Tin nh?n s? t? ??ng xóa sau 15 ngày
        // Logic: ch?y ? admin dashboard ho?c background job
    }
}
