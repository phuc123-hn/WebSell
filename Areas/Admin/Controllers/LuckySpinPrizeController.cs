using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;
using System.Data.Entity;

namespace web1.Areas.Admin.Controllers
{
    [Authorize]
    public class LuckySpinPrizeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/LuckySpinPrize
        public ActionResult Index()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var prizes = db.LuckySpinPrizes.OrderBy(p => p.DisplayOrder).ToList();
            return View(prizes);
        }

        // GET: Admin/LuckySpinPrize/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var prize = db.LuckySpinPrizes.Find(id);
            if (prize == null)
                return HttpNotFound();

            ViewBag.Vouchers = new SelectList(db.Vouchers.Where(v => v.IsActive), "Id", "Name", 
                prize.PrizeType == "Voucher" ? prize.VoucherId : null);
            ViewBag.Products = new SelectList(db.Products, "Id", "Name", 
                prize.PrizeType == "Product" ? prize.ProductId : null);

            return View(prize);
        }

        // POST: Admin/LuckySpinPrize/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LuckySpinPrize prize)
        {
            if (ModelState.IsValid)
            {
                var existingPrize = db.LuckySpinPrizes.Find(prize.Id);
                if (existingPrize != null)
                {
                    existingPrize.Name = prize.Name;
                    existingPrize.Description = prize.Description;
                    existingPrize.WinRate = prize.WinRate;
                    existingPrize.PrizeType = prize.PrizeType;
                    existingPrize.VoucherId = prize.VoucherId;
                    existingPrize.ProductId = prize.ProductId;
                    existingPrize.DisplayOrder = prize.DisplayOrder;
                    existingPrize.IsActive = prize.IsActive;
                    existingPrize.UpdatedAt = DateTime.Now;

                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật giải thưởng thành công!";
                    return RedirectToAction("Index");
                }
            }

            ViewBag.Vouchers = new SelectList(db.Vouchers.Where(v => v.IsActive), "Id", "Name", prize.VoucherId);
            ViewBag.Products = new SelectList(db.Products, "Id", "Name", prize.ProductId);
            return View(prize);
        }

        // GET: Admin/LuckySpinPrize/History
        public ActionResult History()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var histories = db.LuckySpinHistories
                .Include("User")
                .Include("Prize")
                .Include("WonVoucher")
                .Include("WonProduct")
                .OrderByDescending(h => h.SpinDate)
                .ToList();

            return View(histories);
        }

        // GET: Admin/LuckySpinPrize/UserSpinDetail/:userId
        public ActionResult UserSpinDetail(int userId)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var user = db.Users.Find(userId);
            if (user == null)
                return HttpNotFound();

            var spinHistories = db.LuckySpinHistories
                .Where(h => h.UserId == userId)
                .Include("Prize")
                .Include("WonVoucher")
                .Include("WonProduct")
                .OrderByDescending(h => h.SpinDate)
                .ToList();

            // Tự động cấp voucher cho các lần quay voucher chưa có voucher
            var hasChanges = false;
            foreach (var history in spinHistories)
            {
                // Reload prize nếu null
                if (history.Prize == null)
                {
                    history.Prize = db.LuckySpinPrizes.Find(history.PrizeId);
                }

                if (history.Prize != null && history.Prize.PrizeType == "Voucher" && !history.WonVoucherId.HasValue)
                {
                    var baseVoucher = history.Prize.VoucherId.HasValue ? 
                        db.Vouchers.Find(history.Prize.VoucherId.Value) : null;

                    if (baseVoucher != null)
                    {
                        // Tạo private voucher cho user
                        var privateVoucher = new Voucher
                        {
                            Code = baseVoucher.Code + "_" + userId + "_" + DateTime.Now.Ticks,
                            Name = baseVoucher.Name,
                            Description = baseVoucher.Description + " (Từ vòng quay may mắn)",
                            DiscountPercent = baseVoucher.DiscountPercent,
                            DiscountAmount = baseVoucher.DiscountAmount,
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(15),
                            Quantity = 1,
                            MinOrderAmount = baseVoucher.MinOrderAmount,
                            IsActive = true,
                            IsLuckySpinVoucher = true,
                            IsPrivate = true,
                            OwnerUserId = userId,
                            CreatedAt = DateTime.Now
                        };

                        db.Vouchers.Add(privateVoucher);
                        db.SaveChanges();

                        // Update spin history
                        history.WonVoucherId = privateVoucher.Id;
                        history.Status = "Claimed";
                        history.ClaimedDate = DateTime.Now;
                        db.SaveChanges();
                        
                        hasChanges = true;
                    }
                }
            }

            // Reload lại data nếu có thay đổi
            if (hasChanges)
            {
                spinHistories = db.LuckySpinHistories
                    .Where(h => h.UserId == userId)
                    .Include("Prize")
                    .Include("WonVoucher")
                    .Include("WonProduct")
                    .OrderByDescending(h => h.SpinDate)
                    .ToList();
            }

            ViewBag.User = user;
            return View(spinHistories);
        }

        // POST: Admin/LuckySpinPrize/GrantVoucher
        [HttpPost]
        public ActionResult GrantVoucher(int spinHistoryId)
        {
            try
            {
                if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                {
                    return Json(new { success = false, message = "Không có quyền" });
                }

                var spinHistory = db.LuckySpinHistories.Find(spinHistoryId);
                if (spinHistory == null)
                    return Json(new { success = false, message = "Không tìm thấy lịch sử quay" });

                // Chỉ cấp voucher nếu giải là voucher và chưa được cấp
                if (spinHistory.Prize?.PrizeType != "Voucher" || spinHistory.WonVoucherId.HasValue)
                    return Json(new { success = false, message = "Không thể cấp voucher cho giải này" });

                var baseVoucher = spinHistory.Prize.VoucherId.HasValue ? 
                    db.Vouchers.Find(spinHistory.Prize.VoucherId.Value) : null;

                if (baseVoucher == null)
                    return Json(new { success = false, message = "Không tìm thấy voucher gốc" });

                // Tạo private voucher cho user
                var privateVoucher = new Voucher
                {
                    Code = baseVoucher.Code + "_" + spinHistory.UserId + "_" + DateTime.Now.Ticks,
                    Name = baseVoucher.Name,
                    Description = baseVoucher.Description + " (Từ vòng quay may mắn)",
                    DiscountPercent = baseVoucher.DiscountPercent,
                    DiscountAmount = baseVoucher.DiscountAmount,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(15),
                    Quantity = 1,
                    MinOrderAmount = baseVoucher.MinOrderAmount,
                    IsActive = true,
                    IsLuckySpinVoucher = true,
                    IsPrivate = true,
                    OwnerUserId = spinHistory.UserId,
                    CreatedAt = DateTime.Now
                };

                db.Vouchers.Add(privateVoucher);
                db.SaveChanges();

                // Update spin history
                spinHistory.WonVoucherId = privateVoucher.Id;
                spinHistory.Status = "Claimed";
                spinHistory.ClaimedDate = DateTime.Now;
                db.SaveChanges();

                TempData["Success"] = "Đã cấp voucher cho user";
                return Json(new { success = true, message = "Cấp voucher thành công" });
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
