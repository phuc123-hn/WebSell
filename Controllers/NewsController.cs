using System;
using System.Linq;
using System.Web.Mvc;
using web1.Models;
using web1.Data;
using System.Data.Entity;

namespace web1.Controllers
{
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: News
        public ActionResult Index(string category = null)
        {
            var news = db.News.Where(n => n.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                news = news.Where(n => n.Category == category);
            }

            news = news.OrderByDescending(n => n.PublishDate);

            return View(news.ToList());
        }

        // GET: News/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var newsItem = db.News.Find(id);
            if (newsItem == null || !newsItem.IsActive)
                return HttpNotFound();

            var relatedNews = db.News
                .Where(n => n.Id != id && n.IsActive && n.Category == newsItem.Category)
                .OrderByDescending(n => n.PublishDate)
                .Take(5)
                .ToList();

            ViewBag.RelatedNews = relatedNews;
            return View(newsItem);
        }

        // GET: News/GetLatestNews
        public ActionResult GetLatestNews(int count = 5)
        {
            var latestNews = db.News
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.PublishDate)
                .Take(count)
                .ToList();

            return PartialView("_NewsTicker", latestNews);
        }

        // GET: News/GetNewsByCategory
        public ActionResult GetNewsByCategory(string category, int count = 3)
        {
            var news = db.News
                .Where(n => n.IsActive && n.Category == category)
                .OrderByDescending(n => n.PublishDate)
                .Take(count)
                .ToList();

            return PartialView("_NewsList", news);
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