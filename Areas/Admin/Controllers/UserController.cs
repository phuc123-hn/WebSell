using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using web1.Models;
using web1.Data;

namespace web1.Areas.Admin.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/User
        public ActionResult Index()
        {
            if (Session["Role"] != null && Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var users = db.Users.ToList();
            return View(users);
        }

        // GET: Admin/User/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // GET: Admin/User/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            // Không cho xóa admin
            if (user.Role == "Admin")
            {
                TempData["Error"] = "Không thể xóa tài khoản Admin!";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // POST: Admin/User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = db.Users.Find(id);
            
            if (user == null)
                return HttpNotFound();

            if (user.Role == "Admin")
            {
                TempData["Error"] = "Không thể xóa tài khoản Admin!";
                return RedirectToAction("Index");
            }

            db.Users.Remove(user);
            db.SaveChanges();
            TempData["Success"] = "Xóa người dùng thành công!";
            return RedirectToAction("Index");
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
