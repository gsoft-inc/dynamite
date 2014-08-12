using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using GSoft.Dynamite.Extensions;
using Microsoft.SharePoint.Utilities;
using GSoft.Dynamite.Configuration;

namespace GSoft.Dynamite.Exceptions
{
    /// <summary>
    /// Wrap your code with this handler to swallow and log all exceptions
    /// </summary>
    /// <remarks>
    /// This is meant as a presentation utility to prevent errors in Web Parts from making whole pages explode.
    /// Do not use this utility in internal or service-level code, because catching all exception types is 
    /// usually considered a bad practice.
    /// </remarks>
    public class CatchAllExceptionHandler : ICatchAllExceptionHandler
    {
        private ILogger logger;
        private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatchAllExceptionHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The project configuration</param>
        public CatchAllExceptionHandler(ILogger logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        /// <summary>
        /// Calls the void-returning method and swallows (+ logs) all exceptions types
        /// </summary>
        /// <param name="web">The context's web.</param>
        /// <param name="methodToInvoke">The delegate to invoke.</param>
        public void Execute(SPWeb web, Action methodToInvoke)
        {
            try
            {
                methodToInvoke.Invoke();
            }
            catch (ThreadAbortException threadAbortException)
            {
                var redirectSourceUrl = "<unknown>";

                if (HttpContext.Current != null)
                {
                    redirectSourceUrl = HttpContext.Current.Request.Url.AbsoluteUri;
                }

                this.logger.Info("Automatic redirection detected at " + redirectSourceUrl + ". Exception: " + threadAbortException.ToString());
            }
            catch (Exception exception)
            {
                this.LogExceptionAndEmail(web, exception);
            }
        }

        private void LogExceptionAndEmail(SPWeb web, Exception exception)
        {
            // Id the top-level calling method to flesh out the logger info
            var stackTrace = new StackTrace();
            var stackFrames = stackTrace.GetFrames();
            var callerStackFrame = stackFrames[2];
            var callerMethod = callerStackFrame.GetMethod().Name;
            var callerType = callerStackFrame.GetMethod().DeclaringType;

            var message = string.Format(
                CultureInfo.InvariantCulture,
                "An unexpected exception occurred: <ul><li>Top-level class: <b>{0}</b></li><li>Top-level method: <b>{1}</b></li><li>Exception:<br><ul><li>{2}</li></ul></li></ul>",
                callerType,
                callerMethod,
                exception.ToString());
            this.logger.Error(message);

            // Email the dev team
            string devTeamEmail = this.configuration.GetErrorEmailByMostNestedScope(web);

            if (!string.IsNullOrEmpty(devTeamEmail))
            {
                var errorUrl = "<unknown>";

                if (HttpContext.Current != null)
                {
                    errorUrl = HttpContext.Current.Request.Url.AbsoluteUri;
                }

                SendEmail(web, devTeamEmail, string.Format("[Automatic Error Email] {0} - Error at {1}", web.Title, errorUrl), message);
            }
            else
            {
                this.logger.Error("[]");
            }
        }

        private static void SendEmail(SPWeb web, string emailTo, string emailTitle, string body)
        {
            var headers = new StringDictionary();
            headers.Add("to", emailTo);
            headers.Add("subject", emailTitle);

            web.RunAsSystem(elevatedWeb =>
            {
                SPUtility.SendEmail(elevatedWeb, headers, body);
            });
        }
    }
}
