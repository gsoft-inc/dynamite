using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Lists.Entities;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// Use to create Published Links in a publishing site
    /// </summary>
    public class PublishedLinksEditor : IPublishedLinksEditor
    {
        private readonly IListLocator listLocator;
        private readonly ISharePointEntityBinder entityBinder;

        /// <summary>
        /// Initializes a new instance of <see cref="PublishedLinksEditor"/>
        /// </summary>
        /// <param name="listLocator">The list locator</param>
        /// <param name="entityBinder">The entity binder</param>
        public PublishedLinksEditor(IListLocator listLocator, ISharePointEntityBinder entityBinder)
        {
            this.listLocator = listLocator;
            this.entityBinder = entityBinder;
        }

        /// <summary>
        /// Method to create if not exist the publishing link in a Publishing link list of the site
        /// </summary>
        /// <param name="site">The current Site to create the publishing link.</param>
        /// <param name="publishedLink">The publishing link to create</param>
        public void EnsurePublishedLinks(SPSite site, PublishedLink publishedLink)
        {
            var publishedLinksList = this.listLocator.TryGetList(site.RootWeb, "/PublishedLinks");

            if (publishedLinksList != null && !publishedLinksList.Items.Cast<SPListItem>().Any(link => link.Title == publishedLink.Title))
            {
                var item = publishedLinksList.Items.Add();
                this.entityBinder.FromEntity(publishedLink, item);

                item.Update();
            }
        }
    }
}
