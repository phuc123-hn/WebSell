using System;
using System.Linq;
using System.Web.Mvc;
using web1.Models;
using web1.Data;
using web1.Helpers;

namespace web1.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Trang chủ - Hiển thị Landing page 1 lần duy nhất mỗi session
        public ActionResult Index()
        {
            bool landingShown = Session["LandingShown"] != null && (bool)Session["LandingShown"];

            if (!landingShown)
            {
                Session["LandingShown"] = true;
                return View("Landing");
            }

            return RedirectToAction("HomePage");
        }

        // Trang chủ thực - Homepage (public)
        public ActionResult HomePage()
        {
            var hotProducts = db.Products
                                .Where(p => p.IsHot)
                                .Take(6)
                                .ToList();

            var latestNews = db.News
                               .Where(n => n.IsActive)
                               .OrderByDescending(n => n.PublishDate)
                               .Take(5)
                               .ToList();

            var categories = db.Categories.ToList();

            ViewBag.HotProducts = hotProducts;
            ViewBag.LatestNews = latestNews;
            ViewBag.Categories = categories;

            return View("Index");
        }

        // Trang giới thiệu (public)
        public ActionResult About()
        {
            ViewBag.Message = "TechWorld - Website bán hàng công nghệ hàng đầu Việt Nam";
            return View();
        }

        // Trang liên hệ
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Contact(string name, string email, string phone, string message)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                ViewBag.Error = "Vui lòng điền đầy đủ thông tin";
                return View();
            }

            // giả lập gửi email
            System.Diagnostics.Debug.WriteLine($"Contact: {name} - {email} - {message}");

            ViewBag.SuccessMessage = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất.";

            return View();
        }

        // POST: Home/SendMessage (gửi tin nhắn đến admin)
        [Authorize]
        [HttpPost]
        public ActionResult SendMessage(string content)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung tin nhắn không được để trống" });
            }

            if (content.Length > 5000)
            {
                return Json(new { success = false, message = "Tin nhắn quá dài (tối đa 5000 ký tự)" });
            }

            int userId = (int)(Session["UserId"] ?? 0);

            try
            {
                var message = new Message
                {
                    UserId = userId,
                    Content = content,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };

                db.Messages.Add(message);
                db.SaveChanges();

                ViewBag.SuccessMessage = "Tin nhắn đã được gửi! Admin sẽ phản hồi trong 15 ngày.";

                // Auto-delete old messages (older than 15 days)
                var oldMessages = db.Messages
                    .Where(m => m.CreatedAt < DateTime.Now.AddDays(-15))
                    .ToList();

                if (oldMessages.Any())
                {
                    db.Messages.RemoveRange(oldMessages);
                    db.SaveChanges();
                }

                return View("Contact");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Home/GetNotifications
        [Authorize]
        public ActionResult GetNotifications()
        {
            int userId = (int)(Session["UserId"] ?? 0);

            try
            {
                // Lấy notifications từ Messages (tin nhắn từ Admin)
                var adminMessages = db.Messages
                    .Where(m => m.UserId == userId && !m.IsRead)
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(5)
                    .ToList();

                var notifications = adminMessages.Select(m => new
                {
                    title = "Tin nhắn từ Admin",
                    message = m.Content.Length > 100 ? m.Content.Substring(0, 100) + "..." : m.Content,
                    type = "info"
                }).ToList();

                // Nếu user là Admin, lấy unread messages từ users
                var userRole = Session["Role"]?.ToString();
                if (userRole == "Admin")
                {
                    var userMessages = db.Messages
                        .Where(m => !m.IsRead)
                        .OrderByDescending(m => m.CreatedAt)
                        .Take(3)
                        .ToList();

                    var adminNotifications = userMessages.Select(m => new
                    {
                        title = "Tin nhắn từ " + m.User.FullName,
                        message = m.Content.Length > 100 ? m.Content.Substring(0, 100) + "..." : m.Content,
                        type = "warning"
                    }).ToList();

                    notifications.AddRange(adminNotifications);
                }

                return Json(new { notifications = notifications }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { notifications = new object[0], error = ex.Message }, JsonRequestBehavior.AllowGet);
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