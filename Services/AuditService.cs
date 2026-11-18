using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using web1.Infrastructure.Logging;
using web1.Data;

namespace web1.Services
{
    /// <summary>
    /// Audit logging service - track all admin actions
    /// Log to database AuditLog table
    /// </summary>
    public interface IAuditService
    {
        Task LogAsync(int? userId, string action, string entity, int? entityId, 
                      string oldValue, string newValue, string description, string ipAddress);
        Task LogBanAsync(int adminId, int userId, string reason, string ipAddress);
        Task LogWarnAsync(int adminId, int postId, string reason, string ipAddress);
        Task LogPaymentAsync(int userId, int orderId, string status, string transactionId, string ipAddress);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _db;
        private readonly FileLogger _logger = new FileLogger();

        public AuditService(ApplicationDbContext db = null)
        {
            _db = db ?? new ApplicationDbContext();
        }

        public async Task LogAsync(int? userId, string action, string entity, int? entityId,
                                   string oldValue, string newValue, string description, string ipAddress)
        {
            try
            {
                var auditLog = new Models.AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Entity = entity,
                    EntityId = entityId,
                    OldValue = oldValue,
                    NewValue = newValue,
                    Description = description,
                    IpAddress = ipAddress,
                    CreatedAt = DateTime.UtcNow
                };

                _db.AuditLogs.Add(auditLog);
                await _db.SaveChangesAsync();

                _logger.LogInfo($"Audit log created: {action} on {entity} #{entityId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating audit log for {action}", ex);
            }
        }

        public async Task LogBanAsync(int adminId, int userId, string reason, string ipAddress)
        {
            await LogAsync(adminId, "BanUser", "User", userId, null, reason, 
                          $"Ng??i dùng {userId} b? c?m: {reason}", ipAddress);
        }

        public async Task LogWarnAsync(int adminId, int postId, string reason, string ipAddress)
        {
            await LogAsync(adminId, "WarnPost", "Post", postId, null, reason,
                          $"Bài vi?t {postId} b? c?nh báo: {reason}", ipAddress);
        }

        public async Task LogPaymentAsync(int userId, int orderId, string status, string transactionId, string ipAddress)
        {
            await LogAsync(userId, "Payment", "Order", orderId, null, status,
                          $"Thanh toán ??n hàng {orderId}: {status} (Transaction: {transactionId})", ipAddress);
        }
    }
}
