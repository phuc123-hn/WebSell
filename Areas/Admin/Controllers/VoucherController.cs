using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;

namespace web1.Areas.Admin.Controllers
{
    [Authorize]
    public class VoucherController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Voucher
        public ActionResult Index()
        {
            if (Session["Role"] != null && Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Vouchers S? Ki?n - có EventId
            var eventVouchers = db.Vouchers
                .Include(v => v.Event)
                .Where(v => v.EventId.HasValue)
                .OrderByDescending(v => v.CreatedAt)
                .ToList();

            // Vouchers Chung - không có EventId và không private
            var publicVouchers = db.Vouchers
                .Include(v => v.Event)
                .Where(v => !v.EventId.HasValue && !v.IsPrivate)
                .OrderByDescending(v => v.CreatedAt)
                .ToList();

            // Vouchers Sprint - private (user s? h?u)
            var sprintVouchers = db.Vouchers
                .Include(v => v.OwnerUser)
                .Where(v => v.IsPrivate)
                .OrderByDescending(v => v.CreatedAt)
                .ToList();

            ViewBag.EventVouchers = eventVouchers;
            ViewBag.PublicVouchers = publicVouchers;
            ViewBag.SprintVouchers = sprintVouchers;

            return View();
        }

        // GET: Admin/Voucher/Create
        public ActionResult Create()
        {
            ViewBag.Events = new SelectList(db.Events.Where(e => e.IsActive), "Id", "Name");
            return View();
        }

        // POST: Admin/Voucher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Voucher voucher)
        {
            // Xóa validation error cho DiscountAmount n?u không c?n thi?t
            if (ModelState.ContainsKey("DiscountAmount"))
            {
                if (voucher.DiscountAmount == 0 && voucher.DiscountPercent > 0)
                {
                    ModelState.Remove("DiscountAmount");
                }
            }

            if (ModelState.IsValid)
            {
                // Ki?m tra code trùng
                if (db.Vouchers.Any(v => v.Code == voucher.Code))
                {
                    ModelState.AddModelError("", "Mã voucher ?ã t?n t?i");
                    ViewBag.Events = new SelectList(db.Events.Where(e => e.IsActive), "Id", "Name");
                    return View(voucher);
                }

                if (voucher.EndDate <= voucher.StartDate)
                {
                    ModelState.AddModelError("", "Ngày k?t thúc ph?i sau ngày b?t ??u");
                    ViewBag.Events = new SelectList(db.Events.Where(e => e.IsActive), "Id", "Name");
                    return View(voucher);
                }

                voucher.Code = voucher.Code.ToUpper();
                voucher.UsedQuantity = 0;

                db.Vouchers.Add(voucher);
                db.SaveChanges();

                TempData["Success"] = "T?o voucher thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.Events = new SelectList(db.Events.Where(e => e.IsActive), "Id", "Name");
            return View(voucher);
        }

        // GET: Admin/Voucher/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var voucher = db.Vouchers.Find(id);
            if (voucher == null)
                return HttpNotFound();

            ViewBag.Events = new SelectList(db.Events.Where(e => e.IsActive), "Id", "Name", voucher.EventId);
            return View(voucher);
        }

        // POST: Admin/Voucher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                if (voucher.EndDate <= voucher.StartDate)
                {
                    ModelState.AddModelError("", "Ngày k?t thúc ph?i sau ngày b?t ??u");
                    ViewBag.Events = new SelectList(db.Events.Where(e => e.IsActive), "Id", "Name", voucher.EventId);
                    return View(voucher);
                }

                var existingVoucher = db.Vouchers.Find(voucher.Id);
                if (existingVoucher != null)
                {
                    existingVoucher.Name = voucher.Name;
                    existingVoucher.Description = voucher.Description;
                    existingVoucher.DiscountPercent = voucher.DiscountPercent;
                    existingVoucher.DiscountAmount = voucher.DiscountAmount;
                    existingVoucher.StartDate = voucher.StartDate;
                    existingVoucher.EndDate = voucher.EndDate;
                    existingVoucher.Quantity = voucher.Quantity;
                    existingVoucher.MinOrderAmount = voucher.MinOrderAmount;
                    existingVoucher.EventId = voucher.EventId;
                    existingVoucher.IsActive = voucher.IsActive;
                    existingVoucher.UpdatedAt = DateTime.Now;

                    db.SaveChanges();
                    TempData["Success"] = "C?p nh?t voucher thành công!";
                    return RedirectToAction("Index");
                }
            }

            ViewBag.Events = new SelectList(db.Events.Where(e => e.IsActive), "Id", "Name", voucher.EventId);
            return View(voucher);
        }

        // GET: Admin/Voucher/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var voucher = db.Vouchers.Include(v => v.Event).FirstOrDefault(v => v.Id == id);
            if (voucher == null)
                return HttpNotFound();

            // L?y l?ch s? s? d?ng
            var usageHistory = db.VoucherUsages.Where(vu => vu.VoucherId == id)
                                              .Include(vu => vu.User)
                                              .Include(vu => vu.Order)
                                              .OrderByDescending(vu => vu.UsedAt)
                                              .ToList();

            ViewBag.UsageHistory = usageHistory;
            return View(voucher);
        }

        // GET: Admin/Voucher/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var voucher = db.Vouchers.Find(id);
            if (voucher == null)
                return HttpNotFound();

            return View(voucher);
        }

        // POST: Admin/Voucher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var voucher = db.Vouchers.Find(id);

            if (voucher != null)
            {
                db.Vouchers.Remove(voucher);
                db.SaveChanges();
                TempData["Success"] = "Xóa voucher thành công!";
            }

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
