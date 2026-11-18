using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;
using web1.Services;
using System.Data.Entity;

namespace web1.Controllers
{
    [Authorize]
    public class LuckySpinController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private LuckySpinService luckySpinService = new LuckySpinService();

        // GET: LuckySpin/History
        public ActionResult History()
        {
            int userId = (int)(Session["UserId"] ?? 0);
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var spinHistories = db.LuckySpinHistories
                .Where(h => h.UserId == userId)
                .Include("Prize")
                .Include("WonVoucher")
                .Include("WonProduct")
                .OrderByDescending(h => h.SpinDate)
                .ToList();

            return View(spinHistories);
        }

        // GET: LuckySpin/Index
        [Authorize]
        public ActionResult Index()
        {
            int userId = (int)(Session["UserId"] ?? 0);
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var user = db.Users.Find(userId);
            ViewBag.SpinCount = user?.SpinCount ?? 0;
            return View();
        }

        // POST: LuckySpin/Spin
        [HttpPost]
        public ActionResult Spin()
        {
            try
            {
                int userId = (int)(Session["UserId"] ?? 0);
                if (userId == 0)
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

                var user = db.Users.Find(userId);
                if (user == null || user.SpinCount <= 0)
                    return Json(new { success = false, message = "Bạn không có lượt quay nào" });

                // Trừ 1 spin
                user.SpinCount -= 1;
                db.SaveChanges();

                // Thực hiện quay — dùng orderId = 0 vì không còn cần
                var spinResult = luckySpinService.SpinLucky(userId, 0);
                if (spinResult == null)
                {
                    // Hoàn lại spin nếu lỗi
                    user.SpinCount += 1;
                    db.SaveChanges();
                    return Json(new { success = false, message = "Lỗi khi quay" });
                }

                return Json(new {
                    success = true,
                    spinHistoryId = spinResult.Id,
                    prizeId = spinResult.PrizeId,
                    prizeName = spinResult.Prize?.Name ?? "Không xác định",
                    prizeType = spinResult.Prize?.PrizeType ?? "",
                    wonVoucherCode = spinResult.WonVoucher?.Code,
                    wonProductName = spinResult.WonProduct?.Name,
                    spinCountLeft = user.SpinCount,
                    message = $"Bạn đã trúng: {spinResult.Prize?.Name}"
                });
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errors = string.Join(" | ", ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                
                var errorMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Validation Error: {errors}\r\n{ex.StackTrace}";
                LogError(errorMsg);
                
                return Json(new { success = false, message = "Validation: " + errors });
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                // Lấy inner exception chi tiết nhất
                var inner = ex.InnerException?.InnerException?.Message 
                         ?? ex.InnerException?.Message 
                         ?? ex.Message;
                
                var errorMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DB Update Error: {inner}\r\n{ex.StackTrace}";
                LogError(errorMsg);
                
                return Json(new { success = false, message = "DB Error: " + inner });
            }
            catch (Exception ex)
            {
                var errorMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] General Error: {ex.Message}\r\n{ex.StackTrace}";
                LogError(errorMsg);
                
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private void LogError(string message)
        {
            try
            {
                string logPath = System.AppDomain.CurrentDomain.BaseDirectory + "logs\\controller_error.log";
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));
                System.IO.File.AppendAllText(logPath, message + "\r\n---\r\n");
            }
            catch { }
        }

        // POST: LuckySpin/ClaimVoucher
        [HttpPost]
        public ActionResult ClaimVoucher(int spinHistoryId)
        {
            try
            {
                int userId = (int)(Session["UserId"] ?? 0);
                if (userId == 0)
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

                var result = luckySpinService.ClaimVoucher(spinHistoryId, userId);

                if (result)
                {
                    return Json(new { success = true, message = "Voucher đã được thêm vào tài khoản" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể nhận voucher" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: LuckySpin/GetSpinCount
        [HttpGet]
        public ActionResult GetSpinCount()
        {
            int userId = (int)(Session["UserId"] ?? 0);
            if (userId == 0)
                return Json(new { spinCount = 0 }, JsonRequestBehavior.AllowGet);

            var user = db.Users.Find(userId);
            return Json(new { spinCount = user?.SpinCount ?? 0 }, JsonRequestBehavior.AllowGet);
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
