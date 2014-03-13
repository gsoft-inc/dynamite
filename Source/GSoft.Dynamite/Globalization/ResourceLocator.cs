using System;
using System.Globalization;
using System.Resources;
using System.Web;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Globalization
{
    /// <summary>
    /// Locates resource objects from either AppGlobalResources or from 14/Resources
    /// </summary>
    public class ResourceLocator : IResourceLocator
    {
        private string[] _defaultResourceFileNames;

        /// <summary>
        /// Creates a new resource locator which will default to the provided
        /// resource file name.
        /// </summary>
        /// <param name="defaultApplicationResourceFileNames">The current application's default/global resource file names</param>
        public ResourceLocator(string defaultApplicationResourceFileName)
        {
            this._defaultResourceFileNames = new string[] { defaultApplicationResourceFileName };
        }

        /// <summary>
        /// Creates a new resource locator which will default to the provided
        /// resource file name.
        /// </summary>
        /// <param name="defaultApplicationResourceFileNames">The current application's default/global resource file names</param>
        public ResourceLocator(string[] defaultApplicationResourceFileNames)
        {
            this._defaultResourceFileNames = defaultApplicationResourceFileNames;
        }

        /// <summary>
        /// Retrieves the resource object specified by the key
        /// </summary>
        /// <param name="resourceKey">The resource key</param>
        /// <returns>The resource in the current UI language</returns>
        public string Find(string resourceKey)
        {
            return this.Find(resourceKey, CultureInfo.CurrentUICulture.LCID);
        }

        /// <summary>
        /// Retrieves the resource object specified by the key and language
        /// </summary>
        /// <param name="resourceKey">The resource key</param>
        /// <param name="lcid">The LCID of the desired culture</param>
        /// <returns>The resource in the specified language</returns>
        public string Find(string resourceKey, int lcid)
        {
            string resourceValue = null;

            // Scan all the default resource files
            foreach (var fileName in this._defaultResourceFileNames)
            {
                resourceValue = this.Find(fileName, resourceKey, new CultureInfo(lcid));

                if (!string.IsNullOrEmpty(resourceValue) && !resourceValue.StartsWith("$Resources"))
                {
                    // exit as soon as you find the resource in one of the default files
                    break;
                }
            }

            return resourceValue;
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
