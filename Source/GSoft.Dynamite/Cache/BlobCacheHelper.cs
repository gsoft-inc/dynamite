using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Configuration;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Cache
{
    /// <summary>
    /// Methods to help manage BLOB Cache storage in SharePoint.
    /// </summary>
    public class BlobCacheHelper : IBlobCacheHelper
    {
        private readonly ILogger logger;
        private readonly IWebConfigModificationHelper webConfigModificationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobCacheHelper"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="webConfigModificationHelper">The web configuration modification helper.</param>
        public BlobCacheHelper(ILogger logger, IWebConfigModificationHelper webConfigModificationHelper)
        {
            this.logger = logger;
            this.webConfigModificationHelper = webConfigModificationHelper;
        }

        /// <summary>
        /// Flushes the BLOB cache for the specified Web Application.
        /// WARNING: This method needs to be run as Farm Admin and have security_admin SQL server role and the db_owner role
        /// on the web app's content DB in order to successfully flush the web app's BLOB cache.
        /// </summary>
        /// <param name="webApplication">The SharePoint web application.</param>
        public void FlushBlobCache(SPWebApplication webApplication)
        {
            try
            {
                PublishingCache.FlushBlobCache(webApplication);
            }
            catch (SPException exception)
            {
                this.logger.Error("Failed to flush the BLOB cache accross the web app. You need You need security_admin SQL server role and the db_owner role on the web app's content DB. Caught and swallowed exception: {0}", exception);
            }
            catch (AccessViolationException exception)
            {
                this.logger.Warn("Received an AccessViolationException when flushing BLOB Cache. Trying again with RemoteAdministratorAccessDenied set to true. Caught and swallowed exception: {0}", exception);

                bool initialRemoteAdministratorAccessDenied = true;
                SPWebService myService = SPWebService.ContentService;

                try
                {
                    initialRemoteAdministratorAccessDenied = myService.RemoteAdministratorAccessDenied;
                    myService.RemoteAdministratorAccessDenied = false;
                    myService.Update();

                    PublishingCache.FlushBlobCache(webApplication);
                }
                finally
                {
                    myService.RemoteAdministratorAccessDenied = initialRemoteAdministratorAccessDenied;
                    myService.Update();
                }
            }
        }

        /// <summary>
        /// Ensures the BLOB cache is enabled or disabled in the specified Web Application.
        /// This method Updates the web application web.config file in order to enable or disable BLOB Cache.
        /// </summary>
        /// <param name="webApplication">The SharePoint web application.</param>
        /// <param name="enabled">Enable or disable the BLOB Cache.</param>
        public void EnsureBlobCache(SPWebApplication webApplication, bool enabled)
        {
            SPWebConfigModification modification = new SPWebConfigModification();
            modification.Path = "configuration/SharePoint/BlobCache";
            modification.Name = "enabled";
            modification.Value = enabled ? "true" : "false";
            modification.Sequence = 0;
            modification.Owner = "Dynamite-BlobCache";
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureAttribute;

            var modifications = new Collection<SPWebConfigModification>();
            modifications.Add(modification);

            this.webConfigModificationHelper.AddAndCleanWebConfigModification(webApplication, modifications);
        }
    }
}
