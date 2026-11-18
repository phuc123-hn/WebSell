using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using web1.Data;
using web1.Models;

namespace web1.Areas.Admin.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Event
        public ActionResult Index()
        {
            if (Session["Role"] != null && Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var events = db.Events.Include(e => e.Vouchers).OrderByDescending(e => e.StartDate).ToList();
            return View(events);
        }

        // GET: Admin/Event/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Event eventModel)
        {
            if (ModelState.IsValid)
            {
                if (eventModel.EndDate <= eventModel.StartDate)
                {
                    ModelState.AddModelError("", "Ngày kết thúc phải sau ngày bắt đầu");
                    return View(eventModel);
                }

                db.Events.Add(eventModel);
                db.SaveChanges();

                TempData["Success"] = "Tạo sự kiện thành công!";
                return RedirectToAction("Index");
            }

            return View(eventModel);
        }

        // GET: Admin/Event/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var eventModel = db.Events.Find(id);
            if (eventModel == null)
                return HttpNotFound();

            return View(eventModel);
        }

        // POST: Admin/Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Event eventModel)
        {
            if (ModelState.IsValid)
            {
                if (eventModel.EndDate <= eventModel.StartDate)
                {
                    ModelState.AddModelError("", "Ngày kết thúc phải sau ngày bắt đầu");
                    return View(eventModel);
                }

                var existingEvent = db.Events.Find(eventModel.Id);
                if (existingEvent != null)
                {
                    existingEvent.Name = eventModel.Name;
                    existingEvent.Description = eventModel.Description;
                    existingEvent.StartDate = eventModel.StartDate;
                    existingEvent.EndDate = eventModel.EndDate;
                    existingEvent.ImageUrl = eventModel.ImageUrl;
                    existingEvent.IsActive = eventModel.IsActive;
                    existingEvent.UpdatedAt = DateTime.Now;

                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật sự kiện thành công!";
                    return RedirectToAction("Index");
                }
            }

            return View(eventModel);
        }

        // GET: Admin/Event/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var eventModel = db.Events.Find(id);
            if (eventModel == null)
                return HttpNotFound();

            return View(eventModel);
        }

        // POST: Admin/Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var eventModel = db.Events.Find(id);

            if (eventModel != null)
            {
                db.Events.Remove(eventModel);
                db.SaveChanges();
                TempData["Success"] = "Xóa sự kiện thành công!";
            }

            return RedirectToAction("Index");
        }

        // POST: Admin/Event/UploadImage
        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase file)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn file!" });
                }

                // Kiểm tra loại file
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                string fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Chỉ hỗ trợ JPG, PNG, GIF!" });
                }

                // Kiểm tra kích thước (5MB)
                if (file.ContentLength > 5 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "Kích thước phải nhỏ hơn 5MB!" });
                }

                // Tạo tên file duy nhất
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                fileName = fileName + "_" + DateTime.Now.Ticks.ToString() + fileExtension;

                // Tạo thư mục nếu chưa tồn tại
                string uploadFolder = Server.MapPath("~/Content/Event");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Lưu file
                string filePath = Path.Combine(uploadFolder, fileName);
                file.SaveAs(filePath);

                // Trả về đường dẫn tương đối
                string relativePath = "/Content/Event/" + fileName;

                return Json(new
                {
                    success = true,
                    message = "Upload thành công!",
                    filePath = relativePath,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/Event/Detail/5
        public ActionResult Detail(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var eventModel = db.Events.Include(e => e.Vouchers).FirstOrDefault(e => e.Id == id);
            if (eventModel == null)
                return HttpNotFound();

            ViewBag.QuestionCount = db.EventQuestions.Where(q => q.EventId == id).Count();
            ViewBag.VoucherCount = db.Vouchers.Where(v => v.EventId == id).Count();
            ViewBag.ParticipantCount = db.EventParticipants.Where(p => p.EventId == id).Count();

            return View(eventModel);
        }

        // POST: Admin/Event/CreateQuestion
        [HttpPost]
        public ActionResult CreateQuestion(int eventId, string question, string optionA, string optionB, string optionC, string optionD, string correctAnswer)
        {
            try
            {
                if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(correctAnswer))
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                if (!new[] { "A", "B", "C", "D" }.Contains(correctAnswer.ToUpper()))
                    return Json(new { success = false, message = "Đáp án đúng không hợp lệ" });

                var evt = db.Events.Find(eventId);
                if (evt == null)
                    return Json(new { success = false, message = "Sự kiện không tìm thấy" });

                var newQuestion = new EventQuestion
                {
                    EventId = eventId,
                    Question = question,
                    OptionA = optionA,
                    OptionB = optionB,
                    OptionC = optionC,
                    OptionD = optionD,
                    CorrectAnswer = correctAnswer.ToUpper()
                };

                db.EventQuestions.Add(newQuestion);
                db.SaveChanges();

                return Json(new { success = true, message = "Thêm câu hỏi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/Event/DeleteQuestion
        [HttpPost]
        public ActionResult DeleteQuestion(int id)
        {
            try
            {
                var question = db.EventQuestions.Find(id);
                if (question == null)
                    return Json(new { success = false, message = "Câu hỏi không tìm thấy" });

                db.EventQuestions.Remove(question);
                db.SaveChanges();

                return Json(new { success = true, message = "Xóa câu hỏi thành công!" });
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
