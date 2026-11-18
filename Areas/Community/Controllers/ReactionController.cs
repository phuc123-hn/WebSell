using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;

namespace web1.Areas.Community.Controllers
{
    [Authorize]
    public class ReactionController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // POST: Community/Reaction/Toggle
        [HttpPost]
        public ActionResult Toggle(int postId, string reactionType)
        {
            // Validate reaction type
            var validTypes = new[] { "like", "love", "haha", "wow", "angry" };
            if (!validTypes.Contains(reactionType?.ToLower()))
            {
                return Json(new { success = false, message = "Loại phản ứng không hợp lệ" });
            }

            var post = db.Posts.Find(postId);
            if (post == null || post.IsDeleted)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại" });
            }

            int userId = (int)(Session["UserId"] ?? 0);
            reactionType = reactionType.ToLower();

            try
            {
                // Kiểm tra xem user đã reaction bài này chưa
                var existingReaction = db.Reactions.FirstOrDefault(r => r.PostId == postId && r.UserId == userId);

                if (existingReaction != null)
                {
                    if (existingReaction.ReactionType == reactionType)
                    {
                        // Loại bỏ reaction nếu cùng loại
                        db.Reactions.Remove(existingReaction);
                        db.SaveChanges();

                        var counts = GetReactionCounts(postId);
                        return Json(new
                        {
                            success = true,
                            reacted = false,
                            message = "Đã loại bỏ phản ứng",
                            counts = counts
                        });
                    }
                    else
                    {
                        // Thay đổi reaction type
                        existingReaction.ReactionType = reactionType;
                        existingReaction.CreatedAt = DateTime.Now;
                        db.SaveChanges();

                        var counts = GetReactionCounts(postId);
                        return Json(new
                        {
                            success = true,
                            reacted = true,
                            message = "Đã cập nhật phản ứng",
                            counts = counts
                        });
                    }
                }
                else
                {
                    // Tạo reaction mới
                    var reaction = new Reaction
                    {
                        PostId = postId,
                        UserId = userId,
                        ReactionType = reactionType,
                        CreatedAt = DateTime.Now
                    };

                    db.Reactions.Add(reaction);
                    db.SaveChanges();

                    var counts = GetReactionCounts(postId);
                    return Json(new
                    {
                        success = true,
                        reacted = true,
                        message = "Đã thêm phản ứng",
                        counts = counts
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Community/Reaction/GetCounts
        [HttpGet]
        public ActionResult GetCounts(int postId)
        {
            var counts = GetReactionCounts(postId);
            int userId = (int)(Session["UserId"] ?? 0);
            var userReaction = db.Reactions.FirstOrDefault(r => r.PostId == postId && r.UserId == userId);

            return Json(new
            {
                counts = counts,
                userReaction = userReaction?.ReactionType
            }, JsonRequestBehavior.AllowGet);
        }

        private dynamic GetReactionCounts(int postId)
        {
            var reactions = db.Reactions.Where(r => r.PostId == postId).ToList();

            return new
            {
                like = reactions.Count(r => r.ReactionType == "like"),
                love = reactions.Count(r => r.ReactionType == "love"),
                haha = reactions.Count(r => r.ReactionType == "haha"),
                wow = reactions.Count(r => r.ReactionType == "wow"),
                angry = reactions.Count(r => r.ReactionType == "angry"),
                total = reactions.Count
            };
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
