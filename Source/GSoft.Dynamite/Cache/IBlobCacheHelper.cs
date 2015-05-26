using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.Cache
{
    /// <summary>
    /// Methods to help manage BLOB Cache storage in SharePoint.
    /// </summary>
    public interface IBlobCacheHelper
    {
        /// <summary>
        /// Flushes the BLOB cache for the specified Web Application.
        /// WARNING: This method needs to be run as Farm Admin and have security_admin SQL server role and the db_owner role 
        /// on the web app's content DB in order to successfully flush the web app's BLOB cache.
        /// </summary>
        /// <param name="webApplication">The SharePoint web application.</param>
        void FlushBlobCache(SPWebApplication webApplication);

        /// <summary>
        /// Ensures the BLOB cache is enabled or disabled in the specified Web Application.
        /// This method Updates the web application web.config file in order to enable or disable BLOB Cache.
        /// </summary>
        /// <param name="webApplication">The SharePoint web application.</param>
        /// <param name="enabled">Enable or disable the BLOB Cache.</param>
        void EnsureBlobCache(SPWebApplication webApplication, bool enabled);
    }
}
