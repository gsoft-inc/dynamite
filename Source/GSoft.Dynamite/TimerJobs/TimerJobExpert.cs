// -----------------------------------------------------------------------
// <copyright file="TimerJobExpert.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.TimerJobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// The TimerJobExpert interface.
    /// </summary>
    public interface ITimerJobExpert
    {
        /// <summary>
        /// The create job.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="workItemType">
        /// The work item type.
        /// </param>
        /// <returns>
        /// The <see cref="Guid"/>.
        /// </returns>
        Guid CreateJob(SPSite site, Guid workItemType);

        /// <summary>
        /// The start job.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The jobId<see cref="Guid"/>.
        /// </returns>
        Guid StartJob(SPSite site, string name);

        /// <summary>
        /// The wait for job.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="jobId">
        /// The job id.
        /// </param>
        /// <param name="startDate">
        /// The start date.
        /// </param>
        void WaitForJob(SPSite site, Guid jobId, DateTime startDate);
    }

    /// <summary>
    /// The timer job expert.
    /// </summary>
    public class TimerJobExpert : ITimerJobExpert
    {
        /// <summary>
        /// The create job.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="workItemType">
        /// The work item type.
        /// </param>
        /// <returns>
        /// The <see cref="Guid"/>.
        /// </returns>
        public Guid CreateJob(SPSite site, Guid workItemType)
        {
            var rootWeb = site.RootWeb;

            var workItemId = site.AddWorkItem(Guid.NewGuid(), DateTime.Now.ToUniversalTime(), workItemType, rootWeb.ID, site.ID, 1, false, Guid.Empty, Guid.Empty, rootWeb.CurrentUser.ID, null, string.Empty, Guid.Empty, false);
            return workItemId;
        }

        /// <summary>
        /// The start job.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="jobName">
        /// The job name.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If jobName is not found, exception is thrown
        /// </exception>
        /// <returns>
        /// The job id<see cref="Guid"/>.
        /// </returns>
        public Guid StartJob(SPSite site, string jobName)
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
                throw new ArgumentException(string.Format("Error: Can't find job {0} passed as argument.", jobName));
            }

            return jobDefinition.Id;
        }

        /// <summary>
        /// The wait for job.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="jobId">
        /// The job id.
        /// </param>
        /// <param name="startDate">
        /// The start date.
        /// </param>
        public void WaitForJob(SPSite site, Guid jobId, DateTime startDate)
        {
            var webApplication = site.WebApplication;

            // wait until the job is finished
            while ((from SPJobHistory j in webApplication.JobHistoryEntries
                    where j.JobDefinitionId == jobId && j.StartTime >= startDate && j.Status == SPRunningJobStatus.Succeeded && j.DatabaseName == site.ContentDatabase.Name
                    select j).Any() == false)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
