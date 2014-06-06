using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.WebConfig
{
    /// <summary>
    /// Helper class to add, clean, remove WebConfig modifications programmatically
    /// </summary>
    public class WebConfigModificationHelper
    {
        /// <summary>
        /// Method to add one or multiple WebConfig modifications
        /// NOTE: There should not have 2 modifications with the same Owner.
        /// </summary>
        /// <param name="webApp">The current Web Application</param>
        /// <param name="webConfigModificationCollection">The collection of WebConfig modifications to remove-and-add</param>
        /// <remarks>All SPWebConfigModification Owner should be UNIQUE !</remarks>
        public void AddAndCleanWebConfigModification(SPWebApplication webApp, Collection<SPWebConfigModification> webConfigModificationCollection)
        {
            // Verify emptyness
            if (webConfigModificationCollection == null || !webConfigModificationCollection.Any())
            {
                throw new ArgumentNullException("webConfigModificationCollection");
            }

            SPWebApplication webApplication = SPWebService.ContentService.WebApplications[webApp.Id];

            // Start by cleaning up any existing modification for all owners
            foreach (var owner in webConfigModificationCollection.Select(modif => modif.Owner).Distinct())
            {
                // Remove all modification by the same owner.
                // By Good practice, owner should be unique, so we do this to remove duplicates entries if any.
                this.RemoveExistingModificationsFromOwner(webApplication, owner);
            }

            if (webApplication.Farm.TimerService.Instances.Count > 1)
            {
                // HACK:
                //
                // When there are multiple front-end Web servers in the
                // SharePoint farm, we need to wait for the timer job that
                // performs the Web.config modifications to complete before
                // continuing. Otherwise, we may encounter the following error
                // (e.g. when applying Web.config changes from two different
                // features in rapid succession):
                // 
                // "A web configuration modification operation is already
                // running."
                WaitForOnetimeJobToFinish(
                   webApplication.Farm,
                   "Microsoft SharePoint Foundation Web.Config Update",
                   120);
            }

            // Add WebConfig modifications
            foreach (var webConfigModification in webConfigModificationCollection)
            {
                webApplication.WebConfigModifications.Add(webConfigModification);
            }

            // Commit modification additions to the specified web application
            webApplication.Update();

            // Push modifications through the farm
            webApplication.WebService.ApplyWebConfigModifications();

            if (webApplication.Farm.TimerService.Instances.Count > 1)
            {
                WaitForOnetimeJobToFinish(
                   webApplication.Farm,
                   "Microsoft SharePoint Foundation Web.Config Update",
                   120);
            }
        }

        /// <summary>
        /// Method to remove all existing WebConfig Modifications by the same owner.
        /// By Design, owner should be unique so we can remove duplicates.
        /// </summary>
        /// <param name="webApplication">The current Web Application</param>
        /// <param name="owner">The Owner key. Only one modification should have that owner</param>
        /// <remarks>All SPWebConfigModification Owner should be UNIQUE !</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of public static members discouraged in favor of dependency injection.")]
        public void RemoveExistingModificationsFromOwner(SPWebApplication webApplication, string owner)
        {
            var removeCollection = new Collection<SPWebConfigModification>();
            var modificationCollection = webApplication.WebConfigModifications;

            int count = modificationCollection.Count;
            for (int i = 0; i < count; i++)
            {
                SPWebConfigModification modification = modificationCollection[i];
                if (modification.Owner == owner)
                {
                    // collect modifications to delete
                    removeCollection.Add(modification);
                }
            }

            // now delete the modifications from the web application
            if (removeCollection.Count > 0)
            {
                foreach (SPWebConfigModification modificationItem in removeCollection)
                {
                    webApplication.WebConfigModifications.Remove(modificationItem);
                }

                // Commit modification removals to the specified web application
                webApplication.Update();

                // Push modifications through the farm
                webApplication.WebService.ApplyWebConfigModifications();
            }
        }

        private static bool IsJobDefined(
            SPFarm farm,
            string jobTitle)
        {
            SPServiceCollection services = farm.Services;

            foreach (SPService service in services)
            {
                foreach (SPJobDefinition job in service.JobDefinitions)
                {
                    if (string.Compare(
                        job.Title,
                        jobTitle,
                        StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified timer job is currently running (or
        /// scheduled to run).
        /// </summary>
        /// <param name="farm">The farm to check if the job is running on.</param>
        /// <param name="jobTitle">The title of the timer job.</param>
        /// <returns><c>true</c> if the specified timer job is currently running
        /// (or scheduled to run); otherwise <c>false</c>.</returns>
        private static bool IsJobRunning(
            SPFarm farm,
            string jobTitle)
        {
            SPServiceCollection services = farm.Services;

            foreach (SPService service in services)
            {
                foreach (SPRunningJob job in service.RunningJobs)
                {
                    if (string.Compare(
                        job.JobDefinitionTitle,
                        jobTitle,
                        StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Waits for a one-time SharePoint timer job to finish.
        /// </summary>
        /// <param name="farm">The farm on which the timer job runs.</param>
        /// <param name="jobTitle">The title of the timer job (e.g. "Windows
        /// SharePoint Services Web.Config Update").</param>
        /// <param name="maximumWaitTimeInSeconds">The maximum wait time in seconds.</param>
        /// <exception cref="System.ArgumentNullException">
        /// farm
        /// or
        /// jobTitle
        /// </exception>
        /// <exception cref="System.ArgumentException">The job title must be specified.;jobTitle</exception>
        private static void WaitForOnetimeJobToFinish(
            SPFarm farm,
            string jobTitle,
            int maximumWaitTimeInSeconds)
        {
            if (farm == null)
            {
                throw new ArgumentNullException("farm");
            }

            if (jobTitle == null)
            {
                throw new ArgumentNullException("jobTitle");
            }
            else if (string.IsNullOrEmpty(jobTitle) == true)
            {
                throw new ArgumentException(
                    "The job title must be specified.",
                    "jobTitle");
            }

            float waitTime = 0;

            do
            {
                bool isJobDefined = IsJobDefined(
                    farm,
                    jobTitle);

                if (isJobDefined == false)
                {
                    Console.WriteLine("The timer job (" + jobTitle + ") is not defined. It may have been removed because the job completed.");
                    break;
                }

                bool isJobRunning = IsJobRunning(farm, jobTitle);

                Console.WriteLine("The timer job (" + jobTitle + ") is currently " + (isJobRunning == true ? "running" : "idle") + ". Waiting for the job to finish...");

                int sleepTime = 5000; // milliseconds

                Thread.Sleep(sleepTime);
                waitTime += sleepTime / 1000.0F; // seconds
            }
            while (waitTime < maximumWaitTimeInSeconds);

            if (waitTime >= maximumWaitTimeInSeconds)
            {
                Console.WriteLine("Waited the maximum amount of time (" + maximumWaitTimeInSeconds + " seconds) for the" + " one-time job (" + jobTitle + ") to finish.");
            }
            else
            {
                Console.WriteLine("Waited " + waitTime + " seconds for the one-time job (" + jobTitle + ") to finish.");
            }
        }
    }
}
