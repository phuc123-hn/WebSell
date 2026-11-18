using System;
using System.Web;

namespace web1.Helpers
{
    /// <summary>
    /// Helper class ?? qu?n lý landing page experience
    /// </summary>
    public static class LandingHelper
    {
        private const string LANDING_COOKIE_NAME = "LandingViewed";
        private const int COOKIE_EXPIRY_DAYS = 30;

        /// <summary>
        /// Kiểm tra user đã xem landing page chưa
        /// </summary>
        public static bool HasViewedLanding()
        {
            try
            {
                HttpCookie cookie = HttpContext.Current?.Request.Cookies[LANDING_COOKIE_NAME];
                return cookie != null && cookie.Value == "true";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Đánh dấu user đã xem landing page
        /// </summary>
        public static void MarkLandingAsViewed(HttpResponseBase response)
        {
            try
            {
                HttpCookie landingCookie = new HttpCookie(LANDING_COOKIE_NAME)
                {
                    Value = "true",
                    Expires = DateTime.Now.AddDays(COOKIE_EXPIRY_DAYS),
                    Path = "/"
                };
                response.Cookies.Add(landingCookie);
            }
            catch
            {
                // Silent fail
            }
        }

        /// <summary>
        /// Xóa cookie landing page (cho phép user xem lại)
        /// </summary>
        public static void ClearLandingCookie(HttpResponseBase response)
        {
            try
            {
                HttpCookie cookie = new HttpCookie(LANDING_COOKIE_NAME)
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Path = "/"
                };
                response.Cookies.Add(cookie);
            }
            catch
            {
                // Silent fail
            }
        }

        /// <summary>
        /// Toggle landing page visibility (for testing)
        /// </summary>
        public static void ToggleLandingCookie(HttpResponseBase response)
        {
            if (HasViewedLanding())
            {
                ClearLandingCookie(response);
            }
            else
            {
                MarkLandingAsViewed(response);
            }
        }
    }
}
