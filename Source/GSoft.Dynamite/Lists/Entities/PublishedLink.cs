using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Lists.Entities
{
    /// <summary>
    /// Element in a PublishedLinks list
    /// </summary>
    public class PublishedLink : BaseEntity
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public PublishedLink()
        {
            // Init fields
            this.PublishedLinksPath = new UrlValue();
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="title">The title of the item</param>
        /// <param name="description">The description of the published link</param>
        /// <param name="url">The url of the link</param>
        /// <param name="urlDescription">The description of the url</param>
        public PublishedLink(string title, string description, string url, string urlDescription)
            : this()
        {
            this.Title = title;
            this.PublishedLinksDescription = description;
            this.PublishedLinksPath.Url = url;
            this.PublishedLinksPath.Description = urlDescription;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Property]
        public string PublishedLinksDescription { get; set; }

        /// <summary>
        /// Gets or sets published links url
        /// </summary>
        /// <value>
        /// The display order.
        /// </value>
        [Property(BuiltInFields.PublishedLinksUrlName)]
        public UrlValue PublishedLinksPath { get; set; }
    }
}
