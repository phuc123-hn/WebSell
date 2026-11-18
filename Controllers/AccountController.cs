using System;
using System.Linq;
using System.Web.Mvc;
using web1.Data;
using web1.Infrastructure.Logging;
using web1.Infrastructure.Security;
using web1.Models;
using web1.Infrastructure.Filters;
using web1.Services;

namespace web1.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private IPasswordHasher _passwordHasher = new PasswordHasherService();
        private FileLogger _logger = new FileLogger();

        /// <summary>
        /// Override Json() để trả UTF-8 encoding đúng cách
        /// </summary>
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return base.Json(data, contentType, System.Text.Encoding.UTF8, behavior);
        }

        // ================= LOGIN =================
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RateLimit(maxAttempts: 5, timeWindowSeconds: 300, identifierType: "Email")]
        public ActionResult Login(string username, string password, string returnUrl, bool rememberMe = false)
        {
            // Use secure authentication service
            var authService = new SecureAuthenticationService(_passwordHasher, db, _logger);

            if (authService.Authenticate(username, password, rememberMe))
            {
                var user = db.Users.FirstOrDefault(u => u.Username == username);
                
                // Set legacy Session for backward compatibility
                Session["UserId"] = user.Id;
                Session["Username"] = user.Username;
                Session["FullName"] = user.FullName;
                Session["Role"] = user.Role;

                _logger.LogInfo($"User '{username}' logged in successfully");

                // Redirect to return URL if valid
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                // Default redirect based on role
                if (user.Role == "Admin")
                    return RedirectToAction("Index", "Home", new { area = "Admin" });

                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning($"Failed login attempt for username: {username}");
            ViewBag.Error = "Sai tài khoản hoặc mật khẩu. Hoặc tài khoản đã bị khóa do quá nhiều lần đăng nhập thất bại.";
            return View();
        }

        // ================= REGISTER =================
        public ActionResult Register()
        {
            GenerateCaptcha();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // [RateLimit(maxAttempts: 3, timeWindowSeconds: 600, identifierType: "IP")]
        public ActionResult Register(User model)
        {
            try
            {
                string confirmPassword = Request.Form["ConfirmPassword"];
                string captchaInput = Request.Form["CaptchaAnswer"];
                int captchaResult = (int)(Session["CaptchaResult"] ?? -1);

                // Validate CAPTCHA
                if (string.IsNullOrEmpty(captchaInput) || !int.TryParse(captchaInput, out int captchaAnswer) || captchaAnswer != captchaResult)
                {
                    ModelState.AddModelError("", "Xác minh Captcha sai, vui lòng thử lại");
                    GenerateCaptcha();
                    return View(model);
                }

                // Validate password match
                if (model.PasswordHash != confirmPassword)
                {
                    ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
                    GenerateCaptcha();
                    return View(model);
                }

                // Validate password strength
                var passwordErrors = ValidatePasswordStrength(model.PasswordHash);
                if (passwordErrors.Count > 0)
                {
                    foreach (var error in passwordErrors)
                        ModelState.AddModelError("", error);
                    GenerateCaptcha();
                    return View(model);
                }

                // Validate model
                if (!ModelState.IsValid)
                {
                    GenerateCaptcha();
                    return View(model);
                }

                // Check duplicate username
                if (db.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã được sử dụng");
                    GenerateCaptcha();
                    return View(model);
                }

                // Check duplicate email
                if (db.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email đã được sử dụng");
                    GenerateCaptcha();
                    return View(model);
                }

                // Check duplicate phone
                if (db.Users.Any(u => u.Phone == model.Phone))
                {
                    ModelState.AddModelError("", "Số điện thoại đã được sử dụng");
                    GenerateCaptcha();
                    return View(model);
                }

                // Send OTP
                var otpService = new OtpService(db);
                string otpCode = otpService.GenerateOTP();
                otpService.SaveOTP(model.Email, otpCode, "Register", null);  // ✅ Dùng email thay vì phone

                // Store registration data in session for later
                Session["RegisterData"] = model;
                Session["RegisterEmail"] = model.Email;  // ✅ Lưu email thay vì phone

                _logger.LogInfo($"OTP sent for registration: {model.Email}");

                // Redirect to OTP verification
                return RedirectToAction("VerifyOtpRegister", new { email = model.Email });  // ✅ Pass email thay vì phone
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during user registration", ex);
                ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
            }

            GenerateCaptcha();
            return View(model);
        }

        // ================= OTP VERIFICATION FOR REGISTRATION =================
        public ActionResult VerifyOtpRegister(string email)  // ✅ Đổi parameter thành email
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Register");

            ViewBag.Email = email;  // ✅ Pass email vào view
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RateLimit(maxAttempts: 3, timeWindowSeconds: 300, identifierType: "IP")]
        public ActionResult VerifyOtpRegister(string email, string otp)  // ✅ Dùng email
        {
            try
            {
                _logger.LogInfo($"VerifyOtpRegister called with email='{email}', otp='{otp}'");

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
                {
                    _logger.LogWarning($"Empty email or OTP: email={email}, otp={otp}");
                    ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                    ViewBag.Email = email;
                    return View();
                }

                // Verify OTP
                var otpService = new OtpService(db);
                if (!otpService.VerifyOTP(email, otp, "Register"))  // ✅ Verify với email
                {
                    ViewBag.Error = "Mã OTP không chính xác hoặc đã hết hạn";
                    ViewBag.Email = email;
                    return View();
                }

                // Get registration data from session
                var registerData = Session["RegisterData"] as User;
                if (registerData == null)
                {
                    ViewBag.Error = "Phiên đăng ký đã hết hạn, vui lòng thử lại";
                    return RedirectToAction("Register");
                }

                // Hash password
                registerData.PasswordHash = _passwordHasher.HashPassword(registerData.PasswordHash);
                registerData.IsVerified = true;
                registerData.Role = "Customer";
                registerData.CreatedAt = DateTime.Now;
                registerData.OTPExpire = DateTime.Now;

                // Save user to database
                db.Users.Add(registerData);
                db.SaveChanges();

                _logger.LogInfo($"New user registered and verified: {registerData.Username}");

                // ✅ Auto login after successful OTP verification
                var ticket = new System.Web.Security.FormsAuthenticationTicket(
                    version: 2,
                    name: registerData.Username,
                    issueDate: DateTime.Now,
                    expiration: DateTime.Now.AddMinutes(30),
                    isPersistent: false,
                    userData: string.Join(",", new[] { registerData.Id.ToString(), registerData.Role })
                );

                string encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(ticket);
                var authCookie = new System.Web.HttpCookie(
                    System.Web.Security.FormsAuthentication.FormsCookieName, encryptedTicket)
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = System.Web.SameSiteMode.Strict
                };
                Response.Cookies.Add(authCookie);

                // Set session variables (same as Login action)
                Session["UserId"] = registerData.Id;
                Session["Username"] = registerData.Username;
                Session["FullName"] = registerData.FullName;
                Session["Role"] = registerData.Role;

                // Clear session
                Session.Remove("RegisterData");
                Session.Remove("RegisterEmail");

                // Clear anti-abuse records
                var abuseService = new AbuseDetectionService(db);
                abuseService.ClearRecords(Request.UserHostAddress, "Register");

                TempData["SuccessMessage"] = "Đăng ký thành công! Chào mừng bạn.";
                return RedirectToAction("Index", "Home"); // ✅ Go to homepage directly
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying OTP for registration: {email}", ex);
                ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                ViewBag.Email = email;
                return View();
            }
        }

        // ================= SEND OTP (AJAX) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SendOtpRegister(string email)  // ✅ Đổi parameter thành email
        {
            try
            {
                // Set response content type to UTF-8
                Response.ContentType = "application/json; charset=utf-8";
                
                // Validate email format
                if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                {
                    return Json(new { success = false, message = "Email không hợp lệ" });
                }

                // Check if already registered
                if (db.Users.Any(u => u.Email == email))
                {
                    return Json(new { success = false, message = "Email đã được đăng ký" });
                }

                // ✅ Check cooldown trước khi gửi OTP
                var otpService = new OtpService(db);
                int cooldownSecondsRemaining = otpService.GetCooldownSecondsRemaining(email, "Register");
                
                if (cooldownSecondsRemaining > 0)
                {
                    _logger.LogWarning($"OTP resend blocked by cooldown for email: {email}, cooldown remaining: {cooldownSecondsRemaining}s");
                    
                    // Trả về rate limit response
                    return Json(new
                    {
                        success = false,
                        message = string.Format("Vui lòng chờ {0} giây trước khi thử lại.", cooldownSecondsRemaining),
                        requiresChallenge = (object)null,
                        retryAfter = cooldownSecondsRemaining
                    });
                }

                // Generate and send OTP
                string otpCode = otpService.GenerateOTP();
                otpService.SaveOTP(email, otpCode, "Register", null);

                _logger.LogInfo($"OTP resent successfully for registration: {email}");

                return Json(new { success = true, message = "OTP đã được gửi vào email của bạn" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending OTP: {email}", ex);
                return Json(new { success = false, message = "Lỗi khi gửi OTP" });
            }
        }

        // ================= LOGOUT =================
        public ActionResult Logout()
        {
            var authService = new SecureAuthenticationService(_passwordHasher, db, _logger);
            authService.SignOut();
            Session.Clear();

            _logger.LogInfo("User logged out");

            return RedirectToAction("Index", "Home");
        }

        // ================= TEST EMAIL (DEBUG ONLY) =================
        [HttpGet]
        public ActionResult TestEmail(string recipientEmail = "")
        {
            if (string.IsNullOrEmpty(recipientEmail))
                recipientEmail = "phucngo240726@gmail.com";
            
            return View(new { recipientEmail });
        }

        [HttpPost]
        public JsonResult TestEmailSend(string recipientEmail, string testOtp = "123456")
        {
            try
            {
                if (string.IsNullOrEmpty(recipientEmail))
                    return Json(new { success = false, message = "Email không được trống" });

                _logger.LogInfo($"[DEBUG] Test email send requested to: {recipientEmail}");

                var emailService = new EmailService();
                
                _logger.LogInfo($"[DEBUG] Sending test OTP email...");
                bool sent = emailService.SendOTP(recipientEmail, testOtp, "Test User");

                if (sent)
                {
                    _logger.LogInfo($"[DEBUG] ✅ Test email sent successfully");
                    return Json(new { 
                        success = true, 
                        message = $"✅ Email đã được gửi thành công tới {recipientEmail}\n\nKiểm tra inbox hoặc spam folder để nhận email.\n\nNếu vẫn không nhận được, kiểm tra file log để xem chi tiết lỗi." 
                    });
                }
                else
                {
                    _logger.LogInfo($"[DEBUG] ❌ Test email failed - check logs for details");
                    return Json(new { 
                        success = false, 
                        message = "❌ Gửi email thất bại. Kiểm tra file log tại ~/Logs/log_YYYY-MM-DD.txt để xem chi tiết lỗi SMTP." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DEBUG] Error in TestEmailSend: {ex.Message}", ex);
                return Json(new { 
                    success = false, 
                    message = $"❌ Lỗi: {ex.Message}\n\nKiểm tra logs để xem chi tiết." 
                });
            }
        }

        // ================= HELPERS =================
        private void GenerateCaptcha()
        {
            Random rand = new Random();
            int a = rand.Next(1, 10);
            int b = rand.Next(1, 10);

            ViewBag.CaptchaA = a;
            ViewBag.CaptchaB = b;
            Session["CaptchaResult"] = a + b;
        }

        /// <summary>
        /// Validate password strength
        /// </summary>
        private System.Collections.Generic.List<string> ValidatePasswordStrength(string password)
        {
            var errors = new System.Collections.Generic.List<string>();

            if (string.IsNullOrEmpty(password))
                errors.Add("Mật khẩu không được trống");

            if (password.Length < 8)
                errors.Add("Mật khẩu phải có ít nhất 8 ký tự");

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("Mật khẩu phải chứa ít nhất 1 chữ cái hoa");

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]"))
                errors.Add("Mật khẩu phải chứa ít nhất 1 chữ cái thường");

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]"))
                errors.Add("Mật khẩu phải chứa ít nhất 1 chữ số");

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[@$!%*?&]"))
                errors.Add("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (@$!%*?&)");

            return errors;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}