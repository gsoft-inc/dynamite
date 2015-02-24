using System;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Fields.Constants
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

        /// <summary>
        /// PublishingPageImage name
        /// </summary>
        public const string PublishingPageImageName = "PublishingPageImage";

        #endregion

        #region FieldInfo

        /// <summary>
        /// ModerationStatus field info (OOTB type = ModStat)
        /// </summary>
        public static IFieldInfo ModerationStatus 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(ModerationStatusName, SPBuiltInFieldId._ModerationStatus); 
            } 
        }
       
        /// <summary>
        /// PublishingPageContent field info (OOTB type = HTML)
        /// </summary>
        public static IFieldInfo PublishingPageContent 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(PublishingPageContentName, new Guid("f55c4d88-1f2e-4ad9-aaa8-819af4ee7ee8")); 
            } 
        }

        /// <summary>
        /// PublishingPageLayout field info (OOTB type = URL)
        /// </summary>
        public static IFieldInfo PublishingPageLayout 
        { 
            get 
            { 
                return new MinimalFieldInfo<UrlValue>(PublishingPageLayoutName, new Guid("0f800910-b30d-4c8f-b011-8189b2297094")); 
            } 
        }

        /// <summary>
        /// PublishingStartDate field info (OOTB type = DateTime)
        /// </summary>
        public static IFieldInfo PublishingStartDate 
        { 
            get 
            { 
                return new MinimalFieldInfo<DateTime?>(PublishingStartDateName, new Guid("51d39414-03dc-4bd0-b777-d3e20cb350f7")); 
            } 
        }

        /// <summary>
        /// PublishingEndDate field info (OOTB type = DateTime)
        /// </summary>
        public static IFieldInfo PublishingEndDate 
        { 
            get 
            { 
                return new MinimalFieldInfo<DateTime?>(PublishingEndDateName, new Guid("a990e64f-faa3-49c1-aafa-885fda79de62")); 
            } 
        }

        /// <summary>
        /// ArticleStartDate field info (OOTB type = DateTime, Format = DateOnly)
        /// </summary>
        public static IFieldInfo ArticleStartDate 
        { 
            get 
            { 
                return new MinimalFieldInfo<DateTime?>(ArticleStartDateName, new Guid("71316cea-40a0-49f3-8659-f0cefdbdbd4f")); 
            } 
        }

        /// <summary>
        /// Reusable Content AutomaticUpdate field (OOTB type = Boolean)
        /// </summary>
        public static IFieldInfo AutomaticUpdate 
        { 
            get 
            { 
                return new MinimalFieldInfo<DateTime?>(AutomaticUpdateName, new Guid("e977ed93-da24-4fcc-b77d-ac34eea7288f")); 
            } 
        }

        /// <summary>
        /// Reusable Content ShowInRibbon name (OOTB type = Boolean)
        /// </summary>
        public static IFieldInfo ShowInRibbon 
        { 
            get 
            { 
                return new MinimalFieldInfo<bool?>(ShowInRibbonName, new Guid("32e03f99-6949-466a-a4a6-057c21d4b516")); 
            } 
        }

        /// <summary>
        /// Reusable Content ReusableHtml name (OOTB type = HTML)
        /// </summary>
        public static IFieldInfo ReusableHtml 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(ReusableHtmlName, new Guid("82dd22bf-433e-4260-b26e-5b8360dd9105")); 
            } 
        }

        /// <summary>
        /// Reusable Content ContentCategory name (OOTB type = Choice)
        /// </summary>
        public static IFieldInfo ContentCategory 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(ContentCategoryName, new Guid("3a4b7f98-8d14-4800-8bf5-9ad1dd6a82ee")); 
            } 
        }

        /// <summary>
        /// PublishingContact field info (OOTB type = User)
        /// </summary>
        public static IFieldInfo PublishingContact 
        { 
            get 
            { 
                return new MinimalFieldInfo<UserValue>(PublishingContactName, new Guid("aea1a4dd-0f19-417d-8721-95a1d28762ab")); 
            } 
        }

        /// <summary>
        /// PublishingPageImage field info (OOTB type = Image)
        /// </summary>
        public static IFieldInfo PublishingPageImage 
        { 
            get 
            { 
                return new MinimalFieldInfo<ImageValue>(PublishingPageImageName, new Guid("{3DE94B06-4120-41A5-B907-88773E493458}")); 
            } 
        }

        #endregion
    }
}
