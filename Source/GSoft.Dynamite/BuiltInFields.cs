using System;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Utils;

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
        /// ContentType field internal name
        /// </summary>
        public const string ContentTypeName = "ContentType";

        /// <summary>
        /// ContentTypeId field internal name
        /// </summary>
        public const string ContentTypeIdName = "ContentTypeId";

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
        /// TaxCatchAll field info.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "This is the actual SharePoint field name")]
        public static readonly FieldInfo TaxCatchAll = new FieldInfo(TaxCatchAllName, new Guid("f3b0adf9-c1a2-4b02-920d-943fba4b3611"));

        /// <summary>
        /// TaxCatchAllLabel field info.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "This is the actual SharePoint field name")]
        public static readonly FieldInfo TaxCatchAllLabel = new FieldInfo(TaxCatchAllLabelName, new Guid("8f6b6dd8-9357-4019-8172-966fcd502ed2"));

        #endregion
    }
}
