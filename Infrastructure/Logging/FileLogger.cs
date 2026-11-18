using System;
using System.IO;
using System.Web;

namespace web1.Infrastructure.Logging
{
    /// <summary>
    /// Simple logging service (production should use Serilog/NLog)
    /// </summary>
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
    }

    public class FileLogger : ILogger
    {
        private readonly string _logPath;

        public FileLogger()
        {
            _logPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Logs");
            if (!Directory.Exists(_logPath))
                Directory.CreateDirectory(_logPath);
        }

        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        public void LogError(string message, Exception ex = null)
        {
            string errorMsg = message;
            if (ex != null)
                errorMsg += Environment.NewLine + ex.ToString();
            WriteLog("ERROR", errorMsg);
        }

        private void WriteLog(string level, string message)
        {
            try
            {
                string logFile = Path.Combine(_logPath, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(logFile, logEntry);
            }
            catch
            {
                // Silently fail if logging fails
            }
        }
    }
}
