using System;

using Microsoft.Office.Server.Search.Administration;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Class to hold the constant for the already existing managed properties
    /// </summary>
    public static class BuiltInManagedProperties
    {
        /// <summary>
        /// The "Path" of the item is in reality his url
        /// </summary>
        public static ManagedPropertyInfo Url
        {
            get { return new ManagedPropertyInfo("Path", ManagedDataType.Text); }
        }

        /// <summary>
        /// The "ListId" of the item
        /// </summary>
        public static ManagedPropertyInfo ListId
        {
            get { return new ManagedPropertyInfo("ListID", ManagedDataType.Text); }
        }

        /// <summary>
        /// The "SiteUrl" managed property
        /// </summary>
        public static ManagedPropertyInfo SiteUrl
        {
            get { return new ManagedPropertyInfo("spSiteUrl", ManagedDataType.Text); }
        }

        /// <summary>
        /// Managed properties for the ArticleStartDate field
        /// </summary>
        public static ManagedPropertyInfo ArticleStartDate
        {
            get { return new ManagedPropertyInfo("ArticleStartDateOWSDATE", ManagedDataType.Text); }
        }

        /// <summary>
        /// Managed properties for the Meta Description field
        /// </summary>
        public static ManagedPropertyInfo MetaDescription
        {
            get { return new ManagedPropertyInfo("SeoMetaDescriptionOWSTEXT", ManagedDataType.Text); }
        }

        /// <summary>
        /// The OOTB ListItemId managed property
        /// </summary>
        public static ManagedPropertyInfo ListItemId
        {
            get { return new ManagedPropertyInfo("ListItemID", ManagedDataType.Text); }
        }

        /// <summary>
        /// The OOTB Content Type managed property
        /// </summary>
        public static ManagedPropertyInfo ContentType
        {
            get { return new ManagedPropertyInfo("ContentType", ManagedDataType.Text); }
        }

        /// <summary>
        /// The OOTB ContentTypeId managed property
        /// </summary>
        public static ManagedPropertyInfo ContentTypeId
        {
            get { return new ManagedPropertyInfo("ContentTypeId", ManagedDataType.Text); }
        }

        /// <summary>
        /// The OOTB Title managed property
        /// </summary>
        public static ManagedPropertyInfo Title
        {
            get { return new ManagedPropertyInfo("Title", ManagedDataType.Text); }
        }

        /// <summary>
        /// The OOTB PublishingPageContent managed property
        /// </summary>
        public static ManagedPropertyInfo PublishingPageContent
        {
            get { return new ManagedPropertyInfo("PublishingPageContentOWSHTML", ManagedDataType.Text); }
        }

        /// <summary>
        /// The OOTB PublishingImage managed property
        /// </summary>
        public static ManagedPropertyInfo PublishingImage
        {
            get { return new ManagedPropertyInfo("PublishingImage", ManagedDataType.Text); }
        }

        /// <summary>
        /// The OOTB Filename managed property
        /// </summary>
        public static ManagedPropertyInfo FileName
        {
            get { return new ManagedPropertyInfo("Filename", ManagedDataType.Text); }
        }

        /// <summary>
        /// The OOTB FileExtension managed property
        /// </summary>
        public static ManagedPropertyInfo FileExtension
        {
            get { return new ManagedPropertyInfo("FileExtension", ManagedDataType.Text); }
        }

        /// <summary>
        /// The OOTB SecondaryFileExtension managed property
        /// </summary>
        public static ManagedPropertyInfo SecondaryFileExtension
        {
            get { return new ManagedPropertyInfo("SecondaryFileExtension", ManagedDataType.Text); }
        }

        /// <summary>
        /// The OOTB Rank managed property
        /// </summary>
        public static ManagedPropertyInfo Rank
        {
            get { return new ManagedPropertyInfo("Rank", ManagedDataType.Integer); }
        }
    }
}
