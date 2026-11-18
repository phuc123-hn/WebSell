using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using web1.Data;
using web1.Infrastructure.Logging;

namespace web1.Controllers
{
    /// <summary>
    /// Health Check Controller - Monitor system health
    /// Return 200 OK if healthy, 503 if unhealthy
    /// </summary>
    [RoutePrefix("api/health")]
    public class HealthCheckController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly FileLogger _logger = new FileLogger();

        public HealthCheckController()
        {
            _db = new ApplicationDbContext();
        }

        /// <summary>
        /// Check system health
        /// </summary>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            try
            {
                // Check database connection
                bool dbHealthy = await CheckDatabaseHealthAsync();
                
                if (!dbHealthy)
                    return Json(new { status = "unhealthy", reason = "Database connection failed" }, JsonRequestBehavior.AllowGet);

                return Json(new { status = "healthy", timestamp = DateTime.UtcNow }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogError("Health check failed", ex);
                return Json(new { status = "unhealthy", reason = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Check database health
        /// </summary>
        [HttpGet]
        [Route("db")]
        [AllowAnonymous]
        public async Task<ActionResult> CheckDatabase()
        {
            try
            {
                bool healthy = await CheckDatabaseHealthAsync();
                return Json(new { database = healthy ? "connected" : "failed" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { database = "failed", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Detailed health check
        /// </summary>
        [HttpGet]
        [Route("detailed")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Detailed()
        {
            try
            {
                var healthCheck = new
                {
                    timestamp = DateTime.UtcNow,
                    database = await CheckDatabaseHealthAsync(),
                    memoryUsage = GC.GetTotalMemory(false) / 1024 / 1024 + " MB",
                    uptime = GetApplicationUptime()
                };

                return Json(healthCheck, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<bool> CheckDatabaseHealthAsync()
        {
            try
            {
                await _db.Database.Connection.OpenAsync();
                _db.Database.Connection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetApplicationUptime()
        {
            try
            {
                var uptime = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime;
                return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
            }
            catch
            {
                return "Unknown";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db?.Dispose();
            base.Dispose(disposing);
        }
    }
}
