using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required(ErrorMessage = "Tiêu đề bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không quá 255 ký tự")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung bắt buộc")]
        [StringLength(5000, ErrorMessage = "Nội dung không quá 5000 ký tự")]
        public string Content { get; set; }

        /// <summary>
        /// 0 = Review (Đánh giá)
        /// 1 = Rent (Thuê Đồ)
        /// </summary>
        [Required]
        public int Type { get; set; }

        /// <summary>
        /// Rating từ 1-5 (chỉ cho Review, Type=0)
        /// </summary>
        public int? Rating { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete: true = đã xóa, false = còn hiển thị
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Hình ảnh/ảnh sản phẩm cho bài viết
        /// </summary>
        public string ImageUrl { get; set; }

        // Navigation
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}
