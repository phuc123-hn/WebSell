using System;
using System.Threading.Tasks;
using web1.Infrastructure.Logging;

namespace web1.Services
{
    /// <summary>
    /// Async email service - send emails without blocking user
    /// Implement with Hangfire or BackgroundService later
    /// </summary>
    public interface IEmailQueueService
    {
        Task<bool> QueueOTPEmailAsync(string toEmail, string otp, string userName);
        Task<bool> QueuePasswordResetAsync(string toEmail, string resetLink, string userName);
        Task<bool> QueueOrderConfirmationAsync(string toEmail, int orderId, decimal totalAmount);
        Task<bool> QueuePaymentFailureAsync(string toEmail, int orderId, string reason);
        Task<bool> SendImmediate(string toEmail, string subject, string htmlBody);
    }

    public class EmailQueueService : IEmailQueueService
    {
        private readonly IEmailService _emailService;
        private readonly FileLogger _logger = new FileLogger();

        public EmailQueueService(IEmailService emailService = null)
        {
            _emailService = emailService ?? new EmailService();
        }

        public async Task<bool> QueueOTPEmailAsync(string toEmail, string otp, string userName)
        {
            try
            {
                // TODO: Implement with Hangfire
                // BackgroundJob.Enqueue(() => _emailService.SendOTP(toEmail, otp, userName));
                
                // For now, send immediately (blocking)
                return await Task.Run(() => _emailService.SendOTP(toEmail, otp, userName));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error queuing OTP email for {toEmail}", ex);
                return false;
            }
        }

        public async Task<bool> QueuePasswordResetAsync(string toEmail, string resetLink, string userName)
        {
            try
            {
                // TODO: BackgroundJob.Enqueue(() => _emailService.SendPasswordResetLink(toEmail, resetLink, userName));
                return await Task.Run(() => _emailService.SendPasswordResetLink(toEmail, resetLink, userName));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error queuing password reset email for {toEmail}", ex);
                return false;
            }
        }

        public async Task<bool> QueueOrderConfirmationAsync(string toEmail, int orderId, decimal totalAmount)
        {
            try
            {
                // Note: This is a simplified method. For full order details, use SendOrderConfirmation(toEmail, order)
                // TODO: BackgroundJob.Enqueue(() => _emailService.SendOrderConfirmation(toEmail, order));
                string subject = $"✅ Xác nhận đơn hàng #{orderId}";
                string htmlBody = $@"
                    <html dir='ltr' lang='vi'>
                    <head>
                        <meta charset='UTF-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
                            .container {{ max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <p>Cảm ơn bạn đã mua sắm tại TechWorld!</p>
                            <p>Đơn hàng <strong>#{orderId}</strong> của bạn đã được tạo thành công.</p>
                            <p>Tổng tiền: <strong>{totalAmount:N0}₫</strong></p>
                            <p>Bạn sẽ nhận được thông tin cập nhật về đơn hàng qua email này.</p>
                        </div>
                    </body>
                    </html>
                ";
                return await Task.Run(() => _emailService.SendGenericEmail(toEmail, subject, htmlBody));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error queuing order confirmation email for {toEmail}", ex);
                return false;
            }
        }

        public async Task<bool> QueuePaymentFailureAsync(string toEmail, int orderId, string reason)
        {
            try
            {
                string subject = $"⚠️ Thanh toán thất bại - Đơn hàng #{orderId}";
                string htmlBody = $@"
                    <html dir='ltr' lang='vi'>
                    <head>
                        <meta charset='UTF-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                            .header {{ background-color: #e74c3c; color: white; padding: 30px 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                            .content {{ padding: 30px 20px; }}
                            .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #666; border-top: 1px solid #ddd; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>⚠️ Thanh Toán Thất Bại</h1>
                            </div>
                            <div class='content'>
                                <p>Xin chào,</p>
                                <p>Thanh toán cho đơn hàng <strong>#{orderId}</strong> đã thất bại.</p>
                                <p><strong>Lý do:</strong> {reason}</p>
                                <p>Vui lòng thử lại hoặc chọn phương thức thanh toán khác.</p>
                            </div>
                            <div class='footer'>
                                <p>© 2026 TechWorld. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                // TODO: BackgroundJob.Enqueue(() => _emailService.SendGenericEmail(toEmail, subject, htmlBody));
                return await Task.Run(() => _emailService.SendGenericEmail(toEmail, subject, htmlBody));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error queuing payment failure email for {toEmail}", ex);
                return false;
            }
        }

        public async Task<bool> SendImmediate(string toEmail, string subject, string htmlBody)
        {
            try
            {
                return await Task.Run(() => _emailService.SendGenericEmail(toEmail, subject, htmlBody));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending immediate email to {toEmail}", ex);
                return false;
            }
        }
    }
}
