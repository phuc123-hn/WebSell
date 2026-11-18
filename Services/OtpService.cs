using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using web1.Data;
using web1.Models;
using web1.Infrastructure.Logging;
using web1.Infrastructure.Security;

namespace web1.Services
{
    /// <summary>
    /// D?ch v? Anti-Abuse: Rate Limiting, Risk Scoring, Challenge Detection
    /// H? tr? frontend filtering, backend throttling, CAPTCHA, v� behavior analysis
    /// </summary>
    public interface IAbuseDetectionService
    {
        AbuseCheckResult CheckRequest(string identifier, string actionType, string ipAddress = null, string userAgent = null, string deviceFingerprint = null);
        bool IsRateLimited(string identifier, string actionType, int maxAttempts = 5, int windowMinutes = 60);
        int CalculateRiskScore(string identifier, string actionType, int attemptCount, int riskFactors = 0);
        void RecordAttempt(string identifier, string actionType, int? userId = null, string ipAddress = null, string userAgent = null, string deviceFingerprint = null, int riskScore = 0);
        void ClearRecords(string identifier, string actionType);
        List<RateLimitToken> GetRecentAttempts(string identifier, string actionType, int minutesBack = 60);
    }

    public class AbuseDetectionService : IAbuseDetectionService
    {
        private readonly ApplicationDbContext _db;
        private readonly FileLogger _logger = new FileLogger();

        // Configuration
        private const int DEFAULT_WINDOW_MINUTES = 60;
        private const int MAX_ATTEMPTS_THRESHOLD = 5;
        private const int RISK_SCORE_THRESHOLD_CAPTCHA = 40;  // Y�u c?u CAPTCHA
        private const int RISK_SCORE_THRESHOLD_BLOCK = 80;    // Block ngay
        private const int COOLDOWN_SECONDS = 60;              // Gi?i h?n g?i l?i

        public AbuseDetectionService(ApplicationDbContext db = null)
        {
            _db = db ?? new ApplicationDbContext();
        }

        /// <summary>
        /// Ki?m tra m?t y�u c?u - tr? v? k?t qu? abuse check
        /// </summary>
        public AbuseCheckResult CheckRequest(string identifier, string actionType, string ipAddress = null, string userAgent = null, string deviceFingerprint = null)
        {
            try
            {
                var result = new AbuseCheckResult { IsAllowed = true };

                // 1. Ki?m tra blacklist to�n c?c (IP, Device b? block)
                var isBlocked = _db.RateLimitTokens
                    .Where(r => r.Identifier == identifier && r.Status == "Blocked" && r.ExpiresAt > DateTime.UtcNow)
                    .Any();

                if (isBlocked)
                {
                    result.IsAllowed = false;
                    result.Reason = "T�i kho?n/IP b? kh�a t?m th?i do qu� nhi?u y�u c?u ?�ng nghi.";
                    result.RequireChallengeType = "Block";
                    _logger.LogWarning($"[ABUSE] Blocked request from {identifier} for action {actionType}");
                    return result;
                }

                // 2. L?y c�c n? l?c g?n ?�y
                var recentAttempts = GetRecentAttempts(identifier, actionType, DEFAULT_WINDOW_MINUTES);
                int attemptCount = recentAttempts.Count;

                // 3. T�nh Risk Score
                int riskFactors = 0;
                if (!string.IsNullOrEmpty(deviceFingerprint))
                {
                    // Ki?m tra device c� m?i kh�ng
                    var sameDeviceOtherActions = _db.RateLimitTokens
                        .Where(r => r.DeviceFingerprint == deviceFingerprint && r.CreatedAt > DateTime.UtcNow.AddHours(-1))
                        .Select(r => r.ActionType)
                        .Distinct()
                        .Count();
                    
                    if (sameDeviceOtherActions > 3) // Device n�y th? nhi?u action trong 1 gi?
                        riskFactors += 20;
                }

                int riskScore = CalculateRiskScore(identifier, actionType, attemptCount, riskFactors);

                // 4. Quy?t ??nh h�nh ??ng d?a v�o Risk Score
                if (riskScore >= RISK_SCORE_THRESHOLD_BLOCK)
                {
                    result.IsAllowed = false;
                    result.Reason = "Qu� nhi?u y�u c?u ?�ng nghi. Vui l�ng th? l?i sau.";
                    result.RequireChallengeType = "Block";
                    result.RiskScore = riskScore;

                    // Block n�y IP/Device
                    var token = new RateLimitToken
                    {
                        Identifier = identifier,
                        ActionType = actionType,
                        IpAddress = ipAddress,
                        UserAgent = userAgent,
                        DeviceFingerprint = deviceFingerprint,
                        Status = "Blocked",
                        RiskScore = riskScore,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                        CreatedAt = DateTime.UtcNow,
                        BlockReason = "Risk score qu� cao"
                    };
                    _db.RateLimitTokens.Add(token);
                    _db.SaveChanges();

                    _logger.LogError($"[ABUSE] HIGH RISK blocked: {identifier} (score: {riskScore})");
                }
                else if (riskScore >= RISK_SCORE_THRESHOLD_CAPTCHA || attemptCount >= MAX_ATTEMPTS_THRESHOLD)
                {
                    result.IsAllowed = true;
                    result.RequireChallengeType = "Captcha";
                    result.RiskScore = riskScore;
                    _logger.LogWarning($"[ABUSE] CAPTCHA required for {identifier} (risk: {riskScore}, attempts: {attemptCount})");
                }

                // 5. Ki?m tra Cooldown (ch?ng spam g?i l?i)
                if (recentAttempts.Any())
                {
                    var lastAttempt = recentAttempts.OrderByDescending(r => r.CreatedAt).First();
                    var secondsSinceLastAttempt = (DateTime.UtcNow - lastAttempt.CreatedAt).TotalSeconds;

                    if (secondsSinceLastAttempt < COOLDOWN_SECONDS)
                    {
                        result.IsAllowed = false;
                        result.Reason = $"Vui l�ng ch? {(int)(COOLDOWN_SECONDS - secondsSinceLastAttempt)} gi�y tr??c khi th? l?i.";
                        result.RetryAfterSeconds = (int)(COOLDOWN_SECONDS - secondsSinceLastAttempt);
                        _logger.LogInfo($"[RATE_LIMIT] Cooldown enforced for {identifier}");
                        return result;
                    }
                }

                result.RiskScore = riskScore;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CheckRequest for {identifier}", ex);
                return new AbuseCheckResult { IsAllowed = true }; // Fail open - cho ph�p request n?u service b? l?i
            }
        }

        /// <summary>
        /// Ki?m tra rate limit
        /// </summary>
        public bool IsRateLimited(string identifier, string actionType, int maxAttempts = MAX_ATTEMPTS_THRESHOLD, int windowMinutes = DEFAULT_WINDOW_MINUTES)
        {
            try
            {
                var recentAttempts = GetRecentAttempts(identifier, actionType, windowMinutes);
                return recentAttempts.Count >= maxAttempts;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking rate limit for {identifier}", ex);
                return false;
            }
        }

        /// <summary>
        /// T�nh Risk Score d?a v�o attempt count v� risk factors
        /// C�ng th?c: attemptCount * 10 + riskFactors
        /// </summary>
        public int CalculateRiskScore(string identifier, string actionType, int attemptCount, int riskFactors = 0)
        {
            // Base score: m?i attempt = 10 ?i?m
            int baseScore = attemptCount * 10;

            // Th�m riskFactors (device m?i, geo-blocking, v.v.)
            int totalScore = baseScore + riskFactors;

            // Cap t?i 100
            return Math.Min(totalScore, 100);
        }

        /// <summary>
        /// Ghi nh?n m?t attempt
        /// </summary>
        public void RecordAttempt(string identifier, string actionType, int? userId = null, string ipAddress = null, string userAgent = null, string deviceFingerprint = null, int riskScore = 0)
        {
            try
            {
                var token = new RateLimitToken
                {
                    Identifier = identifier,
                    ActionType = actionType,
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceFingerprint = deviceFingerprint,
                    AttemptCount = 1,
                    RiskScore = riskScore,
                    Status = "Normal",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(DEFAULT_WINDOW_MINUTES)
                };

                _db.RateLimitTokens.Add(token);
                _db.SaveChanges();

                _logger.LogInfo($"[RECORD] Attempt recorded: {identifier} - {actionType}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error recording attempt for {identifier}", ex);
            }
        }

        /// <summary>
        /// X�a records c?a m?t identifier + actionType
        /// </summary>
        public void ClearRecords(string identifier, string actionType)
        {
            try
            {
                var records = _db.RateLimitTokens
                    .Where(r => r.Identifier == identifier && r.ActionType == actionType)
                    .ToList();

                foreach (var record in records)
                {
                    _db.RateLimitTokens.Remove(record);
                }

                _db.SaveChanges();
                _logger.LogInfo($"[CLEAR] Records cleared: {identifier} - {actionType}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing records for {identifier}", ex);
            }
        }

        /// <summary>
        /// L?y c�c n? l?c g?n ?�y
        /// </summary>
        public List<RateLimitToken> GetRecentAttempts(string identifier, string actionType, int minutesBack = DEFAULT_WINDOW_MINUTES)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-minutesBack);
                return _db.RateLimitTokens
                    .Where(r => r.Identifier == identifier && r.ActionType == actionType && r.CreatedAt > cutoffTime)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting recent attempts for {identifier}", ex);
                return new List<RateLimitToken>();
            }
        }
    }

    /// <summary>
    /// K?t qu? ki?m tra abuse
    /// </summary>
    public class AbuseCheckResult
    {
        public bool IsAllowed { get; set; } = true;
        public string Reason { get; set; }
        public string RequireChallengeType { get; set; } // None, Captcha, Block
        public int RiskScore { get; set; } = 0;
        public int RetryAfterSeconds { get; set; } = 0;
    }

    // ===== Legacy OTP Service (?? backward compatibility) =====
    public interface IOtpService
    {
        string GenerateOTP();
        bool SaveOTP(string email, string code, string otpType = "Register", int? userId = null);
        bool VerifyOTP(string email, string code, string otpType = "Register");
        bool IsRateLimited(string email, int maxAttempts = 5, int timeWindowMinutes = 60);
        bool ClearOTP(string email, string otpType = "Register");
    }

    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _db;
        private readonly FileLogger _logger = new FileLogger();
        private const int OTP_EXPIRY_MINUTES = 5;
        private const int MAX_ATTEMPTS = 5;
        private const int RATE_LIMIT_WINDOW_MINUTES = 60;
        private const int OTP_RESEND_COOLDOWN_SECONDS = 33;  // Cooldown giữa các lần resend

        public OtpService(ApplicationDbContext db = null)
        {
            _db = db ?? new ApplicationDbContext();
        }

        /// <summary>
        /// Generate OTP: 6 random digits (000000-999999)
        /// </summary>
        public string GenerateOTP()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[4];
                rng.GetBytes(tokenData);
                int randomNumber = Math.Abs(BitConverter.ToInt32(tokenData, 0)) % 1000000;
                return randomNumber.ToString("D6");
            }
        }

        /// <summary>
        /// Kiểm tra cooldown cho resend OTP
        /// Trả về số giây cần chờ (0 nếu có thể resend)
        /// </summary>
        public int GetCooldownSecondsRemaining(string email, string otpType = "Register")
        {
            try
            {
                // Lấy OTP gần nhất đã tạo
                var lastOtp = _db.RateLimitTokens
                    .Where(r => r.Identifier == email && r.ActionType == otpType)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault();

                if (lastOtp == null)
                    return 0;  // Chưa có OTP nào, có thể gửi

                var timeSinceLastOtp = DateTime.UtcNow - lastOtp.CreatedAt;
                int secondsRemaining = OTP_RESEND_COOLDOWN_SECONDS - (int)timeSinceLastOtp.TotalSeconds;

                return Math.Max(0, secondsRemaining);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking cooldown for {email}", ex);
                return 0;
            }
        }

        /// <summary>
        /// Kiểm tra xem có thể resend OTP hay không
        /// </summary>
        public bool CanResendOtp(string email, string otpType = "Register")
        {
            return GetCooldownSecondsRemaining(email, otpType) == 0;
        }

    /// <summary>
    /// Save OTP to database with expiry time and send email
    /// </summary>
    public bool SaveOTP(string email, string code, string otpType = "Register", int? userId = null)
    {
        try
        {
            var token = new RateLimitToken
            {
                Identifier = email,
                ActionType = otpType,
                UserId = userId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
                BlockReason = code // Temporarily store OTP here
            };

            _db.RateLimitTokens.Add(token);
            _db.SaveChanges();

            // Send OTP via email
            var emailService = new EmailService();
            var user = _db.Users.FirstOrDefault(u => u.Email == email || u.Phone == email);
            string userName = user?.FullName ?? "Khách hàng";

            bool emailSent = emailService.SendOTP(email, code, userName);
            if (!emailSent)
            {
                _logger.LogWarning($"Email not sent for OTP to {email}, but OTP saved in database");
            }
            else
            {
                _logger.LogInfo($"OTP email sent successfully to {email} for {otpType}");
            }

            _logger.LogInfo($"OTP saved and email sent to {email} for {otpType}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving OTP for {email}", ex);
            return false;
        }
    }

    /// <summary>
    /// Verify OTP - Check if code matches and is not expired
    /// </summary>
    public bool VerifyOTP(string email, string code, string otpType = "Register")
    {
        try
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
                return false;

            var otpRecord = _db.RateLimitTokens
                .Where(r => r.Identifier == email && 
                            r.ActionType == otpType && 
                            r.Status == "Pending" &&
                            r.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            if (otpRecord == null)
            {
                _logger.LogWarning($"No valid OTP found for {email} - {otpType}");
                return false;
            }

            // Compare OTP code (stored in BlockReason field temporarily)
            bool isValid = otpRecord.BlockReason == code;

            if (isValid)
            {
                // Mark OTP as verified
                otpRecord.Status = "Verified";
                _db.SaveChanges();
                _logger.LogInfo($"OTP verified successfully for {email}");
            }
            else
            {
                _logger.LogWarning($"OTP verification failed for {email} - incorrect code");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error verifying OTP for {email}", ex);
            return false;
        }
    }

        /// <summary>
        /// Check if email is rate limited
        /// </summary>
        public bool IsRateLimited(string email, int maxAttempts = MAX_ATTEMPTS, int timeWindowMinutes = RATE_LIMIT_WINDOW_MINUTES)
        {
            try
            {
                var recentAttempts = _db.RateLimitTokens
                    .Where(r => r.Identifier == email && 
                                r.CreatedAt > DateTime.UtcNow.AddMinutes(-timeWindowMinutes))
                    .Count();

                return recentAttempts >= maxAttempts;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking rate limit for email: {email}", ex);
                return false;
            }
        }

        /// <summary>
        /// Clear OTP records for an email
        /// </summary>
        public bool ClearOTP(string email, string otpType = "Register")
        {
            try
            {
                var otpRecords = _db.RateLimitTokens
                    .Where(r => r.Identifier == email && r.ActionType == otpType)
                    .ToList();

                foreach (var otp in otpRecords)
                {
                    _db.RateLimitTokens.Remove(otp);
                }

                _db.SaveChanges();
                _logger.LogInfo($"OTP cleared for email: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing OTP for email: {email}", ex);
                return false;
            }
        }
    }
}
