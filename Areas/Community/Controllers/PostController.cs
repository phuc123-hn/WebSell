using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;

namespace web1.Areas.Community.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private static DateTime lastPostTime = DateTime.MinValue;
        private const int COOLDOWN_SECONDS = 30;

        // GET: Community/Post/Create
        public ActionResult Create(int type = 0)
        {
            // Type: 0 = Review, 1 = Rent
            // TẤT CẢ users đều được đăng cả Review và Rent (loại bỏ hạn chế Admin)
            ViewBag.PostType = type;
            return View();
        }

        // POST: Community/Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Post model)
        {
            int userId = (int)(Session["UserId"] ?? 0);

            if (userId == 0)
                return RedirectToAction("Login", "Account", new { area = "" });

            // Kiểm tra cooldown (chống spam)
            if ((DateTime.Now - lastPostTime).TotalSeconds < COOLDOWN_SECONDS)
            {
                ModelState.AddModelError("", $"Vui lòng chờ {COOLDOWN_SECONDS} giây trước khi đăng bài tiếp theo");
                ViewBag.PostType = model.Type;
                return View(model);
            }

            // Kiểm tra rating (chỉ cho Review - Type 0)
            if (model.Type == 0 && (!model.Rating.HasValue || model.Rating < 1 || model.Rating > 5))
            {
                ModelState.AddModelError("Rating", "Đánh giá phải từ 1 đến 5 sao");
                ViewBag.PostType = model.Type;
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PostType = model.Type;
                return View(model);
            }

            try
            {
                model.UserId = userId;
                model.CreatedAt = DateTime.Now;
                model.IsDeleted = false;

                db.Posts.Add(model);
                db.SaveChanges();

                lastPostTime = DateTime.Now;

                return RedirectToAction("Detail", new { id = model.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tạo bài: " + ex.Message);
                ViewBag.PostType = model.Type;
                return View(model);
            }
        }

        // GET: Community/Post/Detail/5
        public ActionResult Detail(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var post = db.Posts
                .Include("User")
                .Include("Comments.User")
                .Include("Likes")
                .FirstOrDefault(p => p.Id == id.Value);

            if (post == null || post.IsDeleted)
                return HttpNotFound();

            return View(post);
        }

        // GET: Community/Post/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var post = db.Posts
                .Include("User")
                .FirstOrDefault(p => p.Id == id.Value);

            if (post == null || post.IsDeleted)
                return HttpNotFound();

            int userId = (int)(Session["UserId"] ?? 0);

            // Only author or admin can edit
            if (post.UserId != userId && !User.IsInRole("Admin"))
                return new HttpUnauthorizedResult();

            ViewBag.PostType = post.Type;
            return View(post);
        }

        // POST: Community/Post/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(Post model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PostType = model.Type;
                return View(model);
            }

            int userId = (int)(Session["UserId"] ?? 0);
            var existingPost = db.Posts.Find(model.Id);

            if (existingPost == null || existingPost.IsDeleted)
                return HttpNotFound();

            // Chỉ chính chủ hoặc admin mới được edit
            if (existingPost.UserId != userId && !User.IsInRole("Admin"))
                return new HttpUnauthorizedResult();

            try
            {
                existingPost.Title = model.Title;
                existingPost.Content = model.Content;
                existingPost.Rating = model.Rating;
                existingPost.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                return RedirectToAction("Detail", new { id = existingPost.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật bài: " + ex.Message);
                ViewBag.PostType = model.Type;
                return View(model);
            }
        }

        // POST: Community/Post/Delete/5
        [HttpPost]
        [Authorize]
        public ActionResult Delete(int id)
        {
            var post = db.Posts.Find(id);

            if (post == null || post.IsDeleted)
                return HttpNotFound();

            int userId = (int)(Session["UserId"] ?? 0);

            // Chỉ chính chủ hoặc admin mới được xóa
            if (post.UserId != userId && !User.IsInRole("Admin"))
                return new HttpUnauthorizedResult();

            try
            {
                // Soft delete
                post.IsDeleted = true;
                db.SaveChanges();

                return Json(new { success = true, message = "Bài đã được xóa" });
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