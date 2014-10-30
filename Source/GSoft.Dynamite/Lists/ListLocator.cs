using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
    public class ListLocator : IListLocator
    {
        private IResourceLocator resources;
        private ILogger logger;

        /// <summary>
        /// Creates a list finder
        /// </summary>
        /// <param name="resources">The resource locator</param>
        /// <param name="logger">The Logger</param>
        public ListLocator(IResourceLocator resources, ILogger logger)
        {
            this.resources = resources;
            this.logger = logger;
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
                this.logger.Warn("Failed to find list " + listUrl + " in web " + web.ServerRelativeUrl);
            }

            return list;
        }

        /// <summary>
        /// Find a list by its name's resource key. The web's Language will be used to resolve
        /// the name of the list.
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
                listName = this.resources.Find(listNameResourceKey, web.UICulture.LCID);    // same as (int)web.Language (do not confuse with web.Locale, which refers to culture for currency, etc.
                    
                if (!string.IsNullOrEmpty(listName))
                {
                    list = web.Lists[listName];
                }
            }
            catch (ArgumentException)
            {
                this.logger.Warn("Failed to find list from resource key " + listNameResourceKey + " in web " + web.ServerRelativeUrl);
            }

            return list;
        }

        /// <summary>
        /// Attempts to find a list by trying to match with: 1) the name of the list,
        /// 2) the web-relative URL of the list, 3) the list's root folder name (relative
        /// to /Lists/), 4) by resolving the list's title through its resource key
        /// </summary>
        /// <param name="web">The web in which we should look for the list.</param>
        /// <param name="titleOrUrlOrResourceString">
        /// Can be 1) list title or 2) the web-relative URL of the list or 3) the list's root 
        /// folder name (i.e. the list's /Lists/-relative URL) or 4) a resource string formatted
        /// like "$Resources:Resource.File.Name,TitleResource.Key" or 5) the list's title 
        /// resource key (i.e. TitleResource.Key only).
        /// </param>
        /// <returns>The list if it was found, null otherwise.</returns>
        public SPList TryGetList(SPWeb web, string titleOrUrlOrResourceString)
        {
            // first try finding the list by name, simple
            var list = web.Lists.TryGetList(titleOrUrlOrResourceString);

            if (list == null)
            {
                try
                {
                    // second, try to find the list by its web-relative URL
                    list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, titleOrUrlOrResourceString));
                }
                catch (FileNotFoundException)
                {
                    // ignore exception, we need to try a third attempt that assumes the string parameter represents a resource string
                }

                if (list == null && !titleOrUrlOrResourceString.Contains("Lists"))
                {
                    try
                    {
                        // third, try to find the list by its Lists-relative URL by adding Lists if its missing
                        list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, SPUtility.ConcatUrls("Lists", titleOrUrlOrResourceString)));
                    }
                    catch (FileNotFoundException)
                    {
                        // ignore exception, we need to try a third attempt that assumes the string parameter represents a resource string
                    }
                }

                if (list == null)
                {
                    // finally, try to handle the name as a resource key string
                    string[] resourceStringSplit = titleOrUrlOrResourceString.Split(',');
                    string nameFromResourceString = string.Empty;

                    if (resourceStringSplit.Length > 1)
                    {
                        // We're dealing with a resource string which looks like this: $Resources:Some.Namespace,Resource_Key
                        string resourceFileName = resourceStringSplit[0].Replace("$Resources:", string.Empty);
                        nameFromResourceString = this.resources.Find(resourceFileName, resourceStringSplit[1], web.UICulture.LCID);
                    }
                    else
                    {
                        // let's try to find a resource with that string directly as key
                        nameFromResourceString = this.resources.Find(titleOrUrlOrResourceString, web.UICulture.LCID);
                    }

                    if (!string.IsNullOrEmpty(nameFromResourceString))
                    {
                        list = web.Lists.TryGetList(nameFromResourceString);
                    }
                }
            }

            return list;
        }
    }
}
