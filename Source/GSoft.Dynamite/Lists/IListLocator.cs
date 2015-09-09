using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// List locator is a helper to help find a SPList object.
    /// </summary>
    public interface IListLocator
    {
        /// <summary>
        /// Find a list by its web-relative url
        /// </summary>
        /// <param name="web">The context's web</param>
        /// <param name="listUrl">The web-relative path to the list</param>
        /// <returns>The list</returns>
        SPList GetByUrl(SPWeb web, Uri listUrl);

        /// <summary>
        /// Find a list by its web-relative url
        /// </summary>
        /// <param name="web">The context's web</param>
        /// <param name="listUrl">The web-relative path to the list</param>
        /// <returns>The list</returns>
        SPList GetByUrl(SPWeb web, string listUrl);

        /// <summary>
        /// Find a list by its name's resource key
        /// </summary>
        /// <param name="web">The context's web</param>
        /// <param name="listNameResourceKey">The web-relative path to the list</param>
        /// <returns>The list</returns>
        SPList GetByNameResourceKey(SPWeb web, string listNameResourceKey);

        /// <summary>
        /// Method to find a list with the ListInfo
        /// </summary>
        /// <param name="web">The Web where to find the list</param>
        /// <param name="listInfo">The ListInfo of the list to locate</param>
        /// <returns>A SharePoint SPList</returns>
        SPList TryGetList(SPWeb web, ListInfo listInfo);

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
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "The point of this method is to be lax and accept many types of input strings.")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string", Justification = "Parameter name descriptions the many types of allowed inputs")]
        SPList TryGetList(SPWeb web, string titleOrUrlOrResourceString);
    }
}
