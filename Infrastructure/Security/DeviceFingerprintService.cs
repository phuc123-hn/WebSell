using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace web1.Infrastructure.Security
{
    /// <summary>
    /// Device Fingerprinting Service
    /// T?o signature c?a device ?? phát hi?n bot và attack patterns
    /// </summary>
    public interface IDeviceFingerprintService
    {
        string GenerateFingerprint(HttpContextBase context);
        string GenerateFingerprint(string userAgent, string acceptLanguage, string acceptEncoding);
    }

    public class DeviceFingerprintService : IDeviceFingerprintService
    {
        private const string FP_COOKIE_NAME = "device-fp";
        private const int FP_COOKIE_DAYS = 365; // L?u 1 n?m

        /// <summary>
        /// T?o Device Fingerprint d?a vào request headers
        /// Combination c?a: User-Agent, Accept-Language, Accept-Encoding, IP (partial)
        /// </summary>
        public string GenerateFingerprint(HttpContextBase context)
        {
            string userAgent = context.Request.UserAgent ?? "";
            string acceptLanguage = context.Request.Headers["Accept-Language"] ?? "";
            string acceptEncoding = context.Request.Headers["Accept-Encoding"] ?? "";

            return GenerateFingerprint(userAgent, acceptLanguage, acceptEncoding);
        }

        /// <summary>
        /// T?o fingerprint t? các header
        /// </summary>
        public string GenerateFingerprint(string userAgent, string acceptLanguage, string acceptEncoding)
        {
            string combined = $"{userAgent}|{acceptLanguage}|{acceptEncoding}";
            
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(hashedBytes).Substring(0, 16); // 16 char hash
            }
        }

        /// <summary>
        /// L?u fingerprint vào cookie
        /// </summary>
        public static void SaveFingerprintToCookie(HttpContextBase context, string fingerprint)
        {
            var cookie = new HttpCookie(FP_COOKIE_NAME, fingerprint)
            {
                Expires = DateTime.Now.AddDays(FP_COOKIE_DAYS),
                HttpOnly = true,
                Secure = context.Request.IsSecureConnection
            };
            context.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// L?y fingerprint t? cookie
        /// </summary>
        public static string GetFingerprintFromCookie(HttpContextBase context)
        {
            var cookie = context.Request.Cookies[FP_COOKIE_NAME];
            return cookie?.Value;
        }
    }

    /// <summary>
    /// Behavior Analysis Service
    /// Phát hi?n các hành vi ?áng nghi: t?c ?? request b?t th??ng, pattern attack, v.v.
    /// </summary>
    public interface IBehaviorAnalysisService
    {
        int AnalyzeRequestSpeed(DateTime lastRequestTime, int consecutiveAttempts);
        int AnalyzeIpReputation(string ipAddress);
        int AnalyzeUserBehavior(string userId, string actionType);
    }

    public class BehaviorAnalysisService : IBehaviorAnalysisService
    {
        /// <summary>
        /// Phân tích t?c ?? request - n?u quá nhanh có th? là bot
        /// </summary>
        public int AnalyzeRequestSpeed(DateTime lastRequestTime, int consecutiveAttempts)
        {
            var timeSinceLastRequest = DateTime.UtcNow - lastRequestTime;
            int riskScore = 0;

            if (consecutiveAttempts > 10)
            {
                // Quá nhi?u attempt trong th?i gian ng?n
                if (timeSinceLastRequest.TotalSeconds < 1)
                    riskScore += 30; // Bot behavior
                else if (timeSinceLastRequest.TotalSeconds < 5)
                    riskScore += 20;
                else if (timeSinceLastRequest.TotalSeconds < 30)
                    riskScore += 10;
            }

            return riskScore;
        }

        /// <summary>
        /// Ki?m tra IP reputation (??n gi?n - có th? tích h?p v?i service bên ngoài)
        /// </summary>
        public int AnalyzeIpReputation(string ipAddress)
        {
            int riskScore = 0;

            // Danh sách IP ?en (có th? t? database ho?c external service)
            string[] blacklistedIps = { }; // ?? tr?ng ?? m? r?ng sau

            // Proxy/VPN detection (??n gi?n)
            if (ipAddress.StartsWith("127.") || ipAddress == "::1")
                riskScore += 5; // Localhost - có th? là attacker testing locally

            return riskScore;
        }

        /// <summary>
        /// Phân tích hành vi user - n?u action khác th??ng có th? là account takeover
        /// </summary>
        public int AnalyzeUserBehavior(string userId, string actionType)
        {
            int riskScore = 0;

            // Ví d?: n?u user bình th??ng không bao gi? thanh toán lúc 3h sáng
            DateTime now = DateTime.Now;
            if (actionType == "Payment" && (now.Hour >= 2 && now.Hour <= 5))
                riskScore += 10; // B?t th??ng

            return riskScore;
        }
    }
}
