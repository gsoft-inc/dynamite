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
        /// Creates a work item to be processed by the next associated timer job.
        /// </summary>
        /// <param name="site">The site to which the work item will be associated</param>
        /// <param name="workItemType">The ID of the type of timer job to launch</param>
        Guid CreateWorkItem(SPSite site, Guid workItemType);

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
        Guid StartJobAndReturn(SPSite site, string jobName);

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
        void StartAndWaitForJob(SPSite site, string jobName);
    }
}