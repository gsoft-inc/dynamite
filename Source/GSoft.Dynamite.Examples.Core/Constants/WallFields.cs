using System;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Utils;

namespace GSoft.Dynamite.Examples.Core.Constants
{
    /// <summary>
    /// Field constants for Wall content types.
    /// </summary>
    public static class WallFields
    {
        /// <summary>
        /// WallTextContent field info.
        /// </summary>
        public static readonly FieldInfo TextContent = new FieldInfo(TextContentName, new Guid("AB0361DA-546E-455C-B403-AB6B5E3ADB8E"));

        /// <summary>
        /// WallTags field info.
        /// </summary>
        public static readonly FieldInfo Tags = new FieldInfo(TagsName, new Guid("0763B2D8-D7DF-409C-94D6-77DD6BE0EFE9"));

        /// <summary>
        /// WallAuthor field info.
        /// </summary>
        public static readonly FieldInfo Author = new FieldInfo(AuthorName, new Guid("274117CC-5CCF-458D-8589-9ADBD78BA61A"));

        /// <summary>
        /// WallPostLookup field info.
        /// </summary>
        public static readonly FieldInfo PostLookup = new FieldInfo(PostLookupName, new Guid("3BB9F38C-F375-4184-B4F5-8D232DD8BB5A"));

        /// <summary>
        /// WallTagsTaxHTField0 field info.
        /// </summary>
        public static readonly FieldInfo TagsTaxHT = new FieldInfo(TagsName, new Guid("FED69579-2A42-4E28-96CE-A576F9E7B899"));

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
        /// WallTextContent field name.
        /// </summary>
        internal const string TextContentName = "WallTextContent";

        /// <summary>
        /// WallTags field name.
        /// </summary>
        internal const string TagsName = "WallTags";

        /// <summary>
        /// WallAuthor field name.
        /// </summary>
        internal const string AuthorName = "WallAuthor";

        /// <summary>
        /// WallPostLookup field name.
        /// </summary>
        internal const string PostLookupName = "WallPostLookup";

        /// <summary>
        /// WallTagsTaxHTField0 field name.
        /// </summary>
        internal const string TagsTaxHTName = "WallTagsTaxHTField0";

        /// <summary>
        /// TaxCatchAll field name.
        /// </summary>
        internal const string TaxCatchAllName = "TaxCatchAll";

        /// <summary>
        /// TaxCatchAllLabel field name.
        /// </summary>
        internal const string TaxCatchAllLabelName = "TaxCatchAllLabel";
    }
}
