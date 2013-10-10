using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Sharepoint2013.Utils;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Sharepoint2013.Repositories
{
    /// <summary>
    /// Utility to find lists
    /// </summary>
    public class ListLocator
    {
        private IResourceLocator _resources;

        /// <summary>
        /// Creates a list finder
        /// </summary>
        /// <param name="resources">The resource locator</param>
        public ListLocator(IResourceLocator resources)
        {
            this._resources = resources;
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
            return web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, listUrl));
        }

        /// <summary>
        /// Find a list by its name's resource key
        /// </summary>
        /// <param name="web">The context's web</param>
        /// <param name="listNameResourceKey">The web-relative path to the list</param>
        /// <returns>The list</returns>
        public SPList GetByNameResourceKey(SPWeb web, string listNameResourceKey)
        {
            return web.Lists[this._resources.Find(listNameResourceKey, (int)web.Language)];
        }
    }
}
