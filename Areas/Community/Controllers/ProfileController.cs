using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;

namespace web1.Areas.Community.Controllers
{
    public class ProfileController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Community/Profile/Index/2
        public ActionResult Index(int? userId)
        {
            if (!userId.HasValue)
            {
                // Nếu không có userId, lấy user hiện tại
                userId = (int)(Session["UserId"] ?? 0);
                if (userId == 0)
                    return RedirectToAction("Login", "Account", new { area = "" });
            }

            var user = db.Users.Find(userId.Value);
            if (user == null)
                return HttpNotFound();

            // Lấy các bài đăng của user
            var userPosts = db.Posts
                .Where(p => p.UserId == userId.Value && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            ViewBag.User = user;
            return View(userPosts);
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