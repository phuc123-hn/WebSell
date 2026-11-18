using System;
using System.Web.Mvc;

namespace web1.Infrastructure.Filters
{
    /// <summary>
    /// Global exception handler attribute
    /// Catches unhandled exceptions and logs them
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HandleErrorAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

            // Log exception
            var exception = filterContext.Exception;
            LogException(exception, filterContext.HttpContext);

            // Don't handle if already handled
            if (filterContext.ExceptionHandled)
                return;

            // Handle specific exception types
            if (exception is ArgumentNullException)
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new System.Web.Mvc.ViewDataDictionary()
                    {
                        Model = new HandleErrorInfo(
                            new Exception("Invalid request parameters"),
                            filterContext.RouteData.GetRequiredString("controller"),
                            filterContext.RouteData.GetRequiredString("action")
                        )
                    }
                };
            }
            else
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new System.Web.Mvc.ViewDataDictionary(
                        new HandleErrorInfo(exception,
                            filterContext.RouteData.GetRequiredString("controller"),
                            filterContext.RouteData.GetRequiredString("action")))
                };
            }

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.StatusCode = 500;
        }

        private void LogException(Exception exception, System.Web.HttpContextBase context)
        {
            try
            {
                // Log exception details
                string logMessage = string.Format(
                    "Exception: {0}\r\nMessage: {1}\r\nStackTrace: {2}\r\nURL: {3}",
                    exception.GetType().Name,
                    exception.Message,
                    exception.StackTrace,
                    context?.Request?.Url
                );

                System.Diagnostics.EventLog.WriteEntry("Application", logMessage, System.Diagnostics.EventLogEntryType.Error);
            }
            catch
            {
                // Silently fail if logging fails
            }
        }
    }
}
