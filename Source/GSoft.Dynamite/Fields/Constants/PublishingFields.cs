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

        /// <summary>
        /// BrowserTitle name
        /// </summary>
        public const string BrowserTitleName = "SeoBrowserTitle";

        /// <summary>
        /// MetaDescription name
        /// </summary>
        public const string MetaDescriptionName = "SeoMetaDescription";

        /// <summary>
        /// MetaKeywords name
        /// </summary>
        public const string MetaKeywordsName = "SeoKeywords";

        /// <summary>
        /// HideFromInternetSearchEngines name
        /// </summary>
        public const string HideFromInternetSearchEnginesName = "RobotsNoIndex";

        #endregion

        #region FieldInfo

        /// <summary>
        /// ModerationStatus field info (OOTB type = ModStat)
        /// </summary>
        public static BaseFieldInfo ModerationStatus 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(ModerationStatusName, SPBuiltInFieldId._ModerationStatus); 
            } 
        }
       
        /// <summary>
        /// PublishingPageContent field info (OOTB type = HTML)
        /// </summary>
        public static BaseFieldInfo PublishingPageContent 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(PublishingPageContentName, new Guid("f55c4d88-1f2e-4ad9-aaa8-819af4ee7ee8")); 
            } 
        }

        /// <summary>
        /// PublishingPageLayout field info (OOTB type = URL)
        /// </summary>
        public static BaseFieldInfo PublishingPageLayout 
        { 
            get 
            { 
                return new MinimalFieldInfo<UrlValue>(PublishingPageLayoutName, new Guid("0f800910-b30d-4c8f-b011-8189b2297094")); 
            } 
        }

        /// <summary>
        /// PublishingStartDate field info (OOTB type = DateTime)
        /// </summary>
        public static BaseFieldInfo PublishingStartDate 
        { 
            get 
            { 
                return new MinimalFieldInfo<DateTime?>(PublishingStartDateName, new Guid("51d39414-03dc-4bd0-b777-d3e20cb350f7")); 
            } 
        }

        /// <summary>
        /// PublishingEndDate field info (OOTB type = DateTime)
        /// </summary>
        public static BaseFieldInfo PublishingEndDate 
        { 
            get 
            { 
                return new MinimalFieldInfo<DateTime?>(PublishingEndDateName, new Guid("a990e64f-faa3-49c1-aafa-885fda79de62")); 
            } 
        }

        /// <summary>
        /// ArticleStartDate field info (OOTB type = DateTime, Format = DateOnly)
        /// </summary>
        public static BaseFieldInfo ArticleStartDate 
        { 
            get 
            { 
                return new MinimalFieldInfo<DateTime?>(ArticleStartDateName, new Guid("71316cea-40a0-49f3-8659-f0cefdbdbd4f")); 
            } 
        }

        /// <summary>
        /// Reusable Content AutomaticUpdate field (OOTB type = Boolean)
        /// </summary>
        public static BaseFieldInfo AutomaticUpdate 
        { 
            get 
            { 
                return new MinimalFieldInfo<DateTime?>(AutomaticUpdateName, new Guid("e977ed93-da24-4fcc-b77d-ac34eea7288f")); 
            } 
        }

        /// <summary>
        /// Reusable Content ShowInRibbon name (OOTB type = Boolean)
        /// </summary>
        public static BaseFieldInfo ShowInRibbon 
        { 
            get 
            { 
                return new MinimalFieldInfo<bool?>(ShowInRibbonName, new Guid("32e03f99-6949-466a-a4a6-057c21d4b516")); 
            } 
        }

        /// <summary>
        /// Reusable Content ReusableHtml name (OOTB type = HTML)
        /// </summary>
        public static BaseFieldInfo ReusableHtml 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(ReusableHtmlName, new Guid("82dd22bf-433e-4260-b26e-5b8360dd9105")); 
            } 
        }

        /// <summary>
        /// Reusable Content ContentCategory name (OOTB type = Choice)
        /// </summary>
        public static BaseFieldInfo ContentCategory 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(ContentCategoryName, new Guid("3a4b7f98-8d14-4800-8bf5-9ad1dd6a82ee")); 
            } 
        }

        /// <summary>
        /// PublishingContact field info (OOTB type = User)
        /// </summary>
        public static BaseFieldInfo PublishingContact 
        { 
            get 
            { 
                return new MinimalFieldInfo<UserValue>(PublishingContactName, new Guid("aea1a4dd-0f19-417d-8721-95a1d28762ab")); 
            } 
        }

        /// <summary>
        /// PublishingPageImage field info (OOTB type = Image)
        /// </summary>
        public static BaseFieldInfo PublishingPageImage 
        { 
            get 
            { 
                return new MinimalFieldInfo<ImageValue>(PublishingPageImageName, new Guid("{3DE94B06-4120-41A5-B907-88773E493458}")); 
            } 
        }

        /// <summary>
        /// BrowserTitle field info (OOTB type = string)
        /// </summary>
        public static BaseFieldInfo BrowserTitle
        {
            get
            {
                return new MinimalFieldInfo<string>(BrowserTitleName, new Guid("{ff92f929-d18b-46d4-9879-521378c689ef}"));
            }
        }

        /// <summary>
        /// MetaDescription field info (OOTB type = string)
        /// </summary>
        public static BaseFieldInfo MetaDescription
        {
            get
            {
                return new MinimalFieldInfo<string>(MetaDescriptionName, new Guid("{d83897e5-2430-4df7-8e5a-9bc06c664992}"));
            }
        }

        /// <summary>
        /// MetaKeywords field info (OOTB type = string)
        /// </summary>
        public static BaseFieldInfo MetaKeywords
        {
            get
            {
                return new MinimalFieldInfo<string>(MetaKeywordsName, new Guid("{45ae2169-585c-440b-aa4c-1d5e981fbbe5}"));
            }
        }

        /// <summary>
        /// HideFromInternetSearchEngines field info (OOTB type = string)
        /// </summary>
        public static BaseFieldInfo HideFromInternetSearchEngines
        {
            get
            {
                return new MinimalFieldInfo<bool?>(HideFromInternetSearchEnginesName, new Guid("{325c00dd-fd91-468b-81cf-5bb9951abba1}"));
            }
        }

        #endregion
    }
}
