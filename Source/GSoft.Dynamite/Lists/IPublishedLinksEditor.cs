using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Lists.Entities;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// Use to create Published Links in a publishing site
    /// </summary>
    public interface IPublishedLinksEditor
    {
        /// <summary>
        /// Method to create if not exist the publishing link in a Publishing link list of the site
        /// </summary>
        /// <param name="site">The current Site to create the publishing link.</param>
        /// <param name="publishedLink">The publishing link to create</param>
        void EnsurePublishedLinks(SPSite site, PublishedLink publishedLink);
    }
}
