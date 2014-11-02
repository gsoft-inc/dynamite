namespace GSoft.Dynamite.TimerJobs
{
    using System;

    using Microsoft.SharePoint;

    /// <summary>
    /// The timer job helper
    /// </summary>
    public interface ITimerJobHelper
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
        /// <param name="jobName">
        /// The job name.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If jobName is not found, exception is thrown
        /// </exception>
        /// <returns>
        /// The job id<see cref="Guid"/>.
        /// </returns>
        Guid StartJob(SPSite site, string jobName);

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
}