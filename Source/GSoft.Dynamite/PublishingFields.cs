using System;
using GSoft.Dynamite.Definitions;
using Microsoft.SharePoint;
using GSoft.Dynamite.FieldTypes;

namespace GSoft.Dynamite
{
    /// <summary>
    /// Site column constants for Publishing (OOTB) content types
    /// </summary>
    public static class PublishingFields
    {
        #region Name

        /// <summary>
        /// ModerationStatus field internal name
        /// </summary>
        public const string ModerationStatusName = "_ModerationStatus";

        /// <summary>
        /// PublishingPageContent field internal name
        /// </summary>
        public const string PublishingPageContentName = "PublishingPageContent";

        /// <summary>
        /// PublishingPageLayout field internal name
        /// </summary>
        public const string PublishingPageLayoutName = "PublishingPageLayout";

        /// <summary>
        /// PublishingStartDate field internal name
        /// </summary>
        public const string PublishingStartDateName = "PublishingStartDate";

        /// <summary>
        /// PublishingEndDate field internal name
        /// </summary>
        public const string PublishingEndDateName = "PublishingExpirationDate";

        /// <summary>
        /// ArticleStartDate field internal name
        /// </summary>
        public const string ArticleStartDateName = "ArticleStartDate";

        /// <summary>
        /// Reusable Content AutomaticUpdate name
        /// </summary>
        public const string AutomaticUpdateName = "AutomaticUpdate";

        /// <summary>
        /// Reusable Content ShowInRibbon name
        /// </summary>
        public const string ShowInRibbonName = "ShowInRibbon";

        /// <summary>
        /// Reusable Content ReusableHtml name
        /// </summary>
        public const string ReusableHtmlName = "ReusableHtml";

        /// <summary>
        /// Reusable Content ContentCategory name
        /// </summary>
        public const string ContentCategoryName = "ContentCategory";

        /// <summary>
        /// PublishingContact name
        /// </summary>
        public const string PublishingContactName = "PublishingContact";        

        #endregion

        #region FieldInfo

        /// <summary>
        /// ModerationStatus field info
        /// </summary>
        public static readonly IFieldInfo ModerationStatus = new MinimalFieldInfo(ModerationStatusName, SPBuiltInFieldId._ModerationStatus);       // TODO: turn into ChoiceFieldInfo
       
        /// <summary>
        /// PublishingPageContent field info
        /// </summary>
        public static readonly IFieldInfo PublishingPageContent = new MinimalFieldInfo(PublishingPageContentName, new Guid("f55c4d88-1f2e-4ad9-aaa8-819af4ee7ee8"));

        /// <summary>
        /// PublishingPageLayout field info
        /// </summary>
        public static readonly IFieldInfo PublishingPageLayout = new MinimalFieldInfo(PublishingPageLayoutName, new Guid("0f800910-b30d-4c8f-b011-8189b2297094"));

        /// <summary>
        /// PublishingStartDate field info
        /// </summary>
        public static readonly IFieldInfo PublishingStartDate = new MinimalFieldInfo(PublishingStartDateName, new Guid("51d39414-03dc-4bd0-b777-d3e20cb350f7"));   // TODO: turn into DateFieldInfo or DateTimeFieldInfo

        /// <summary>
        /// PublishingEndDate field info
        /// </summary>
        public static readonly IFieldInfo PublishingEndDate = new MinimalFieldInfo(PublishingEndDateName, new Guid("a990e64f-faa3-49c1-aafa-885fda79de62"));   // TODO: turn into DateFieldInfo or DateTimeFieldInfo

        /// <summary>
        /// ArticleStartDate field info
        /// </summary>
        public static readonly IFieldInfo ArticleStartDate = new MinimalFieldInfo(ArticleStartDateName, new Guid("71316cea-40a0-49f3-8659-f0cefdbdbd4f"));     // TODO: turn into DateFieldInfo

        /// <summary>
        /// Reusable Content AutomaticUpdate name
        /// </summary>
        public static readonly IFieldInfo AutomaticUpdate = new MinimalFieldInfo(AutomaticUpdateName, new Guid("e977ed93-da24-4fcc-b77d-ac34eea7288f"));   // TODO: turn into BooleanFieldInfo

        /// <summary>
        /// Reusable Content ShowInRibbon name
        /// </summary>
        public static readonly IFieldInfo ShowInRibbon = new MinimalFieldInfo(ShowInRibbonName, new Guid("32e03f99-6949-466a-a4a6-057c21d4b516"));   // TODO: turn into BooleanFieldInfo

        /// <summary>
        /// Reusable Content ReusableHtml name
        /// </summary>
        public static readonly IFieldInfo ReusableHtml = new MinimalFieldInfo(ReusableHtmlName, new Guid("82dd22bf-433e-4260-b26e-5b8360dd9105"));

        /// <summary>
        /// Reusable Content ContentCategory name
        /// </summary>
        public static readonly IFieldInfo ContentCategory = new MinimalFieldInfo(ContentCategoryName, new Guid("3a4b7f98-8d14-4800-8bf5-9ad1dd6a82ee"));   // TODO: turninto ChoiceFieldInfo

        /// <summary>
        /// PublishingContact field info
        /// </summary>
        public static readonly IFieldInfo PublishingContact = new MinimalFieldInfo(PublishingContactName, new Guid("aea1a4dd-0f19-417d-8721-95a1d28762ab"));   // TODO: turn into UserFieldInfo

        #endregion
    }
}
