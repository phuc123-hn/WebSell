using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;

namespace web1.Areas.Admin.Controllers
{
    [Authorize]
    public class EventQuestionController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/EventQuestion/Index/5
        public ActionResult Index(int? eventId)
        {
            if (eventId == null)
                return RedirectToAction("Index", "Event");

            if (Session["Role"] != null && Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var evt = db.Events.Find(eventId);
            if (evt == null)
                return HttpNotFound();

            var questions = db.EventQuestions.Where(q => q.EventId == eventId).ToList();
            ViewBag.EventId = eventId;
            ViewBag.EventName = evt.Name;

            return View(questions);
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
