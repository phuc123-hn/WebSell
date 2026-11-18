using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;

namespace web1.Areas.Community.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // POST: Community/Comment/Add
        [HttpPost]
        public ActionResult Add(int postId, string content)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "N?i dung bình lu?n không ???c ?? tr?ng" });
            }

            if (content.Length > 2000)
            {
                return Json(new { success = false, message = "Bình lu?n không ???c quá 2000 ký t?" });
            }

            var post = db.Posts.Find(postId);
            if (post == null || post.IsDeleted)
            {
                return Json(new { success = false, message = "Bài ??ng không t?n t?i" });
            }

            int userId = (int)(Session["UserId"] ?? 0);

            try
            {
                var comment = new Comment
                {
                    PostId = postId,
                    UserId = userId,
                    Content = content,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                db.Comments.Add(comment);
                db.SaveChanges();

                return Json(new 
                { 
                    success = true, 
                    message = "Bình lu?n ?ã ???c thêm",
                    commentId = comment.Id,
                    createdAt = comment.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "L?i: " + ex.Message });
            }
        }

        // POST: Community/Comment/Delete
        [HttpPost]
        public ActionResult Delete(int commentId)
        {
            var comment = db.Comments.Find(commentId);

            if (comment == null || comment.IsDeleted)
            {
                return Json(new { success = false, message = "Bình lu?n không t?n t?i" });
            }

            int userId = (int)(Session["UserId"] ?? 0);

            // Ch? chính ch? ho?c admin m?i ???c xóa
            if (comment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "B?n không có quy?n xóa bình lu?n này" });
            }

            try
            {
                // Soft delete
                comment.IsDeleted = true;
                db.SaveChanges();

                return Json(new { success = true, message = "Bình lu?n ?ã ???c xóa" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "L?i: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
