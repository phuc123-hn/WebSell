using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using web1.Data;
using web1.Models;
using web1.Infrastructure.Logging;

namespace web1.Services
{
    public interface IVNPayService
    {
        string BuildPaymentUrl(Order order);
        bool VerifyPaymentResponse(Dictionary<string, string> vnpayData, out string message);
    }

    public class VNPayService : IVNPayService
    {
        private readonly ApplicationDbContext _db;
        private readonly FileLogger _logger = new FileLogger();

        // VNPay configuration - READ from Web.config
        private readonly string _vnpayTmnCode;
        private readonly string _vnpaySecretKey;
        private readonly string _vnpayUrl;
        private readonly string _vnpayReturnUrl;
        private readonly string _vnpayNotifyUrl;

        public VNPayService(ApplicationDbContext db = null)
        {
            _db = db ?? new ApplicationDbContext();

            // Read from Web.config - NEVER hard-code
            _vnpayTmnCode = ConfigurationManager.AppSettings["VNPay_TmnCode"] ?? "";
            _vnpaySecretKey = ConfigurationManager.AppSettings["VNPay_SecretKey"] ?? "";
            _vnpayUrl = ConfigurationManager.AppSettings["VNPay_Url"] ?? "https://sandbox.vnpayment.vn/paygate/pay.html";
            _vnpayReturnUrl = ConfigurationManager.AppSettings["VNPay_ReturnUrl"] ?? "";
            _vnpayNotifyUrl = ConfigurationManager.AppSettings["VNPay_NotifyUrl"] ?? "";

            ValidateConfiguration();
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_vnpayTmnCode) || string.IsNullOrEmpty(_vnpaySecretKey))
            {
                _logger.LogError("VNPay configuration is incomplete. Check Web.config for VNPay_TmnCode and VNPay_SecretKey");
            }
        }

        /// <summary>
        /// Build VNPay payment URL
        /// </summary>
        public string BuildPaymentUrl(Order order)
        {
            try
            {
                if (order == null || order.Id == 0)
                {
                    _logger.LogWarning("BuildPaymentUrl: Invalid order");
                    return null;
                }

                var vnpayData = new SortedDictionary<string, string>();

                // Required VNPay parameters
                vnpayData.Add("vnp_Version", "2.1.0");
                vnpayData.Add("vnp_Command", "pay");
                vnpayData.Add("vnp_TmnCode", _vnpayTmnCode);
                vnpayData.Add("vnp_Amount", ((long)(order.TotalAmount * 100)).ToString()); // Amount in 100 VND units
                vnpayData.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpayData.Add("vnp_CurrCode", "VND");
                vnpayData.Add("vnp_OrderInfo", $"Thanh toan don hang #{order.Id}");
                vnpayData.Add("vnp_OrderType", "other");
                vnpayData.Add("vnp_ReturnUrl", _vnpayReturnUrl);
                vnpayData.Add("vnp_Locale", "vi");
                vnpayData.Add("vnp_IpAddr", GetClientIpAddress());

                // Build query string
                var queryBuilder = new StringBuilder();
                foreach (var kvp in vnpayData)
                {
                    queryBuilder.Append(System.Web.HttpUtility.UrlEncode(kvp.Key) + "=" + System.Web.HttpUtility.UrlEncode(kvp.Value) + "&");
                }

                // Calculate secure hash
                string queryString = queryBuilder.ToString().TrimEnd('&');
                string secureHash = ComputeHmacSha512(queryString, _vnpaySecretKey);
                string paymentUrl = _vnpayUrl + "?" + queryString + "&vnp_SecureHash=" + secureHash;

                _logger.LogInfo($"BuildPaymentUrl: Payment URL built for order {order.Id}");
                return paymentUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error building VNPay payment URL for order {order?.Id}", ex);
                return null;
            }
        }

        /// <summary>
        /// Verify VNPay payment response with secure hash
        /// </summary>
        public bool VerifyPaymentResponse(Dictionary<string, string> vnpayData, out string message)
        {
            message = "";

            try
            {
                if (vnpayData == null || vnpayData.Count == 0)
                {
                    message = "VNPay data is empty";
                    _logger.LogWarning("VerifyPaymentResponse: " + message);
                    return false;
                }

                // Extract secure hash
                if (!vnpayData.TryGetValue("vnp_SecureHash", out string receivedHash))
                {
                    message = "Secure hash not found in response";
                    _logger.LogWarning("VerifyPaymentResponse: " + message);
                    return false;
                }

                // Build string for hash verification (without vnp_SecureHash)
                var sortedData = new SortedDictionary<string, string>(vnpayData);
                sortedData.Remove("vnp_SecureHash");
                sortedData.Remove("vnp_SecureHashType");

                var queryBuilder = new StringBuilder();
                foreach (var kvp in sortedData)
                {
                    queryBuilder.Append(System.Web.HttpUtility.UrlEncode(kvp.Key) + "=" + System.Web.HttpUtility.UrlEncode(kvp.Value) + "&");
                }

                string queryString = queryBuilder.ToString().TrimEnd('&');
                string calculatedHash = ComputeHmacSha512(queryString, _vnpaySecretKey);

                // Compare hashes (case-insensitive)
                if (!receivedHash.Equals(calculatedHash, StringComparison.OrdinalIgnoreCase))
                {
                    message = "Secure hash verification failed";
                    _logger.LogWarning("VerifyPaymentResponse: " + message);
                    return false;
                }

                // Check response code
                if (!vnpayData.TryGetValue("vnp_ResponseCode", out string responseCode))
                {
                    message = "Response code not found";
                    _logger.LogWarning("VerifyPaymentResponse: " + message);
                    return false;
                }

                if (responseCode == "00")
                {
                    message = "Payment successful";
                    _logger.LogInfo("VerifyPaymentResponse: " + message);
                    return true;
                }

                // Handle various VNPay response codes
                message = GetVNPayErrorMessage(responseCode);
                _logger.LogWarning($"VerifyPaymentResponse: Payment failed with code {responseCode}");
                return false;
            }
            catch (Exception ex)
            {
                message = "Error verifying payment response";
                _logger.LogError("Error verifying VNPay response", ex);
                return false;
            }
        }

        /// <summary>
        /// Calculate HMAC-SHA512 hash
        /// </summary>
        private string ComputeHmacSha512(string message, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Get client IP address
        /// </summary>
        private string GetClientIpAddress()
        {
            try
            {
                var context = System.Web.HttpContext.Current;
                if (context == null) return "0.0.0.0";

                string ip = context.Request.ServerVariables["HTTP_CF_CONNECTING_IP"];
                if (string.IsNullOrEmpty(ip))
                    ip = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ip))
                    ip = context.Request.ServerVariables["REMOTE_ADDR"];
                if (string.IsNullOrEmpty(ip))
                    ip = context.Request.UserHostAddress;

                return ip ?? "0.0.0.0";
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        /// <summary>
        /// Get user-friendly error message from VNPay response code
        /// </summary>
        private string GetVNPayErrorMessage(string responseCode)
        {
            var errorMessages = new Dictionary<string, string>
            {
                { "01", "Giao d?ch b? t? ch?i" },
                { "02", "Merchant's account is not available" },
                { "04", "Transaction is suspected fraudulent" },
                { "05", "Amount exceeded limit" },
                { "07", "Giao d?ch không ???c h? tr?" },
                { "08", "Invalid information" },
                { "09", "Merchant not available" },
                { "10", "Unknown error" },
                { "11", "Insert User Identification" },
                { "12", "Merchant Maintains Cryptography Error" },
                { "13", "Unsupported transaction" },
                { "14", "Invalid merchant" },
                { "20", "Amount exceeds transaction limit" },
                { "99", "Giao d?ch b? h?y" }
            };

            return errorMessages.ContainsKey(responseCode) ? errorMessages[responseCode] : $"Payment error code: {responseCode}";
        }
    }
}
