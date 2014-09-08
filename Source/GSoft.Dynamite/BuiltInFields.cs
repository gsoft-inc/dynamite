using System;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Definitions;
using Microsoft.SharePoint;

namespace GSoft.Dynamite
{
    /// <summary>
    /// Site columns constants for built-in (OOTB) content types
    /// </summary>
    public static class BuiltInFields
    {
        #region Name

        /// <summary>
        /// Title field internal name
        /// </summary>
        public const string TitleName = "Title";

        /// <summary>
        /// FileRef (i.e. File Url) field internal name
        /// </summary>
        public const string FileRefName = "FileRef";

        /// <summary>
        /// FileLeafRef (i.e. DocumentName) field internal name
        /// </summary>
        public const string FileLeafRefName = "FileLeafRef";

        /// <summary>
        /// The URL field internal name
        /// </summary>
        public const string UrlName = "URL";

        /// <summary>
        /// ContentType field internal name
        /// </summary>
        public const string ContentTypeName = "ContentType";

        /// <summary>
        /// ContentTypeId field internal name
        /// </summary>
        public const string ContentTypeIdName = "ContentTypeId";

        /// <summary>
        /// The publishing page content name
        /// </summary>
        public const string PublishingPageContentName = "PublishingPageContent";

        /// <summary>
        /// The comments note field name
        /// </summary>
        public const string CommentsName = "Comments";

        /// <summary>
        /// The publishing start date field name
        /// </summary>
        public const string PublishingStartDateName = "PublishingStartDate";

        /// <summary>
        /// The publishing expiration date field name
        /// </summary>
        public const string PublishingExpirationDateName = "PublishingExpirationDate";

        /// <summary>
        /// The publishing contact field name
        /// </summary>
        public const string PublishingContactName = "PublishingContact";

        /// <summary>
        /// The publishing contact email field name
        /// </summary>
        public const string PublishingContactEmailName = "PublishingContactEmail";

        /// <summary>
        /// The publishing contact picture field name
        /// </summary>
        public const string PublishingContactPictureName = "PublishingContactPicture";

        /// <summary>
        /// The publishing rollup image field name
        /// </summary>
        public const string PublishingRollupImageName = "PublishingRollupImage";

        /// <summary>
        /// TaxCatchAll field name.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "Mean Catch All, not Catchall")]
        public const string TaxCatchAllName = "TaxCatchAll";

        /// <summary>
        /// TaxCatchAllLabel field name.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "Mean Catch All, not Catchall")]
        public const string TaxCatchAllLabelName = "TaxCatchAllLabel";

        #endregion

        #region FieldInfo

        /// <summary>
        /// Title field info
        /// </summary>
        public static readonly FieldInfo Title = new FieldInfo(TitleName, SPBuiltInFieldId.Title);

        /// <summary>
        /// FileRef (i.e. File Url) field info
        /// </summary>
        public static readonly FieldInfo FileRef = new FieldInfo(FileRefName, SPBuiltInFieldId.FileRef);

        /// <summary>
        /// FileLeafRef (i.e. Document Name) field info
        /// </summary>
        public static readonly FieldInfo FileLeafRef = new FieldInfo(FileLeafRefName, SPBuiltInFieldId.FileLeafRef);

        /// <summary>
        /// ContentType field info
        /// </summary>
        public static readonly FieldInfo ContentType = new FieldInfo(ContentTypeName, SPBuiltInFieldId.ContentType);

        /// <summary>
        /// ContentTypeId field info
        /// </summary>
        public static readonly FieldInfo ContentTypeId = new FieldInfo(ContentTypeIdName, SPBuiltInFieldId.ContentTypeId);

        /// <summary>
        /// URL field info
        /// </summary>
        public static readonly FieldInfo Url = new FieldInfo(UrlName, SPBuiltInFieldId.URL);

        /// <summary>
        /// TaxCatchAll field info.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "This is the actual SharePoint field name")]
        public static readonly FieldInfo TaxCatchAll = new FieldInfo(TaxCatchAllName, new Guid("f3b0adf9-c1a2-4b02-920d-943fba4b3611"));

        /// <summary>
        /// TaxCatchAllLabel field info.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "This is the actual SharePoint field name")]
        public static readonly FieldInfo TaxCatchAllLabel = new FieldInfo(TaxCatchAllLabelName, new Guid("8f6b6dd8-9357-4019-8172-966fcd502ed2"));

        /// <summary>
        /// The publishing page content field info.
        /// </summary>
        public static readonly FieldInfo PublishingPageContent = new FieldInfo(PublishingPageContentName, new Guid("f55c4d88-1f2e-4ad9-aaa8-819af4ee7ee8"));

        /// <summary>
        /// The comments field info.
        /// </summary>
        public static readonly FieldInfo Comments = new FieldInfo(CommentsName, SPBuiltInFieldId.Comments);

        /// <summary>
        /// The publishing start date field info.
        /// </summary>
        public static readonly FieldInfo PublishingStartDate = new FieldInfo(PublishingStartDateName, new Guid("51d39414-03dc-4bd0-b777-d3e20cb350f7"));

        /// <summary>
        /// The publishing expiration date field info.
        /// </summary>
        public static readonly FieldInfo PublishingExpirationDate = new FieldInfo(PublishingExpirationDateName, new Guid("a990e64f-faa3-49c1-aafa-885fda79de62"));

        /// <summary>
        /// The publishing contact field info.
        /// </summary>
        public static readonly FieldInfo PublishingContact = new FieldInfo(PublishingContactName, new Guid("aea1a4dd-0f19-417d-8721-95a1d28762ab"));

        /// <summary>
        /// The publishing contact email field info.
        /// </summary>
        public static readonly FieldInfo PublishingContactEmail = new FieldInfo(PublishingContactEmailName, new Guid("c79dba91-e60b-400e-973d-c6d06f192720"));

        /// <summary>
        /// The publishing contact picture field info.
        /// </summary>
        public static readonly FieldInfo PublishingContactPicture = new FieldInfo(PublishingContactPictureName, new Guid("dc47d55f-9bf9-494a-8d5b-e619214dd19a"));

        /// <summary>
        /// The publishing rollup image field info.
        /// </summary>
        public static readonly FieldInfo PublishingRollupImage = new FieldInfo(PublishingRollupImageName, new Guid("543bc2cf-1f30-488e-8f25-6fe3b689d9ac"));

        #endregion
    }
}
