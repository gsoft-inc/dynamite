using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.TimerJobs
{
    /// <summary>
    /// The timer job helper
    /// </summary>
    public class TimerJobHelper : ITimerJobHelper
    {
        /// <summary>
        /// Creates a work item to be processed by the next associated timer job.
        /// </summary>
        /// <param name="site">The site to which the work item will be associated</param>
        /// <param name="workItemType">The ID of the type of timer job to launch</param>
        /// <returns>The Id of the created work item</returns>
        public Guid CreateWorkItem(SPSite site, Guid workItemType)
        {
            var rootWeb = site.RootWeb;
            return site.AddWorkItem(Guid.NewGuid(), DateTime.Now.ToUniversalTime(), workItemType, rootWeb.ID, site.ID, 1, false, Guid.Empty, Guid.Empty, rootWeb.CurrentUser.ID, null, string.Empty, Guid.Empty, false);
        }

        /// <summary>
        /// Starts a timer job (runs it only once)
        /// </summary>
        /// <param name="site">
        /// The site that will determine which web app's timer job definition will be used
        /// </param>
        /// <param name="jobName">
        /// The job name (i.e. the CamelCaseTimerJobTypeName).
        /// </param>
        /// <exception cref="ArgumentException">
        /// If jobName is not found, exception is thrown
        /// </exception>
        /// <returns>
        /// The started job id <see cref="Guid"/>.
        /// </returns>
        public Guid StartJobAndReturn(SPSite site, string jobName)
        {
            SPWebApplication webApplication = site.WebApplication;
            SPJobDefinition jobDefinition = (from SPJobDefinition job in
                                                 webApplication.JobDefinitions
                                             where job.Name == jobName
                                             select job).FirstOrDefault();

            if (jobDefinition != null)
            {
                jobDefinition.RunNow();
            }
            else
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error: Can't find job {0} passed as argument.", jobName));
            }

            return jobDefinition.Id;
        }

        /// <summary>
        /// Starts a timer job (runs it only once) and blocks until it is done.
        /// </summary>
        /// <param name="site">
        /// The site that will determine which web app's timer job definition will be used.
        /// </param>
        /// <param name="jobName">
        /// The job name (i.e. the CamelCaseTimerJobTypeName).
        /// </param>
        /// <exception cref="ArgumentException">
        /// If jobName is not found, exception is thrown
        /// </exception>
        public void StartAndWaitForJob(SPSite site, string jobName)
        {
            DateTime justBeforeJobStartTime = DateTime.Now.ToUniversalTime();
            Guid jobId = this.StartJobAndReturn(site, jobName);
            var webApplication = site.WebApplication;

            Console.Write(string.Format(CultureInfo.InvariantCulture, "  ~~~ Waiting for timer job {0} with ID={1} to finish...", jobName, jobId));

            // wait until the job is finished
            bool jobIsDone = false;
            while (!jobIsDone)
            {
                Console.Write(".");

                var jobDefinition = webApplication.JobDefinitions.Single(jd => jd.Id == jobId);
                jobIsDone = jobDefinition.HistoryEntries.Any(
                    historyEntry =>  
                        historyEntry.StartTime >= justBeforeJobStartTime 
                        && historyEntry.Status == SPRunningJobStatus.Succeeded 
                        && historyEntry.DatabaseName == site.ContentDatabase.Name);

                Console.Write(".");

                if (!jobIsDone)
                {
                    // wait for a relatively long while to avoid poking the content database too often
                    Thread.Sleep(5000);
                    Console.Write(".");
                }
            }

            Console.WriteLine("..done!");
        }
    }
}
