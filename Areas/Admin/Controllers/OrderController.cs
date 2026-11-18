using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using web1.Models;
using web1.Data;

namespace web1.Areas.Admin.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Order
        public ActionResult Index()
        {
            if (Session["Role"] != null && Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var orders = db.Orders.Include("User").OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        // GET: Admin/Order/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var order = db.Orders.Include("User").Include("OrderDetails.Product").FirstOrDefault(o => o.Id == id);
            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        // GET: Admin/Order/UpdateStatus/5
        public ActionResult UpdateStatus(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var order = db.Orders.Include("User").FirstOrDefault(o => o.Id == id);
            if (order == null)
                return HttpNotFound();

            ViewBag.StatusList = new SelectList(new[]
            {
                new { value = "Pending", text = "Chờ xử lý" },
                new { value = "Processing", text = "Đang xử lý" },
                new { value = "Shipped", text = "Đã gửi" },
                new { value = "Delivered", text = "Đã giao" },
                new { value = "Cancelled", text = "Đã hủy" }
            }, "value", "text", order.Status);

            return View(order);
        }

        // POST: Admin/Order/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = db.Orders.Find(id);
            if (order == null)
                return HttpNotFound();

            order.Status = status;
            db.SaveChanges();

            TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction("Details", new { id = id });
        }

        // POST: Admin/Order/MarkAsPaid/5
        [HttpPost]
        public ActionResult MarkAsPaid(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
                return HttpNotFound();

            order.IsPaid = true;
            db.SaveChanges();

            TempData["Success"] = "Đơn hàng đã được đánh dấu là đã thanh toán!";
            return RedirectToAction("Details", new { id = id });
        }

        // GET: Admin/Order/Cancel/5
        public ActionResult Cancel(int? id)
        {
            if (id == null)
                return HttpNotFound();

            var order = db.Orders.Include("User").Include("OrderDetails.Product").FirstOrDefault(o => o.Id == id);
            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        // POST: Admin/Order/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public ActionResult CancelConfirmed(int id)
        {
            var order = db.Orders.Include("OrderDetails").FirstOrDefault(o => o.Id == id);
            
            if (order == null)
                return HttpNotFound();

            if (order.Status == "Delivered" || order.Status == "Cancelled")
            {
                TempData["Error"] = "Không thể hủy đơn hàng đã giao hoặc đã bị hủy!";
                return RedirectToAction("Details", new { id = id });
            }

            order.Status = "Cancelled";

            // Hoàn lại số lượng sản phẩm
            foreach (var detail in order.OrderDetails)
            {
                var product = db.Products.Find(detail.ProductId);
                if (product != null)
                {
                    product.Stock += detail.Quantity;
                }
            }

            db.SaveChanges();
            TempData["Success"] = "Đơn hàng đã được hủy!";
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
