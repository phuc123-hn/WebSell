using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Security;
using web1.Data;
using web1.Infrastructure.Logging;
using web1.Models;

namespace web1.Infrastructure.Security
{
    /// <summary>
    /// Secure authentication service with:
    /// - Login attempt tracking
    /// - Account lockout after failed attempts
    /// - IP address logging
    /// - Session timeout
    /// - CSRF protection
    /// </summary>
    public class SecureAuthenticationService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly ApplicationDbContext _db;
        private readonly FileLogger _logger;

        // Security constants
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 15;
        private const int SessionTimeoutMinutes = 30;

        public SecureAuthenticationService(IPasswordHasher passwordHasher, ApplicationDbContext db, FileLogger logger)
        {
            _passwordHasher = passwordHasher;
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user with security checks
        /// </summary>
        public bool Authenticate(string username, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                LogLoginAttempt(username, false, "Empty credentials");
                return false;
            }

            string clientIp = GetClientIpAddress();

            try
            {
                // Check if account is locked
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    LogLoginAttempt(username, false, "User not found");
                    return false;
                }

                // Check account lockout
                var lockout = _db.AccountLockouts.FirstOrDefault(al => al.UserId == user.Id && al.LockedUntil > DateTime.Now);
                if (lockout != null)
                {
                    LogLoginAttempt(username, false, $"Account locked until {lockout.LockedUntil}");
                    return false;
                }

                // Verify password
                if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    IncrementFailedAttempts(user.Id);
                    LogLoginAttempt(username, false, "Invalid password");
                    return false;
                }

                // Check if verified
                if (!user.IsVerified)
                {
                    LogLoginAttempt(username, false, "User not verified");
                    return false;
                }

                // Reset failed attempts on successful login
                var failedAttempts = _db.AccountLockouts.Where(al => al.UserId == user.Id).ToList();
                foreach (var attempt in failedAttempts)
                {
                    _db.AccountLockouts.Remove(attempt);
                }

                // Create claims identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.GivenName, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("IpAddress", clientIp),
                    new Claim("LoginTime", DateTime.Now.ToString("O"))
                };

                var identity = new ClaimsIdentity(claims, "ApplicationCookie");
                var principal = new ClaimsPrincipal(identity);

                // Create authentication cookie with timeout
                var ticket = new FormsAuthenticationTicket(
                    version: 2,
                    name: username,
                    issueDate: DateTime.Now,
                    expiration: rememberMe ? DateTime.Now.AddDays(7) : DateTime.Now.AddMinutes(SessionTimeoutMinutes),
                    isPersistent: rememberMe,
                    userData: string.Join(",", new[] { user.Id.ToString(), user.Role, clientIp })
                );

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                {
                    HttpOnly = true,  // Prevent JavaScript access
                    Secure = true,    // HTTPS only
                    SameSite = SameSiteMode.Strict,  // CSRF protection
                    Expires = ticket.Expiration
                };

                HttpContext.Current.Response.Cookies.Add(authCookie);

                // Set claims principal
                HttpContext.Current.User = principal;

                // Log successful login
                LogLoginAttempt(username, true, "Login successful");

                _db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error for {username}", ex);
                LogLoginAttempt(username, false, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sign out user securely
        /// </summary>
        public void SignOut()
        {
            FormsAuthentication.SignOut();
            if (HttpContext.Current?.Response != null)
            {
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true,
                    Secure = true
                };
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// Increment failed login attempts and lock account if needed
        /// </summary>
        private void IncrementFailedAttempts(int userId)
        {
            var lockout = _db.AccountLockouts.FirstOrDefault(al => al.UserId == userId);

            if (lockout == null)
            {
                lockout = new AccountLockout
                {
                    UserId = userId,
                    FailedAttempts = 1,
                    LockTime = DateTime.Now,
                    LockedUntil = DateTime.Now.AddMinutes(0) // Not locked yet
                };
                _db.AccountLockouts.Add(lockout);
            }
            else
            {
                lockout.FailedAttempts++;

                // Lock account after MaxFailedAttempts
                if (lockout.FailedAttempts >= MaxFailedAttempts)
                {
                    lockout.LockedUntil = DateTime.Now.AddMinutes(LockoutMinutes);
                    lockout.Reason = $"Account locked after {MaxFailedAttempts} failed attempts";

                    _logger.LogWarning($"Account {userId} locked due to failed login attempts");
                }
            }

            _db.SaveChanges();
        }

        /// <summary>
        /// Log login attempt
        /// </summary>
        private void LogLoginAttempt(string username, bool isSuccessful, string reason)
        {
            try
            {
                var attempt = new LoginAttempt
                {
                    Username = username,
                    IpAddress = GetClientIpAddress(),
                    IsSuccessful = isSuccessful,
                    AttemptTime = DateTime.Now,
                    FailureReason = reason
                };

                _db.LoginAttempts.Add(attempt);
                _db.SaveChanges();
            }
            catch
            {
                // Silently fail if logging fails
            }
        }

        /// <summary>
        /// Get client IP address
        /// </summary>
        private string GetClientIpAddress()
        {
            try
            {
                var context = HttpContext.Current;
                string ipAddress = context?.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = context?.Request.ServerVariables["REMOTE_ADDR"];

                return ipAddress ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get current user ID from claims
        /// </summary>
        public int? GetCurrentUserId()
        {
            var principal = HttpContext.Current?.User as ClaimsPrincipal;
            if (principal != null)
            {
                var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null && int.TryParse(claim.Value, out int userId))
                    return userId;
            }
            return null;
        }

        /// <summary>
        /// Get current username from claims
        /// </summary>
        public string GetCurrentUsername()
        {
            var principal = HttpContext.Current?.User as ClaimsPrincipal;
            return principal?.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// Get current role from claims
        /// </summary>
        public string GetCurrentRole()
        {
            var principal = HttpContext.Current?.User as ClaimsPrincipal;
            return principal?.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
