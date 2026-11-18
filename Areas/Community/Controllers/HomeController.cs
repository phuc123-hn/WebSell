using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;

namespace web1.Areas.Community.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Community/Home/Index (T?t c? bài)
        public ActionResult Index(int? type)
        {
            var posts = db.Posts
                .Include(p => p.User)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            // Filter theo type n?u có
            if (type.HasValue)
            {
                posts = posts.Where(p => p.Type == type.Value).ToList();
            }

            ViewBag.CurrentType = type;
            return View(posts);
        }

        // GET: Community/Home/Rentals (Thuê ?? - Type = 1)
        public ActionResult Rentals()
        {
            var rentals = db.Posts
                .Include(p => p.User)
                .Where(p => p.Type == 1 && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View("Index", rentals);
        }

        // GET: Community/Home/Reviews (?ánh Giá - Type = 0)
        public ActionResult Reviews()
        {
            var reviews = db.Posts
                .Include(p => p.User)
                .Where(p => p.Type == 0 && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View("Index", reviews);
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
