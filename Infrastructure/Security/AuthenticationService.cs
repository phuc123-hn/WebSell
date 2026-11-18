using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Security;
using web1.Data;
using web1.Models;

namespace web1.Infrastructure.Security
{
    /// <summary>
    /// Authentication service using Claims-based identity
    /// Replaces Session["UserId"] with secure claims
    /// </summary>
    public interface IAuthenticationService
    {
        bool Authenticate(string username, string password, bool rememberMe = false);
        void SignOut();
        int? GetCurrentUserId();
        string GetCurrentUsername();
        string GetCurrentRole();
        User GetCurrentUser(ApplicationDbContext db);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly ApplicationDbContext _db;

        public AuthenticationService(IPasswordHasher passwordHasher, ApplicationDbContext db)
        {
            _passwordHasher = passwordHasher;
            _db = db;
        }

        /// <summary>
        /// Authenticate user and create secure claims
        /// </summary>
        public bool Authenticate(string username, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            try
            {
                // Find user
                var user = _db.Users.Find(username);
                if (user == null)
                    return false;

                // Verify password
                if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
                    return false;

                // Check if verified
                if (!user.IsVerified)
                    return false;

                // Create claims identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.GivenName, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("LastLogin", DateTime.Now.ToString("O"))
                };

                var identity = new ClaimsIdentity(claims, "ApplicationCookie");
                var principal = new ClaimsPrincipal(identity);

                // Create authentication cookie
                var ticket = new FormsAuthenticationTicket(
                    version: 2,
                    name: username,
                    issueDate: DateTime.Now,
                    expiration: rememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddHours(8),
                    isPersistent: rememberMe,
                    userData: string.Join(",", new[] { user.Id.ToString(), user.Role })
                );

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                {
                    HttpOnly = true,
                    Secure = true, // HTTPS only
                    Expires = ticket.Expiration
                };

                HttpContext.Current.Response.Cookies.Add(authCookie);

                // Set claims principal
                HttpContext.Current.User = principal;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sign out user
        /// </summary>
        public void SignOut()
        {
            FormsAuthentication.SignOut();
            if (HttpContext.Current?.Response != null)
            {
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                HttpContext.Current.Response.Cookies.Add(cookie);
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

        /// <summary>
        /// Get full current user object from database
        /// </summary>
        public User GetCurrentUser(ApplicationDbContext db)
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
                return db.Users.Find(userId.Value);
            return null;
        }
    }
}
