using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;

namespace web1.Areas.Community.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Community/Admin/Posts
        public ActionResult Posts()
        {
            // Kiểm tra role
            if (!User.IsInRole("Admin"))
            {
                return new HttpUnauthorizedResult();
            }

            // Lấy tất cả bài (bao gồm cả đã xóa)
            var posts = db.Posts
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View(posts);
        }

        // POST: Community/Admin/WarningPost/5
        [HttpPost]
        public ActionResult WarningPost(int id, string reason)
        {
            var post = db.Posts.Find(id);
            if (post == null || post.IsDeleted)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại" });
            }

            int adminId = (int)(Session["UserId"] ?? 0);

            try
            {
                // Thêm warning
                var warning = new PostWarning
                {
                    PostId = id,
                    AdminId = adminId,
                    Reason = reason,
                    WarningDate = DateTime.Now,
                    Action = 0
                };

                db.PostWarnings.Add(warning);
                db.SaveChanges();

                // Kiểm tra warning count của user
                int warningCount = db.PostWarnings
                    .Where(w => w.Post.UserId == post.UserId && w.Action == 0)
                    .Select(w => w.Post.UserId)
                    .Distinct()
                    .Count();

                // Nếu >= 5 lần cảnh báo -> Ban user
                if (warningCount >= 5)
                {
                    BanUser(post.UserId, "Vi phạm cộng đồng nhiều lần (5 cảnh báo)");
                    return Json(new { success = true, message = "Cảnh báo bài viết. User đã bị ban do cảnh báo >= 5 lần" });
                }

                return Json(new { success = true, message = "Đã cảnh báo bài viết (" + warningCount + "/5)" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Community/Admin/DeletePost/5
        [HttpPost]
        public ActionResult DeletePost(int id)
        {
            var post = db.Posts.Find(id);

            if (post == null)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại" });
            }

            int adminId = (int)(Session["UserId"] ?? 0);

            try
            {
                // Thêm warning action
                var warning = new PostWarning
                {
                    PostId = id,
                    AdminId = adminId,
                    Reason = "Bài viết bị gỡ bởi Admin",
                    WarningDate = DateTime.Now,
                    Action = 1  // Removed
                };

                db.PostWarnings.Add(warning);

                // Soft delete
                post.IsDeleted = true;
                db.SaveChanges();

                return Json(new { success = true, message = "Bài đã được xóa" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Community/Admin/BanUser/5
        [HttpPost]
        public ActionResult BanUser(int userId, string reason, int banDays = 7)
        {
            var user = db.Users.Find(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User không tồn tại" });
            }

            int adminId = (int)(Session["UserId"] ?? 0);

            try
            {
                var existingBan = db.UserBans.FirstOrDefault(ub => ub.UserId == userId);

                if (existingBan == null)
                {
                    var userBan = new UserBan
                    {
                        UserId = userId,
                        IsBanned = true,
                        BanStartDate = DateTime.Now,
                        BanEndDate = DateTime.Now.AddDays(banDays),
                        BanReason = reason,
                        BanDays = banDays,
                        WarningCount = 5,
                        CreatedAt = DateTime.Now
                    };

                    db.UserBans.Add(userBan);
                }
                else
                {
                    existingBan.IsBanned = true;
                    existingBan.BanStartDate = DateTime.Now;
                    existingBan.BanEndDate = DateTime.Now.AddDays(banDays);
                    existingBan.BanReason = reason;
                    existingBan.BanDays = banDays;
                    existingBan.UpdatedAt = DateTime.Now;
                }

                db.SaveChanges();

                return Json(new { success = true, message = "Đã ban user khỏi community " + banDays + " ngày" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Community/Admin/UnbanUser/5
        [HttpPost]
        public ActionResult UnbanUser(int userId)
        {
            var userBan = db.UserBans.FirstOrDefault(ub => ub.UserId == userId);
            if (userBan == null)
            {
                return Json(new { success = false, message = "User không bị ban" });
            }

            try
            {
                userBan.IsBanned = false;
                userBan.BanEndDate = DateTime.Now;
                userBan.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                return Json(new { success = true, message = "Đã bỏ ban user" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Community/Admin/RestorePost/5
        [HttpPost]
        public ActionResult RestorePost(int id)
        {
            var post = db.Posts.Find(id);

            if (post == null)
            {
                return Json(new { success = false, message = "Bài đăng không tồn tại" });
            }

            try
            {
                // Restore bài
                post.IsDeleted = false;
                db.SaveChanges();

                return Json(new { success = true, message = "Bài đã được khôi phục" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
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