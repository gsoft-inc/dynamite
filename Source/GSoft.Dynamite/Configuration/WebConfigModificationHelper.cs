using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.Configuration
{
    /// <summary>
    /// Helper class to add, clean, remove WebConfig modifications programmatically
    /// </summary>
    public class WebConfigModificationHelper : IWebConfigModificationHelper
    {
        private ILogger logger;

        /// <summary>
        /// Creates a new instance of <see cref="WebConfigModificationHelper"/>
        /// </summary>
        /// <param name="logger">Logging utility</param>
        public WebConfigModificationHelper(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Method to add one or multiple WebConfig modifications
        /// NOTE: There should not have 2 modifications with the same Owner.
        /// </summary>
        /// <param name="webApp">The current Web Application</param>
        /// <param name="webConfigModificationCollection">The collection of WebConfig modifications to remove-and-add</param>
        /// <remarks>All SPWebConfigModification Owner should be UNIQUE !</remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging any exception that occurs while waiting for propagation of web.config modificaiton via timer jobs as Fatal so we won't miss it.")]
        public void AddAndCleanWebConfigModification(SPWebApplication webApp, Collection<SPWebConfigModification> webConfigModificationCollection)
        {
            if (webApp == null)
            {
                throw new ArgumentNullException("webApp");
            }

            if (webConfigModificationCollection == null || !webConfigModificationCollection.Any())
            {
                throw new ArgumentNullException("webConfigModificationCollection");
            }

            if (SPWebService.ContentService == null)
            {
                throw new InvalidOperationException("Error while attempting to modify web.config. SPWebServiceContentService instance is NULL.");
            }

            SPWebApplication webApplication = SPWebService.ContentService.WebApplications[webApp.Id];

            if (webApplication == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Failed to find web application with ID {0} from ContentService.", webApp.Id));
            }

            // Start by cleaning up any existing modification for all owners
            // By Good practice, owners should be unique, so we do this to remove duplicates entries if any.
            var owners = webConfigModificationCollection.Select(modif => modif.Owner).Distinct().ToList();
            this.RemoveExistingModificationsFromOwner(webApplication, owners);
            
            if (webApplication.WebConfigModifications == null)
            {
                throw new InvalidOperationException("Collection WebConfigModifications of webApplication is unexpectedly NULL. Cannot attempt to addition of web.config modification.");
            }

            // Add WebConfig modifications
            foreach (var webConfigModification in webConfigModificationCollection)
            {
                webApplication.WebConfigModifications.Add(webConfigModification);
            }

            // Commit modification additions to the specified web application
            webApplication.Update();

            if (webApplication.WebService == null)
            {
                throw new InvalidOperationException("Parent WebService of webApplication is unexpectedly NULL. Cannot attempt to ApplyWebConfigModifications.");
            }

            if (webApplication.Farm == null)
            {
                throw new InvalidOperationException("Parent Farm of webApplication is unexpectedly NULL. Cannot attempt to ApplyWebConfigModifications.");
            }

            // Push modifications through the farm
            webApplication.WebService.ApplyWebConfigModifications();

            // Wait for timer job
            try
            {
                WaitForWebConfigPropagation(webApplication.Farm);
            }
            catch (Exception exception)
            {
                this.logger.Fatal(
                    "WebConfigModificationHelper: Failed to wait for propagation of addition of web.config modification to all servers in the Farm. Exception: {0}",
                    exception.ToString());
            }
        }

        /// <summary>
        /// Method to remove all existing WebConfig Modifications by the same owner.
        /// By Design, owner should be unique so we can remove duplicates.
        /// </summary>
        /// <param name="webApplication">The current Web Application</param>
        /// <param name="ownerOfModificationToRemove">The Owner key. Only one modification should have that owner</param>
        /// <remarks>
        /// All SPWebConfigModification Owner should be UNIQUE !
        /// </remarks>
        public void RemoveExistingModificationsFromOwner(SPWebApplication webApplication, string ownerOfModificationToRemove)
        {
            this.RemoveExistingModificationsFromOwner(webApplication, new List<string>() { ownerOfModificationToRemove });
        }

        /// <summary>
        /// Method to remove all existing WebConfig Modifications for the listed owners.
        /// By Design, owner should be unique per WebConfig modification so we can remove duplicates.
        /// </summary>
        /// <param name="webApplication">The current Web Application</param>
        /// <param name="ownersOfModificationsToRemove">A list of owners for which we want to remove modifications.</param>
        /// <remarks>All SPWebConfigModification Owner should be UNIQUE !</remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging any exception that occurs while waiting for propagation of web.config modificaiton via timer jobs as Fatal so we won't miss it.")]
        public void RemoveExistingModificationsFromOwner(SPWebApplication webApplication, IList<string> ownersOfModificationsToRemove)
        {
            if (webApplication == null)
            {
                throw new ArgumentNullException("webApplication");
            }

            var indicesOfNullWebConfigModifications = new List<int>();
            var modificationsToRemove = new Collection<SPWebConfigModification>();
            var modificationCollection = webApplication.WebConfigModifications;

            if (modificationCollection == null)
            {
                this.logger.Warn(
                    "WebConfigModificationHelper: Attempted to remove web.config modification from web app with ID {0} but no existing modification exists.",
                    webApplication.Id);
            }
            else
            {
                int currentIndex = 0;
                foreach (SPWebConfigModification modification in modificationCollection)
                {
                    if (modification != null)
                    {
                        if (!string.IsNullOrEmpty(modification.Owner))
                        {
                            if (ownersOfModificationsToRemove.Contains(modification.Owner))
                            {
                                // collect modifications to delete
                                modificationsToRemove.Add(modification);
                            }
                        }
                        else
                        {
                            this.logger.Warn(
                                "WebConfigModificationHelper: owner for existing modification {0} is empty. Cannot attempt removal.",
                                modification.Name);
                        }
                    }
                    else
                    {
                        indicesOfNullWebConfigModifications.Add(currentIndex);
                        this.logger.Warn(
                            "WebConfigModificationHelper: web application with ID {0} has a NULL modification.",
                            webApplication.Id);
                    }

                    currentIndex++;
                }

                // Now delete the modifications from the web application (and also clean up NULL values from web app's WebConfigModifications collection)
                if (modificationsToRemove.Count > 0 || indicesOfNullWebConfigModifications.Count > 0)
                {
                    var webAppConfigModifications = webApplication.WebConfigModifications;

                    if (indicesOfNullWebConfigModifications.Count > 0)
                    {
                        // WEIRD EDGE CASE: we detected NULL items in the web app's WebConfigModifications collection.
                        // Let's clean those up before moving on (otherwise further additions to the collection might fail
                        // to propagate correctly).
                        this.logger.Warn(
                                "WebConfigModificationHelper: web application with ID {0} has at least one NULL modification in its WebConfigModification collection. "
                                + "Attempting to delete those NULL entries now because they might interfere with new additions to WebConfigModifications.",
                                webApplication.Id);

                        foreach (int indexOfNullWebConfigModification in indicesOfNullWebConfigModifications)
                        {
                            webAppConfigModifications.RemoveAt(indexOfNullWebConfigModification);
                        }
                    }

                    // Remove the Owner's web config modification we want to clean up
                    foreach (SPWebConfigModification modificationItem in modificationsToRemove)
                    {
                        webAppConfigModifications.Remove(modificationItem);
                    }

                    // Commit modification removals to the specified web application
                    webApplication.Update();

                    if (webApplication.WebService == null)
                    {
                        throw new InvalidOperationException("Parent WebService of webApplication is unexpectedly NULL. Cannot attempt to ApplyWebConfigModifications.");
                    }

                    if (webApplication.Farm == null)
                    {
                        throw new InvalidOperationException("Parent Farm of webApplication is unexpectedly NULL. Cannot attempt to ApplyWebConfigModifications.");
                    }

                    // Push modifications through the farm
                    webApplication.WebService.ApplyWebConfigModifications();

                    // Wait for timer job 
                    try
                    {
                        WaitForWebConfigPropagation(webApplication.Farm);
                    }
                    catch (Exception exception)
                    {
                        this.logger.Fatal(
                            "WebConfigModificationHelper: Failed to wait for propagation of removal of web.config modification to all servers in the Farm. Exception: {0}",
                            exception.ToString());
                    }
                }
            }
        }
        
        /// <summary>
        /// Waits for web configuration propagation.
        /// When there are multiple front-end Web servers in the
        /// SharePoint farm, we need to wait for the timer job that
        /// performs the Web.config modifications to complete before
        /// continuing. Otherwise, we may encounter the following error
        /// (e.g. when applying Web.config changes from two different
        /// features in rapid succession): "A web configuration modification operation is already running."
        /// </summary>
        /// <param name="farm">The SharePoint farm.</param>
        private static void WaitForWebConfigPropagation(SPFarm farm)
        {
            if (farm.TimerService.Instances.Count > 1)
            {
                WaitForOnetimeJobToFinish(farm, "Microsoft SharePoint Foundation Web.Config Update", 120);
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
