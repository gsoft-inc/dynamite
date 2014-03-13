using System;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// Utility to find lists
    /// </summary>
    public class ListLocator
    {
        private IResourceLocator _resources;
        private ILogger _logger;

        /// <summary>
        /// Creates a list finder
        /// </summary>
        /// <param name="resources">The resource locator</param>
        public ListLocator(IResourceLocator resources, ILogger logger)
        {
            this._resources = resources;
            this._logger = logger;
        }

        /// <summary>
        /// Find a list by its web-relative url
        /// </summary>
        /// <param name="web">The context's web</param>
        /// <param name="listUrl">The web-relative path to the list</param>
        /// <returns>The list</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Statics to be avoided in favor consistency with use of constructor injection for class collaborators.")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "List urls are available as strings through the ListUrls utility.")]
        public SPList GetByUrl(SPWeb web, string listUrl)
        {
            SPList list = null;

            try
            {
                list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, listUrl));
            }
            catch (ArgumentException)
            {
                this._logger.Warn("Failed to find list " + listUrl + " in web " + web.ServerRelativeUrl);
            }

            return list;
        }

        /// <summary>
        /// Find a list by its name's resource key
        /// </summary>
        /// <param name="web">The context's web</param>
        /// <param name="listNameResourceKey">The web-relative path to the list</param>
        /// <returns>The list</returns>
        public SPList GetByNameResourceKey(SPWeb web, string listNameResourceKey)
        {
            SPList list = null;
            string listName = string.Empty;

            try
            {
                listName = this._resources.Find(listNameResourceKey, (int)web.Language);
                    
                if (!string.IsNullOrEmpty(listName))
                {
                    list = web.Lists[listName];
                }
            }
            catch (ArgumentException)
            {
                this._logger.Warn("Failed to find list from resource key " + listNameResourceKey + " in web " + web.ServerRelativeUrl);
            }

            return list;
        }
    }
}
