using System.Web.Mvc;

namespace web1.Infrastructure.Filters
{
    /// <summary>
    /// Security Headers Filter - Add HTTP security headers
    /// Prevent XSS, Clickjacking, MIME sniffing, etc.
    /// </summary>
    public class SecurityHeadersAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;

            // HSTS - Force HTTPS for 1 year
            response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");

            // X-Content-Type-Options - Prevent MIME sniffing
            response.AddHeader("X-Content-Type-Options", "nosniff");

            // X-Frame-Options - Prevent Clickjacking
            response.AddHeader("X-Frame-Options", "DENY");

            // X-XSS-Protection - Enable browser XSS protection
            response.AddHeader("X-XSS-Protection", "1; mode=block");

            // Referrer-Policy - Control referrer information
            response.AddHeader("Referrer-Policy", "strict-origin-when-cross-origin");

            // Content-Security-Policy - Prevent XSS
            response.AddHeader("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' cdnjs.cloudflare.com cdn.jsdelivr.net code.jquery.com; " +
                "style-src 'self' 'unsafe-inline' cdnjs.cloudflare.com fonts.googleapis.com; " +
                "img-src 'self' data: https:; " +
                "font-src 'self' fonts.gstatic.com; " +
                "connect-src 'self' sandbox.vnpayment.vn; " +
                "frame-ancestors 'none'");

            // Permissions Policy (formerly Feature Policy)
            response.AddHeader("Permissions-Policy", 
                "camera=(), microphone=(), geolocation=(), payment=()");

            base.OnActionExecuting(filterContext);
        }
    }
}
