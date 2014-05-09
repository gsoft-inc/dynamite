using System;
using System.Globalization;
using System.Resources;
using System.Web;
using GSoft.Dynamite.Structures;
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
        public ResourceLocator(IResourceLocatorConfig resourceFileConfig)
        {
            this._defaultResourceFileNames = resourceFileConfig.ResourceFileKeys;
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
        /// Finds the specified resource.
        /// </summary>
        /// <param name="resource">The resource value configuration.</param>
        /// <returns>The resource value in the current UI language.</returns>
        [Obsolete("See ResourceValue class")]
        public string Find(ResourceValue resource)
        {
            return this.Find(resource.File, resource.Key, CultureInfo.CurrentUICulture);
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
        /// Finds the specified resource.
        /// </summary>
        /// <param name="resource">The resource value configuration.</param>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The resource in the specified language.</returns>
        [Obsolete("See ResourceValue class")]
        public string Find(ResourceValue resource, int lcid)
        {
            return this.Find(resource.File, resource.Key, new CultureInfo(lcid));
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
