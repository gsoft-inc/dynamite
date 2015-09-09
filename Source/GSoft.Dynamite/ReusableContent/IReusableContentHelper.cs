using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ReusableContent
{
    /// <summary>
    /// Contract on the Helper for the Reusable Content
    /// </summary>
    public interface IReusableContentHelper
    {
        /// <summary>
        /// Gets the reusable content by title.
        /// </summary>
        /// <param name="site">The Site Collection.</param>
        /// <param name="reusableContentTitle">The reusable content title.</param>
        /// <returns>The reusable content</returns>
        ReusableContentInfo GetByTitle(SPSite site, string reusableContentTitle);

        /// <summary>
        /// Method to get all available Reusable Content Titles
        /// </summary>
        /// <param name="site">The current Site collection context</param>
        /// <returns>A list of string (reusable content title) or null.</returns>
        IList<string> GetAllReusableContentTitles(SPSite site);

        /// <summary>
        /// Method to ensure (create if not exist) and update a reusable content in a specific site.
        /// </summary>
        /// <param name="site">The Site Collection to ensure the reusablec content</param>
        /// <param name="reusableContents">The information on the reusable contents to ensure</param>
        void EnsureReusableContent(SPSite site, IList<ReusableContentInfo> reusableContents);
    }
}
