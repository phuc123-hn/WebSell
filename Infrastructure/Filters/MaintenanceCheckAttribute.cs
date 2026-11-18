using System;
using System.Web.Mvc;

namespace web1.Infrastructure.Filters
{
    /// <summary>
    /// Maintenance Mode Filter - Activate when deploying or fixing critical issues
    /// Returns 503 Service Unavailable
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class MaintenanceCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                // Check if maintenance mode is enabled
                // This would typically check ApplicationSettings service
                // For now, check a web.config setting
                string maintenanceMode = System.Configuration.ConfigurationManager.AppSettings["MaintenanceMode"] ?? "false";

                if (bool.TryParse(maintenanceMode, out bool isMaintenanceMode) && isMaintenanceMode)
                {
                    string maintenanceMessage = System.Configuration.ConfigurationManager.AppSettings["MaintenanceMessage"] ?? 
                                               "H? th?ng ?ang b?o trì. Vui lòng quay l?i sau.";

                    // Allow admin to bypass maintenance mode
                    var userRole = filterContext.HttpContext.Session?["Role"]?.ToString();
                    if (userRole != "Admin")
                    {
                        filterContext.Result = new ViewResult
                        {
                            ViewName = "Maintenance",
                            ViewData = new ViewDataDictionary(new { Message = maintenanceMessage })
                        };

                        filterContext.HttpContext.Response.StatusCode = 503;
                        filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MaintenanceCheck: {ex.Message}");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
