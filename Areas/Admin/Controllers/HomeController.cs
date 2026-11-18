using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using web1.Models;
using web1.Data;

namespace web1.Areas.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Home
        public ActionResult Index()
        {
            // Kiểm tra quyền admin
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalUsers = db.Users.Count();
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalNews = db.News.Count();
            ViewBag.CommunityPosts = db.Posts.Count(p => !p.IsDeleted);

            ViewBag.TotalRevenue =
                db.Orders
                  .Where(o => o.Status == "Delivered")
                  .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            // Category Statistics
            var categoryStats = db.Categories
                .Select(c => db.Products.Count(p => p.CategoryId == c.Id))
                .ToList();
            ViewBag.CategoryStats = categoryStats;

            // Order Status Statistics
            var orderStats = new int[]
            {
                db.Orders.Count(o => o.Status == "Pending"),
                db.Orders.Count(o => o.Status == "Processing"),
                db.Orders.Count(o => o.Status == "Delivered"),
                db.Orders.Count(o => o.Status == "Cancelled")
            };
            ViewBag.OrderStats = orderStats;

            // Revenue Statistics (7 ngày gần nhất)
            var revenueStats = new decimal[7];
            var revenueLabels = new string[7];

            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i);
                int index = 6 - i;
                revenueStats[index] = db.Orders
                    .Where(o => o.Status == "Delivered" && 
                                DbFunctions.TruncateTime(o.OrderDate) == DbFunctions.TruncateTime(date))
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                
                revenueLabels[index] = date.ToString("dd/MM");
            }
            ViewBag.RevenueStats = revenueStats;
            ViewBag.RevenueLabels = revenueLabels;

            return View();
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