using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class EventQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Sự kiện bắt buộc")]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [Required(ErrorMessage = "Nội dung câu hỏi bắt buộc")]
        [StringLength(500)]
        public string Question { get; set; }

        [Required(ErrorMessage = "Đáp án A bắt buộc")]
        [StringLength(255)]
        public string OptionA { get; set; }

        [Required(ErrorMessage = "Đáp án B bắt buộc")]
        [StringLength(255)]
        public string OptionB { get; set; }

        [Required(ErrorMessage = "Đáp án C bắt buộc")]
        [StringLength(255)]
        public string OptionC { get; set; }

        [Required(ErrorMessage = "Đáp án D bắt buộc")]
        [StringLength(255)]
        public string OptionD { get; set; }

        [Required(ErrorMessage = "Đáp án đúng bắt buộc")]
        [StringLength(1)]
        public string CorrectAnswer { get; set; } // A, B, C, D

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}