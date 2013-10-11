using System;
using System.Globalization;
using System.Resources;
using System.Web;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Sharepoint2013.Utils
{
    /// <summary>
    /// Locates resource objects from either AppGlobalResources or from 14/Resources
    /// </summary>
    public class ResourceLocator : IResourceLocator
    {
        private string _defaultResourceFileName;

        /// <summary>
        /// Creates a new resource locator which will default to the provided
        /// resource file name.
        /// </summary>
        /// <param name="defaultApplicationResourceFileName">The current application's default/global resource file name</param>
        public ResourceLocator(string defaultApplicationResourceFileName)
        {
            this._defaultResourceFileName = defaultApplicationResourceFileName;
        }

        /// <summary>
        /// Retrieves the resource object specified by the key
        /// </summary>
        /// <param name="resourceKey">The resource key</param>
        /// <returns>The resource in the current UI language</returns>
        public string Find(string resourceKey)
        {
            return this.Find(this._defaultResourceFileName, resourceKey, CultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Retrieves the resource object specified by the key and language
        /// </summary>
        /// <param name="resourceKey">The resource key</param>
        /// <param name="lcid">The LCID of the desired culture</param>
        /// <returns>The resource in the specified language</returns>
        public string Find(string resourceKey, int lcid)
        {
            return this.Find(this._defaultResourceFileName, resourceKey, new CultureInfo(lcid));
        }

        /// <summary>
        /// Retrieves the resource object specified by the key and language
        /// </summary>
        /// <param name="resourceFileName">The name of to the resource file where the resource is located</param>
        /// <param name="resourceKey">The resource key</param>
        /// <returns>The resource in the specified language</returns>
        public string Find(string resourceFileName, string resourceKey)
        {
            return this.Find(resourceFileName, resourceKey, CultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Retrieves the resource object specified by the key and language
        /// </summary>
        /// <param name="resourceFileName">The name of to the resource file where the resource is located</param>
        /// <param name="resourceKey">The resource key</param>
        /// <param name="lcid">The LCID of the desired culture</param>
        /// <returns>The resource in the specified language</returns>
        public string Find(string resourceFileName, string resourceKey, int lcid)
        {
            return this.Find(resourceFileName, resourceKey, new CultureInfo(lcid));
        }

        /// <summary>
        /// Retrieves the resource object specified by the key and culture
        /// </summary>
        /// <param name="resourceFileName">The name of to the resource file where the resource is located</param>
        /// <param name="resourceKey">The resource key</param>
        /// <param name="culture">The desired culture</param>
        /// <returns>The resource in the specified language</returns>
        public string Find(string resourceFileName, string resourceKey, CultureInfo culture)
        {
            string found = string.Empty;

            try
            {
                // First, attempt to find the resource in VirtualDir/AppGlobalResources
                found = HttpContext.GetGlobalResourceObject(resourceFileName, resourceKey, culture) as string;
            }
            catch (MissingManifestResourceException)
            {
                // Swallow the exception
            }

            if (string.IsNullOrEmpty(found))
            {
                // Second, look into the 14/Resources
                found = SPUtility.GetLocalizedString("$Resources:" + resourceKey, resourceFileName, Convert.ToUInt32(culture.LCID));
            }

            return found;
        }
    }
}
