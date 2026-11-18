using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using web1.Data;
using web1.Infrastructure.Logging;
using web1.Services;

namespace web1.Infrastructure.Filters
{
    /// <summary>
    /// Anti-Abuse Rate Limiting Attribute
    /// Ch?n brute force tr�n Login, Register, Payment, API
    /// K?t h?p IP, Device Fingerprint, Email ?? ph�t hi?n attack patterns
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private readonly int _maxAttempts;
        private readonly int _timeWindowSeconds;
        private readonly string _identifierType; // "Email", "Username", "IP", "UserId"
        private readonly FileLogger _logger = new FileLogger();

        public RateLimitAttribute(int maxAttempts = 5, int timeWindowSeconds = 300, string identifierType = "IP")
        {
            _maxAttempts = maxAttempts;
            _timeWindowSeconds = timeWindowSeconds;
            _identifierType = identifierType;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                // 1. L?y identifier
                string identifier = GetIdentifier(filterContext, _identifierType);
                if (string.IsNullOrEmpty(identifier))
                {
                    base.OnActionExecuting(filterContext);
                    return;
                }

                // 2. L?y th�ng tin request
                string ipAddress = GetClientIpAddress(filterContext.HttpContext);
                string userAgent = filterContext.HttpContext.Request.UserAgent;
                string deviceFingerprint = filterContext.HttpContext.Request.Cookies["device-fp"]?.Value;
                string actionType = DeriveActionType(filterContext);

                // 3. G?i AbuseDetectionService ?? ki?m tra
                using (var db = new ApplicationDbContext())
                {
                    var abuseService = new AbuseDetectionService(db);
                    var checkResult = abuseService.CheckRequest(
                        identifier,
                        actionType,
                        ipAddress,
                        userAgent,
                        deviceFingerprint
                    );

                    // 4. Ghi nh?n attempt n?u request ???c ph�p
                    if (checkResult.IsAllowed && checkResult.RequireChallengeType == null)
                    {
                        abuseService.RecordAttempt(identifier, actionType, null, ipAddress, userAgent, deviceFingerprint, checkResult.RiskScore);
                    }

                    // 5. X? l� k?t qu?
                    if (!checkResult.IsAllowed)
                    {
                        _logger.LogWarning($"[RATE_LIMIT] Request blocked: {identifier} ({actionType}) - {checkResult.Reason}");

                        // Phân biệt AJAX/JSON vs form submit
                        bool isAjaxRequest = filterContext.HttpContext.Request.IsAjaxRequest() ||
                                           filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

                        bool isJsonAction = filterContext.ActionDescriptor.ActionName.StartsWith("Send") ||
                                            filterContext.ActionDescriptor.ActionName.EndsWith("Json") ||
                                            filterContext.ActionDescriptor.ActionName.EndsWith("Ajax");

                        if (isAjaxRequest || isJsonAction)
                        {
                            // AJAX/JSON endpoint → trả JSON
                            filterContext.Result = new JsonResult
                            {
                                Data = new
                                {
                                    success = false,
                                    message = checkResult.Reason,
                                    requiresChallenge = checkResult.RequireChallengeType,
                                    retryAfter = checkResult.RetryAfterSeconds
                                },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }
                        else
                        {
                            // Form submit thông thường → TempData + Redirect
                            var controller = filterContext.Controller;
                            controller.TempData["RateLimitError"] = checkResult.Reason;
                            controller.TempData["RateLimitRetryAfter"] = checkResult.RetryAfterSeconds;

                            // Redirect lại cùng page (hoặc action)
                            filterContext.Result = new RedirectToRouteResult(
                                filterContext.RouteData.Values
                            );
                        }
                        return;
                    }

                    // 6. N?u c?n CAPTCHA, th�m flag v�o viewbag
                    if (checkResult.RequireChallengeType == "Captcha")
                    {
                        filterContext.Controller.ViewBag.RequireCaptcha = true;
                        filterContext.Controller.ViewBag.RiskScore = checkResult.RiskScore;
                        _logger.LogInfo($"[RATE_LIMIT] CAPTCHA required for {identifier} (risk: {checkResult.RiskScore})");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in RateLimit filter", ex);
                // Fail open - cho ph�p request n?u service b? l?i
            }

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// L?y identifier t? request
        /// </summary>
        private string GetIdentifier(ActionExecutingContext filterContext, string type)
        {
            try
            {
                var request = filterContext.HttpContext.Request;

                switch (type.ToLower())
                {
                    case "email":
                        var email = request.Form["email"] ?? request.QueryString["email"];
                        return !string.IsNullOrEmpty(email) ? email : null;

                    case "username":
                        var username = request.Form["username"] ?? request.QueryString["username"];
                        return !string.IsNullOrEmpty(username) ? username : null;

                    case "userid":
                        var userId = filterContext.HttpContext.Session?["UserId"];
                        return userId?.ToString() ?? null;

                    case "ip":
                    default:
                        return GetClientIpAddress(filterContext.HttpContext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting identifier", ex);
                return GetClientIpAddress(filterContext.HttpContext); // Fallback to IP
            }
        }

        /// <summary>
        /// X�c ??nh action type (Register, Login, Payment, etc.)
        /// </summary>
        private string DeriveActionType(ActionExecutingContext filterContext)
        {
            string action = filterContext.ActionDescriptor.ActionName.ToLower();
            if (action.Contains("register"))
                return "Register";
            else if (action.Contains("login"))
                return "Login";
            else if (action.Contains("payment") || action.Contains("checkout"))
                return "Payment";
            else if (action.Contains("password"))
                return "PasswordReset";
            else
                return "API";
        }

        /// <summary>
        /// L?y IP address c?a client (h? tr? proxy)
        /// </summary>
        private string GetClientIpAddress(HttpContextBase context)
        {
            string ip = context.Request.ServerVariables["HTTP_CF_CONNECTING_IP"];
            if (string.IsNullOrEmpty(ip))
                ip = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
                ip = context.Request.ServerVariables["REMOTE_ADDR"];
            if (string.IsNullOrEmpty(ip))
                ip = context.Request.UserHostAddress;

            return ip ?? "0.0.0.0";
        }
    }
}
