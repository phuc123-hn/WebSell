using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Models;

namespace web1.Controllers
{
    [Authorize]
    public class AccountProfileController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AccountProfile
        public ActionResult MyAccount()
        {
            int userId = (int)(Session["UserId"] ?? 0);
            
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var user = db.Users.Find(userId);
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // GET: AccountProfile/MyOrders
        public ActionResult MyOrders()
        {
            int userId = (int)(Session["UserId"] ?? 0);

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var user = db.Users.Find(userId);
            if (user == null)
                return HttpNotFound();

            var orders = db.Orders.Where(o => o.UserId == userId)
                                 .OrderByDescending(o => o.OrderDate)
                                 .ToList();

            return View(orders);
        }

        // GET: AccountProfile/OrderDetails/5
        public ActionResult OrderDetails(int? id)
        {
            if (id == null)
                return HttpNotFound();

            int userId = (int)(Session["UserId"] ?? 0);
            var order = db.Orders
                .Include("OrderDetails.Product")
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return HttpNotFound();

            return View(order);
        }

        // GET: AccountProfile/MyVouchers
        public ActionResult MyVouchers()
        {
            int userId = (int)(Session["UserId"] ?? 0);

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Lấy tất cả voucher của user từ các nguồn:
            // 1. Private vouchers (từ lucky spin)
            // 2. Vouchers từ event (RequireEventParticipation = true và user đã tham gia)
            // 3. Vouchers được admin cấp trực tiếp (IsPrivate = true && OwnerUserId = userId)

            var myVouchers = new List<VoucherViewModel>();

            // 1. Private vouchers từ lucky spin
            var privateVouchers = db.Vouchers
                .Where(v => v.OwnerUserId == userId && v.IsPrivate)
                .ToList();
            
            foreach (var v in privateVouchers)
            {
                var isUsed = db.VoucherUsages.Any(vu => vu.VoucherId == v.Id && vu.UserId == userId);
                myVouchers.Add(new VoucherViewModel { Voucher = v, IsUsed = isUsed });
            }

            // 2. Vouchers từ event mà user đã tham gia
            var userEvents = db.EventParticipants
                .Where(ep => ep.UserId == userId)
                .Select(ep => ep.EventId)
                .ToList();

            if (userEvents.Any())
            {
                var eventVouchers = db.Vouchers
                    .Where(v => v.EventId.HasValue && userEvents.Contains(v.EventId.Value) && v.IsActive)
                    .ToList();
                
                foreach (var v in eventVouchers)
                {
                    if (!myVouchers.Any(vm => vm.Voucher.Id == v.Id))
                    {
                        var isUsed = db.VoucherUsages.Any(vu => vu.VoucherId == v.Id && vu.UserId == userId);
                        myVouchers.Add(new VoucherViewModel { Voucher = v, IsUsed = isUsed });
                    }
                }
            }

            // 3. Vouchers public được admin tạo (không require event, không private)
            var publicVouchers = db.Vouchers
                .Where(v => !v.IsPrivate && !v.EventId.HasValue && !v.IsLuckySpinVoucher && v.IsActive)
                .ToList();
            
            foreach (var v in publicVouchers)
            {
                if (!myVouchers.Any(vm => vm.Voucher.Id == v.Id))
                {
                    var isUsed = db.VoucherUsages.Any(vu => vu.VoucherId == v.Id && vu.UserId == userId);
                    myVouchers.Add(new VoucherViewModel { Voucher = v, IsUsed = isUsed });
                }
            }

            // Loại bỏ duplicates và sắp xếp
            myVouchers = myVouchers
                .GroupBy(vm => vm.Voucher.Id)
                .Select(g => g.First())
                .OrderByDescending(vm => vm.Voucher.CreatedAt)
                .ToList();

            return View(myVouchers);
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
