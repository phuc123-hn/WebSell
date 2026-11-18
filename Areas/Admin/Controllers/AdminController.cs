using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;

namespace web1.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Admin/Messages
        public ActionResult Messages(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 20;

            // Auto-delete messages older than 15 days
            var oldMessages = db.Messages
                .Where(m => m.CreatedAt < DateTime.Now.AddDays(-15))
                .ToList();

            if (oldMessages.Any())
            {
                db.Messages.RemoveRange(oldMessages);
                db.SaveChanges();
            }

            // Get messages with pagination
            var messages = db.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalMessages = db.Messages.Count();
            ViewBag.TotalPages = Math.Ceiling((double)ViewBag.TotalMessages / pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.UnreadCount = db.Messages.Count(m => !m.IsRead);

            return View(messages);
        }

        // POST: Admin/Admin/MarkAsRead/5
        [HttpPost]
        public ActionResult MarkAsRead(int id)
        {
            var message = db.Messages.Find(id);
            
            if (message == null)
                return Json(new { success = false, message = "Tin nh?n không t?n t?i" });

            try
            {
                message.IsRead = true;
                db.SaveChanges();

                return Json(new { success = true, message = "?ã ?ánh d?u ?ã ??c" });
            }
            catch
            {
                return Json(new { success = false, message = "L?i" });
            }
        }

        // POST: Admin/Admin/DeleteMessage/5
        [HttpPost]
        public ActionResult DeleteMessage(int id)
        {
            var message = db.Messages.Find(id);

            if (message == null)
                return Json(new { success = false, message = "Tin nh?n không t?n t?i" });

            try
            {
                db.Messages.Remove(message);
                db.SaveChanges();

                return Json(new { success = true, message = "Tin nh?n ?ã ???c xóa" });
            }
            catch
            {
                return Json(new { success = false, message = "L?i" });
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
