using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;
using web1.Services;

namespace web1.Controllers
{
    public class VoucherController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Voucher (Trang săn vé giảm giá)
        public ActionResult Index()
        {
            var now = DateTime.Now;

            // Lấy voucher còn hạn
            var activeVouchers = db.Vouchers
                .Where(v => v.IsActive && v.EndDate >= now && !v.RequireEventParticipation)
                .OrderByDescending(v => v.DiscountPercent)
                .ToList();

            return View(activeVouchers);
        }

        // GET: Voucher/Events (Trang sự kiện)
        public ActionResult Events()
        {
            var now = DateTime.Now;

            // Sự kiện đang diễn ra
            var ongoingEvents = db.Events
                .Include(e => e.Vouchers)
                .Where(e => e.IsActive && e.StartDate <= now && e.EndDate >= now)
                .OrderByDescending(e => e.StartDate)
                .ToList();

            // Sự kiện sắp tới
            var upcomingEvents = db.Events
                .Include(e => e.Vouchers)
                .Where(e => e.IsActive && e.StartDate > now)
                .OrderBy(e => e.StartDate)
                .ToList();

            // Sự kiện đã qua
            var pastEvents = db.Events
                .Include(e => e.Vouchers)
                .Where(e => e.EndDate < now)
                .OrderByDescending(e => e.EndDate)
                .ToList();

            ViewBag.OngoingEvents = ongoingEvents;
            ViewBag.UpcomingEvents = upcomingEvents;
            ViewBag.PastEvents = pastEvents;

            return View();
        }

        // GET: Voucher/EventDetail/5
        public ActionResult EventDetail(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var evt = db.Events.Include(e => e.Vouchers).FirstOrDefault(e => e.Id == id);
            if (evt == null)
                return HttpNotFound();

            ViewBag.Vouchers = db.Vouchers
                .Where(v => v.IsActive && v.EventId == id)
                .ToList();

            int userId = (int)(Session["UserId"] ?? 0);
            if (userId > 0)
            {
                var participant = db.EventParticipants.FirstOrDefault(p => p.EventId == id && p.UserId == userId);
                ViewBag.IsParticipant = participant != null;
                ViewBag.Participant = participant;
            }
            else
            {
                ViewBag.IsParticipant = false;
                ViewBag.Participant = null;
            }

            return View(evt);
        }

        // POST: Voucher/ParticipateEvent
        [HttpPost]
        public ActionResult ParticipateEvent(int eventId)
        {
            try
            {
                int userId = (int)(Session["UserId"] ?? 0);
                if (userId == 0)
                    return Json(new { success = false, message = "Vui lòng đăng nhập trước" });

                var evt = db.Events.Find(eventId);
                if (evt == null)
                    return Json(new { success = false, message = "Sự kiện không tìm thấy" });

                var existingParticipant = db.EventParticipants.FirstOrDefault(p => p.EventId == eventId && p.UserId == userId);

                if (existingParticipant != null)
                    return Json(new { success = false, message = "Bạn đã tham gia sự kiện này rồi" });

                var participant = new EventParticipant
                {
                    EventId = eventId,
                    UserId = userId,
                    Score = 0,
                    DiscountPercent = 0,
                    ParticipatedAt = DateTime.Now
                };

                db.EventParticipants.Add(participant);
                db.SaveChanges();

                // Lấy danh sách câu hỏi
                var questions = db.EventQuestions.Where(q => q.EventId == eventId).ToList();

                return Json(new { success = true, participantId = participant.Id, questionCount = questions.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Voucher/GetEventQuestions/5
        public ActionResult GetEventQuestions(int eventId)
        {
            try
            {
                int userId = (int)(Session["UserId"] ?? 0);
                if (userId == 0)
                    return Json(new { error = "Vui lòng đăng nhập" }, JsonRequestBehavior.AllowGet);

                var questions = db.EventQuestions
                    .Where(q => q.EventId == eventId)
                    .Select(q => new
                    {
                        q.Id,
                        q.Question,
                        q.OptionA,
                        q.OptionB,
                        q.OptionC,
                        q.OptionD
                    })
                    .ToList();

                return Json(questions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Voucher/SubmitQuiz
        [HttpPost]
        public ActionResult SubmitQuiz(int participantId, string answers)
        {
            try
            {
                int userId = (int)(Session["UserId"] ?? 0);
                if (userId == 0)
                    return Json(new { success = false, message = "Vui lòng đăng nhập trước" });

                var participant = db.EventParticipants.Find(participantId);
                if (participant == null)
                    return Json(new { success = false, message = "Tham gia không tìm thấy. Vui lòng tham gia sự kiện lại." });

                if (participant.UserId != userId)
                    return Json(new { success = false, message = "Bạn không có quyền nộp bài này" });

                // Parse answers (format: "1:A,2:B,3:C,...")
                var answerDict = answers.Split(',')
                    .Select(a => a.Split(':'))
                    .Where(a => a.Length == 2)
                    .ToDictionary(a => int.Parse(a[0]), a => a[1]);

                var questions = db.EventQuestions
                    .Where(q => q.EventId == participant.EventId)
                    .ToList();

                if (questions.Count == 0)
                    return Json(new { success = false, message = "Sự kiện này không có câu hỏi" });

                int correctCount = 0;
                foreach (var question in questions)
                {
                    if (answerDict.ContainsKey(question.Id) && answerDict[question.Id] == question.CorrectAnswer)
                    {
                        correctCount++;
                    }
                }

                // Tính điểm (0-10)
                int score = (correctCount * 10) / Math.Max(questions.Count, 1);

                // Tính % giảm dựa vào điểm
                decimal discountPercent = 0;
                if (score >= 7)
                    discountPercent = 25;
                else if (score >= 5)
                    discountPercent = 20;
                else if (score >= 1)
                    discountPercent = 15;

                participant.Score = score;
                participant.DiscountPercent = discountPercent;
                participant.CompletedAt = DateTime.Now;

                db.SaveChanges();

                // Cấp voucher cho user dựa vào event và score
                var eventVoucherService = new EventVoucherService();
                var grantedVouchers = eventVoucherService.GrantEventVouchersToUser(participant.EventId, userId, score);

                var voucherMessage = "";
                if (grantedVouchers.Count > 0)
                {
                    if (grantedVouchers.Count == 1)
                    {
                        var v = grantedVouchers[0];
                        voucherMessage = $" Bạn nhận được voucher: {v.Name} ({v.Code})";
                    }
                    else
                    {
                        voucherMessage = $" Bạn nhận được {grantedVouchers.Count} voucher!";
                    }
                }

                return Json(new
                {
                    success = true,
                    score = score,
                    correctCount = correctCount,
                    totalQuestions = questions.Count,
                    discountPercent = discountPercent,
                    voucherCount = grantedVouchers.Count,
                    message = $"Bạn đã trả lời đúng {correctCount}/{questions.Count} câu. Điểm: {score}/10.{voucherMessage}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Voucher/ValidateCode
        [HttpPost]
        public ActionResult ValidateCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá" });
            }

            var voucher = db.Vouchers.FirstOrDefault(v => v.Code.ToUpper() == code.ToUpper());

            if (voucher == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không tồn tại" });
            }

            var now = DateTime.Now;

            // Kiểm tra hạn sử dụng
            if (voucher.EndDate < now)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn" });
            }

            // Kiểm tra ngày bắt đầu
            if (voucher.StartDate > now)
            {
                return Json(new { success = false, message = "Mã giảm giá chưa được kích hoạt" });
            }

            // Kiểm tra số lượng
            if (voucher.UsedQuantity >= voucher.Quantity && voucher.Quantity > 0)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết" });
            }

            // Kiểm tra trạng thái
            if (!voucher.IsActive)
            {
                return Json(new { success = false, message = "Mã giảm giá không còn hiệu lực" });
            }

            return Json(new
            {
                success = true,
                message = "Mã giảm giá hợp lệ",
                voucher = new
                {
                    id = voucher.Id,
                    code = voucher.Code,
                    name = voucher.Name,
                    discountPercent = voucher.DiscountPercent,
                    discountAmount = voucher.DiscountAmount,
                    description = voucher.Description,
                    minOrderAmount = voucher.MinOrderAmount
                }
            });
        }

        // GET: Voucher/MyVouchers (Vé của người dùng)
        [Authorize]
        public ActionResult MyVouchers()
        {
            int userId = (int)(Session["UserId"] ?? 0);

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Lấy vé mà user đã sử dụng
            var usedVouchers = db.VoucherUsages
                .Where(vu => vu.UserId == userId)
                .Include(vu => vu.Voucher)
                .OrderByDescending(vu => vu.UsedAt)
                .ToList();

            return View(usedVouchers);
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