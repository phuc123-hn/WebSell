using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;

namespace web1.Areas.Community.Controllers
{
    [Authorize]
    public class LikeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // POST: Community/Like/Toggle (Like/Unlike)
        [HttpPost]
        public ActionResult Toggle(int postId)
        {
            var post = db.Posts.Find(postId);
            if (post == null || post.IsDeleted)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại" });
            }

            int userId = (int)(Session["UserId"] ?? 0);

            try
            {
                var existingLike = db.Likes.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);

                if (existingLike != null)
                {
                    // Unlike
                    db.Likes.Remove(existingLike);
                    db.SaveChanges();

                    var likeCount = db.Likes.Count(l => l.PostId == postId);
                    return Json(new
                    {
                        success = true,
                        liked = false,
                        likeCount = likeCount,
                        message = "Đã bỏ like"
                    });
                }
                else
                {
                    // Like
                    var like = new Like
                    {
                        PostId = postId,
                        UserId = userId,
                        CreatedAt = DateTime.Now
                    };

                    db.Likes.Add(like);
                    db.SaveChanges();

                    var likeCount = db.Likes.Count(l => l.PostId == postId);
                    return Json(new
                    {
                        success = true,
                        liked = true,
                        likeCount = likeCount,
                        message = "Đã like"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Community/Like/Count/5
        public ActionResult Count(int postId)
        {
            var likeCount = db.Likes.Count(l => l.PostId == postId);
            return Json(new { count = likeCount }, JsonRequestBehavior.AllowGet);
        }

        // GET: Community/Like/IsLiked/5
        [Authorize]
        public ActionResult IsLiked(int postId)
        {
            int userId = (int)(Session["UserId"] ?? 0);
            var isLiked = db.Likes.Any(l => l.PostId == postId && l.UserId == userId);

            return Json(new { liked = isLiked }, JsonRequestBehavior.AllowGet);
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