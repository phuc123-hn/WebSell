using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Infrastructure.Logging;
using web1.Infrastructure.Filters;
using web1.Services;
using web1.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace web1.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private OtpService _otpService;
        private IEmailQueueService _emailQueueService;
        private FileLogger _logger = new FileLogger();

        public PaymentController()
        {
            _otpService = new OtpService(db);
            _emailQueueService = new EmailQueueService(new EmailService());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SendOtpPayment(int orderId)
        {
            int userId = (int)(Session["UserId"] ?? 0);
            if (userId == 0) return Json(new { success = false, message = "Vui lòng đăng nhập." });

            // Kiểm tra xem có đơn hàng chờ trong Session không
            if (string.IsNullOrEmpty(Session["PendingOrder"] as string))
                return Json(new { success = false, message = "Không tìm thấy thông tin đặt hàng." });

            var user = db.Users.Find(userId);
            string otpCode = _otpService.GenerateOTP();
            _otpService.SaveOTP(user.Email, otpCode, "Payment", userId);

            return Json(new
            {
                success = true,
                message = "Mã OTP đã được gửi",
                email = MaskEmail(user.Email)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ConfirmPaymentWithOtp(int orderId, string otp)
        {
            int userId = (int)(Session["UserId"] ?? 0);
            var user = db.Users.Find(userId);

            if (!_otpService.VerifyOTP(user.Email, otp, "Payment"))
                return Json(new { success = false, message = "Mã OTP không chính xác hoặc đã hết hạn." });

            // ✅ LẤY DỮ LIỆU TỪ SESSION
            var pendingOrderJson = Session["PendingOrder"] as string;
            if (string.IsNullOrEmpty(pendingOrderJson))
                return Json(new { success = false, message = "Phiên đặt hàng đã hết hạn." });

            dynamic pendingOrder = Newtonsoft.Json.JsonConvert.DeserializeObject(pendingOrderJson);

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Tạo đơn hàng
                    string paymentMethod = (string)pendingOrder.PaymentMethod;

                    var order = new Order
                    {
                        UserId = userId,
                        OrderDate = DateTime.Now,
                        ShippingName = (string)pendingOrder.ShippingName,
                        ShippingPhone = (string)pendingOrder.ShippingPhone,
                        ShippingAddress = (string)pendingOrder.ShippingAddress,
                        PaymentMethod = paymentMethod,          // ✅ Chỉ khai báo 1 lần
                        IsPaid = paymentMethod != "COD",
                        Status = "Pending",
                        TotalAmount = (decimal)pendingOrder.TotalAmount,
                        TotalVoucherDiscount = (decimal)pendingOrder.TotalVoucherDiscount,
                    };

                    db.Orders.Add(order);
                    db.SaveChanges(); // Lưu để lấy ID đơn hàng

                    // 2. Lưu Chi tiết & Trừ kho
                    foreach (var item in pendingOrder.CartItems)
                    {
                        int pId = (int)item.ProductId;
                        var product = db.Products.Find(pId);
                        if (product == null || product.Stock < (int)item.Quantity)
                            throw new Exception($"Sản phẩm {(string)item.ProductName} đã hết hàng.");

                        db.OrderDetails.Add(new OrderDetail
                        {
                            OrderId = order.Id,
                            ProductId = pId,
                            Quantity = (int)item.Quantity,
                            UnitPrice = (decimal)item.Price,
                            TotalPrice = (decimal)item.TotalPrice
                        });

                        product.Stock -= (int)item.Quantity;
                    }

                    // 3. Xử lý Voucher
                    string vCode = (string)pendingOrder.VoucherCode;
                    if (!string.IsNullOrEmpty(vCode))
                    {
                        var voucher = db.Vouchers.FirstOrDefault(v => v.Code == vCode);
                        if (voucher != null)
                        {
                            db.VoucherUsages.Add(new VoucherUsage { VoucherId = voucher.Id, UserId = userId, UsedAt = DateTime.Now, OrderId = order.Id });
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    // ✅ Cộng 1 spin cho user
                    var userToUpdate = db.Users.Find(userId);
                    if (userToUpdate != null)
                    {
                        userToUpdate.SpinCount += 1;
                        db.SaveChanges();
                        _logger.LogInfo($"SpinCount += 1 for user {userId}, total: {userToUpdate.SpinCount}");
                    }

                    // ✅ GỬI EMAIL HÓA ĐƠN (ASYNC)
                    Task.Run(async () =>
                    {
                        try
                        {
                            var emailUser = db.Users.Find(userId);
                            if (emailUser != null && !string.IsNullOrEmpty(emailUser.Email))
                            {
                                // Reload order với các chi tiết liên quan
                                var orderToSend = db.Orders
                                    .Include("OrderDetails")
                                    .Include("OrderDetails.Product")
                                    .FirstOrDefault(o => o.Id == order.Id);

                                if (orderToSend != null)
                                {
                                    bool emailSent = await _emailQueueService.SendImmediate(
                                        emailUser.Email,
                                        $"📦 Xác nhận đơn hàng #{order.Id}",
                                        GenerateOrderConfirmationHtml(orderToSend)
                                    );

                                    if (emailSent)
                                    {
                                        _logger.LogInfo($"✅ Order confirmation email sent to {emailUser.Email} for order #{order.Id}");
                                    }
                                    else
                                    {
                                        _logger.LogError($"❌ Failed to send order confirmation email to {emailUser.Email}");
                                    }
                                }
                            }
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError($"Error in email sending task: {emailEx.Message}", emailEx);
                        }
                    });

                    // ✅ XÓA SESSION SAU KHI THÀNH CÔNG
                    Session.Remove("PendingOrder");
                    Session["CartItems"] = new List<CartItem>();
                    Session["VoucherDiscount"] = null;

                    return Json(new
                    {
                        success = true,
                        message = "Thanh toán thành công",
                        redirectUrl = Url.Action("OrderDetails", "AccountProfile", new { id = order.Id })
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = "Lỗi lưu đơn hàng: " + ex.Message });
                }
            }
        }

        /// <summary>
        /// Tạo HTML template cho email xác nhận đơn hàng
        /// </summary>
        private string GenerateOrderConfirmationHtml(Order order)
        {
            if (order == null) return "";

            // Tính tổng tiền hàng
            decimal subtotal = order.OrderDetails?.Sum(d => d.TotalPrice) ?? 0m;
            decimal voucherDiscount = order.TotalVoucherDiscount;
            decimal finalTotal = subtotal - voucherDiscount + 30000m; // 30k phí vận chuyển

            // Build items table
            string itemsHtml = "<table style='width:100%; border-collapse: collapse;'>";
            itemsHtml += "<tr style='background-color: #f8f9fa; border-bottom: 1px solid #ddd;'>";
            itemsHtml += "<th style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Sản phẩm</th>";
            itemsHtml += "<th style='text-align: center; padding: 10px; border: 1px solid #ddd;'>Số lượng</th>";
            itemsHtml += "<th style='text-align: right; padding: 10px; border: 1px solid #ddd;'>Giá</th>";
            itemsHtml += "<th style='text-align: right; padding: 10px; border: 1px solid #ddd;'>Tổng</th>";
            itemsHtml += "</tr>";

            if (order.OrderDetails != null && order.OrderDetails.Count > 0)
            {
                foreach (var item in order.OrderDetails)
                {
                    itemsHtml += $@"
                    <tr style='border-bottom: 1px solid #ddd;'>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{item.Product?.Name ?? "N/A"}</td>
                        <td style='text-align: center; padding: 10px; border: 1px solid #ddd;'>{item.Quantity}</td>
                        <td style='text-align: right; padding: 10px; border: 1px solid #ddd;'>{item.UnitPrice:N0}₫</td>
                        <td style='text-align: right; padding: 10px; border: 1px solid #ddd;'>{item.TotalPrice:N0}₫</td>
                    </tr>";
                }
            }
            itemsHtml += "</table>";

            // Build summary
            string summaryHtml = $@"
            <table style='width:100%; margin-top: 20px;'>
                <tr style='border-bottom: 1px solid #ddd;'>
                    <td style='padding: 8px; text-align: right; width: 50%;'><strong>Tạm tính:</strong></td>
                    <td style='padding: 8px; text-align: right; width: 50%;'>{subtotal:N0}₫</td>
                </tr>";

            decimal discount = order.TotalVoucherDiscount;
            if (discount > 0)
            {
                summaryHtml += $@"
                <tr style='border-bottom: 1px solid #ddd;'>
                    <td style='padding: 8px; text-align: right;'><strong style='color: #27ae60;'>Giảm giá:</strong></td>
                    <td style='padding: 8px; text-align: right; color: #27ae60;'>-{discount:N0}₫</td>
                </tr>";
            }

            summaryHtml += $@"
                <tr style='border-bottom: 1px solid #ddd;'>
                    <td style='padding: 8px; text-align: right;'><strong>Phí vận chuyển:</strong></td>
                    <td style='padding: 8px; text-align: right;'>30,000₫</td>
                </tr>
                <tr style='background-color: #f0f0f0;'>
                    <td style='padding: 12px; text-align: right;'><strong style='font-size: 16px;'>Tổng cộng:</strong></td>
                    <td style='padding: 12px; text-align: right;'><strong style='font-size: 18px; color: #e74c3c;'>{finalTotal:N0}₫</strong></td>
                </tr>
            </table>";

            return $@"
            <html dir='ltr' lang='vi'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Xác nhận đơn hàng #{order.Id}</title>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                    .container {{ max-width: 700px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .header h1 {{ margin: 0; font-size: 28px; }}
                    .content {{ padding: 30px 20px; }}
                    .order-header {{ background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin-bottom: 20px; }}
                    .order-id {{ font-size: 24px; font-weight: bold; color: #667eea; margin: 5px 0; }}
                    .order-date {{ color: #666; font-size: 14px; }}
                    .section-title {{ font-size: 16px; font-weight: bold; color: #333; margin-top: 20px; margin-bottom: 10px; border-bottom: 2px solid #667eea; padding-bottom: 5px; }}
                    .info-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }}
                    .info-label {{ color: #666; }}
                    .info-value {{ font-weight: bold; color: #333; }}
                    .status-pending {{ background-color: #fff3cd; color: #856404; padding: 10px; border-radius: 4px; margin: 15px 0; }}
                    .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; border-top: 1px solid #ddd; }}
                    .footer p {{ margin: 5px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>📦 Xác Nhận Đơn Hàng</h1>
                    </div>
                    <div class='content'>
                        <p>Cảm ơn bạn đã mua sắm tại TechWorld!</p>
                        <p>Đơn hàng của bạn đã được tạo thành công. Vui lòng xem thông tin chi tiết dưới đây:</p>

                        <div class='order-header'>
                            <div class='order-id'>Đơn hàng #{order.Id}</div>
                            <div class='order-date'>{order.OrderDate:dd/MM/yyyy HH:mm:ss}</div>
                        </div>

                        <div class='section-title'>📋 Chi Tiết Đơn Hàng</div>
                        {itemsHtml}

                        <div class='section-title'>💰 Tính Toán Giá</div>
                        {summaryHtml}

                        <div class='section-title'>📍 Địa Chỉ Giao Hàng</div>
                        <div class='info-row'>
                            <span class='info-label'>Tên người nhận:</span>
                            <span class='info-value'>{order.ShippingName}</span>
                        </div>
                        <div class='info-row'>
                            <span class='info-label'>Số điện thoại:</span>
                            <span class='info-value'>{order.ShippingPhone}</span>
                        </div>
                        <div class='info-row'>
                            <span class='info-label'>Địa chỉ:</span>
                            <span class='info-value'>{order.ShippingAddress}</span>
                        </div>

                        <div class='section-title'>💳 Phương Thức Thanh Toán</div>
                        <div class='info-row'>
                            <span class='info-label'>Phương thức:</span>
                            <span class='info-value'>{order.PaymentMethod}</span>
                        </div>

                        <div class='status-pending'>
                            <strong>⏳ Trạng thái:</strong> Đơn hàng đang chờ xử lý<br>
                            <small>Bạn sẽ nhận được email cập nhật khi đơn hàng được xác nhận và giao.</small>
                        </div>

                        <p style='margin-top: 20px; color: #666; font-size: 14px;'>
                            Nếu bạn có bất kỳ câu hỏi nào về đơn hàng này, vui lòng liên hệ với chúng tôi ngay.
                        </p>
                    </div>
                    <div class='footer'>
                        <p>© 2026 TechWorld. All rights reserved.</p>
                        <p>📧 Email: support@techworld.vn | ☎️ Điện thoại: 1900-1234</p>
                        <p style='margin-top: 10px; color: #999;'>Đây là email tự động, vui lòng không trả lời email này.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@")) return email;
            var parts = email.Split('@');
            return parts[0].Substring(0, Math.Min(parts[0].Length, 3)) + "xxx@" + parts[1];
        }
    }
}