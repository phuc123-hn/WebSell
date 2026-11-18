using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using web1.Models;
using web1.Data;
using web1.Services;

namespace web1.Controllers
{
    public class CartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private VoucherService voucherService = new VoucherService();

        private const string CartSessionKey = "CartItems";

        private List<CartItem> GetCartItems()
        {
            if (Session[CartSessionKey] == null)
            {
                Session[CartSessionKey] = new List<CartItem>();
            }
            return (List<CartItem>)Session[CartSessionKey];
        }

        // GET: Cart
        public ActionResult Index()
        {
            var cartItems = GetCartItems();
            return View(cartItems);
        }

        // POST: Cart/Add
        [HttpPost]
        public ActionResult Add(int productId, int quantity = 1)
        {
            var product = db.Products.Find(productId);
            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không tìm thấy" });

            var cartItems = GetCartItems();
            var existingItem = cartItems.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            Session[CartSessionKey] = cartItems;
            return Json(new { success = true, message = "Đã thêm vào giỏ hàng", cartCount = cartItems.Sum(c => c.Quantity) });
        }

        // POST: Cart/Update
        [HttpPost]
        public ActionResult Update(int productId, int quantity)
        {
            var cartItems = GetCartItems();
            var item = cartItems.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cartItems.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                Session[CartSessionKey] = cartItems;
            }

            return Json(new { success = true, cartCount = cartItems.Sum(c => c.Quantity) });
        }

        // POST: Cart/Remove
        [HttpPost]
        public ActionResult Remove(int productId)
        {
            var cartItems = GetCartItems();
            var item = cartItems.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                cartItems.Remove(item);
                Session[CartSessionKey] = cartItems;
            }

            return Json(new { success = true, cartCount = cartItems.Sum(c => c.Quantity) });
        }

        // GET: Cart/GetCartCount
        public ActionResult GetCartCount()
        {
            var cartItems = GetCartItems();
            return Json(new { count = cartItems.Sum(c => c.Quantity) }, JsonRequestBehavior.AllowGet);
        }

        // GET: Cart/Checkout
        [Authorize]
        public ActionResult Checkout()
        {
            var cartItems = GetCartItems();
            if (cartItems.Count == 0)
            {
                return RedirectToAction("Index");
            }

            ViewBag.CartItems = cartItems;
            ViewBag.Total = cartItems.Sum(c => c.TotalPrice);
            return View();
        }

        // ✅ POST: Cart/PlaceOrder (ĐÃ SỬA: LƯU VÀO SESSION)
        [HttpPost]
        [Authorize]
        public ActionResult PlaceOrder(string shippingName, string shippingPhone, string shippingAddress, string paymentMethod, string voucherCode = "")
        {
            var cartItems = GetCartItems();
            if (cartItems.Count == 0)
                return Json(new { success = false, message = "Giỏ hàng trống" });

            int userId = 0;
            if (Session["UserId"] != null)
                int.TryParse(Session["UserId"].ToString(), out userId);

            if (userId == 0)
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn, vui lòng đăng nhập lại" });

            // ✅ Kiểm tra tồn kho trước khi đưa vào Session
            foreach (var item in cartItems)
            {
                var product = db.Products.Find(item.ProductId);
                if (product == null)
                    return Json(new { success = false, message = $"Sản phẩm '{item.ProductName}' không tồn tại" });

                if (product.Stock < item.Quantity)
                    return Json(new { success = false, message = $"Sản phẩm '{item.ProductName}' chỉ còn {product.Stock} trong kho" });
            }

            // ✅ Validate input
            if (string.IsNullOrEmpty(shippingName) || shippingName.Length > 100)
                return Json(new { success = false, message = "Tên người nhận không hợp lệ" });

            if (string.IsNullOrEmpty(shippingPhone) || shippingPhone.Length > 20)
                return Json(new { success = false, message = "Số điện thoại không hợp lệ" });

            if (string.IsNullOrEmpty(shippingAddress) || shippingAddress.Length > 500)
                return Json(new { success = false, message = "Địa chỉ không hợp lệ" });

            decimal subtotal = cartItems.Sum(c => c.TotalPrice);
            decimal discount = 0;
            decimal shipping = 30000;

            if (Session["VoucherDiscount"] != null)
                decimal.TryParse(Session["VoucherDiscount"].ToString(), out discount);

            decimal totalAmount = subtotal - discount + shipping;

            // ✅ LƯU VÀO SESSION THAY VÌ DB
            var pendingOrder = new
            {
                UserId = userId,
                ShippingName = shippingName.Trim(),
                ShippingPhone = shippingPhone.Trim(),
                ShippingAddress = shippingAddress.Trim(),
                PaymentMethod = paymentMethod,
                Subtotal = subtotal,
                TotalVoucherDiscount = discount,
                TotalAmount = totalAmount,
                VoucherCode = voucherCode,
                CartItems = cartItems.Select(i => new {
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    i.Price,
                    i.TotalPrice
                }).ToList()
            };

            // Chuyển đối tượng sang JSON string để lưu vào Session
            Session["PendingOrder"] = Newtonsoft.Json.JsonConvert.SerializeObject(pendingOrder);

            // ✅ Trả về orderId = 0 vì chưa tạo trong Database
            return Json(new
            {
                success = true,
                message = "Vui lòng xác nhận OTP để hoàn tất đơn hàng",
                orderId = 0
            });
        }

        // POST: Cart/ApplyVoucher
        [HttpPost]
        public ActionResult ApplyVoucher(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                    return Json(new { success = false, message = "Mã voucher không được để trống" });

                int userId = 0;
                if (Session["UserId"] != null)
                    int.TryParse(Session["UserId"].ToString(), out userId);

                if (userId == 0)
                    return Json(new { success = false, message = "Vui lòng đăng nhập để áp dụng voucher" });

                var cartItems = GetCartItems();
                decimal subtotal = cartItems.Sum(c => c.TotalPrice);

                var validationResult = voucherService.ValidateVoucher(code, userId, subtotal);

                if (!validationResult.IsValid)
                    return Json(new { success = false, message = validationResult.Message });

                var voucher = validationResult.Voucher;
                if (voucher.RequireEventParticipation && voucher.EventId.HasValue)
                {
                    var participant = db.EventParticipants.FirstOrDefault(p => p.EventId == voucher.EventId && p.UserId == userId);

                    if (participant == null)
                        return Json(new { success = false, message = "Bạn cần tham gia sự kiện để sử dụng voucher này" });
                }

                Session["AppliedVoucher"] = code.ToUpper();
                Session["VoucherDiscount"] = validationResult.DiscountAmount;

                return Json(new { success = true, discountAmount = validationResult.DiscountAmount, message = validationResult.Message });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ApplyVoucher Error: " + ex.ToString());
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Cart/GetUserVouchers
        [HttpGet]
        [Authorize]
        public ActionResult GetUserVouchers()
        {
            try
            {
                int userId = 0;
                if (Session["UserId"] != null)
                    int.TryParse(Session["UserId"].ToString(), out userId);

                if (userId == 0)
                    return Json(new { success = false, message = "Vui lòng đăng nhập", vouchers = new List<object>() }, JsonRequestBehavior.AllowGet);

                var cartItems = GetCartItems();
                decimal subtotal = cartItems.Sum(c => c.TotalPrice);

                var userVouchers = db.Vouchers.Where(v =>
                    v.IsActive &&
                    v.IsPrivate &&
                    v.OwnerUserId == userId &&
                    v.StartDate <= DateTime.Now &&
                    v.EndDate > DateTime.Now
                ).ToList();

                var publicVouchers = db.Vouchers.Where(v =>
                    v.IsActive &&
                    !v.IsPrivate &&
                    v.StartDate <= DateTime.Now &&
                    v.EndDate > DateTime.Now &&
                    (v.Quantity == 0 || v.UsedQuantity < v.Quantity) &&
                    v.MinOrderAmount <= subtotal &&
                    !db.VoucherUsages.Any(vu => vu.VoucherId == v.Id && vu.UserId == userId)
                ).ToList();

                var allVouchers = userVouchers.Union(publicVouchers)
                    .OrderBy(v => v.EndDate)
                    .ToList();

                var voucherList = allVouchers.Select(v => new
                {
                    v.Id,
                    v.Code,
                    v.Name,
                    v.Description,
                    DiscountDisplay = v.DiscountPercent > 0
                        ? $"-{v.DiscountPercent}%"
                        : $"-{v.DiscountAmount.ToString("N0")}đ",
                    EndDate = v.EndDate.ToString("dd/MM/yyyy HH:mm"),
                    DaysLeft = (v.EndDate - DateTime.Now).TotalDays,
                    IsPrivate = v.IsPrivate,
                    MinOrderAmount = v.MinOrderAmount.ToString("N0")
                }).ToList();

                return Json(new { success = true, vouchers = voucherList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetUserVouchers Error: " + ex.ToString());
                return Json(new { success = false, message = "Lỗi: " + ex.Message, vouchers = new List<object>() }, JsonRequestBehavior.AllowGet);
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