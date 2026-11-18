using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace web1.Helpers
{
    /// <summary>
    /// Server-side validation helper
    /// </summary>
    public static class ValidationHelper
    {
        // Email pattern
        private const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        // Password pattern: min 8 chars, at least 1 uppercase, 1 lowercase, 1 number, 1 special char
        private const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";

        // Username pattern: alphanumeric + underscore, 3-20 chars
        private const string UsernamePattern = @"^[a-zA-Z0-9_]{3,20}$";

        /// <summary>
        /// Validate email format
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                return Regex.IsMatch(email, EmailPattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate password strength
        /// Min 8 chars, upper, lower, number, special char
        /// </summary>
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            return Regex.IsMatch(password, PasswordPattern);
        }

        /// <summary>
        /// Validate username format
        /// </summary>
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return Regex.IsMatch(username, UsernamePattern);
        }

        /// <summary>
        /// Validate phone number (basic VN format)
        /// </summary>
        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // VN phone: 10 digits starting with 0, or +84
            var cleaned = phone.Replace("-", "").Replace(" ", "");
            return Regex.IsMatch(cleaned, @"^(\+84|0)[0-9]{9}$");
        }

        /// <summary>
        /// Validate URL format
        /// </summary>
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out Uri result)
                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Sanitize input to prevent XSS
        /// </summary>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Remove script tags and dangerous attributes
            input = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"on\w+\s*=", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"<iframe[^>]*>.*?</iframe>", "", RegexOptions.IgnoreCase);

            return input.Trim();
        }

        /// <summary>
        /// Validate data annotations and return errors
        /// </summary>
        public static List<string> ValidateModel(object model)
        {
            var errors = new List<string>();
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(model);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, context, results, true))
            {
                foreach (var error in results)
                    errors.Add(error.ErrorMessage);
            }

            return errors;
        }
    }
}
