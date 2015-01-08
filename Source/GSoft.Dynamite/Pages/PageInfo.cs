using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.WebParts;

namespace GSoft.Dynamite.Pages
{
    /// <summary>
    /// Definition of a publishing page
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// Default PageInfo constructor for serialization purposes
        /// </summary>
        public PageInfo()
        {
            this.WebParts = new List<WebPartInfo>();
            this.FieldValues = new List<FieldValueInfo>();
        }

        /// <summary>
        /// Name of the file for the page without the ".aspx" extension.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Title of the page
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The page layout of the page
        /// </summary>
        public PageLayoutInfo PageLayout { get; set; }

        /// <summary>
        /// The content type of the page
        /// </summary>
        public string ContentTypeId { get; set; }

        /// <summary>
        /// WebParts by zone
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow replacement of backing store collection for more flexible object initialization.")]
        public ICollection<WebPartInfo> WebParts { get; set; }

        /// <summary>
        /// Get the site relative url to use in term driven page setting
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "URL has a '~site/full/path/here' tokenized format and needs to be used as a string.")]
        public string SiteTokenizedTermDrivenPageUrl
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "~site/{0}", this.LibraryRelativePageUrl.ToString());
            }
        }

        /// <summary>
        /// Get the site relative url to use in term driven page setting
        /// </summary>
        public Uri LibraryRelativePageUrl
        {
            get
            {
                return new Uri("Pages/" + this.FileName + ".aspx", UriKind.Relative);
            }
        }

        /// <summary>
        /// Are we publishing this page or not ?
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// The values to be assigned to the page when its created.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow replacement of backing store collection for more flexible object initialization.")]
        public IList<FieldValueInfo> FieldValues { get; set; }
    }
}