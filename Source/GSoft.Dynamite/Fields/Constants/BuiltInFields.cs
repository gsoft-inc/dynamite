using System;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Fields.Constants
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
        /// Linked to item with edit menu
        /// </summary>
        public const string TitleLinkName = "LinkTitle";

        /// <summary>
        /// Linked to item with no edit menu
        /// </summary>
        public const string TitleLinkNoMenuName = "LinkTitleNoMenu";

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
        /// TaxCatchAll field name.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "Mean Catch All, not Catchall")]
        public const string TaxCatchAllName = "TaxCatchAll";

        /// <summary>
        /// TaxCatchAllLabel field name.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "Mean Catch All, not Catchall")]
        public const string TaxCatchAllLabelName = "TaxCatchAllLabel";

        /// <summary>
        /// The assigned to field name
        /// </summary>
        public const string AssignedToName = "AssignedTo";

        /// <summary>
        /// The percent complete field name
        /// </summary>
        public const string PercentCompleteName = "PercentComplete";

        /// <summary>
        /// The predecessors field name
        /// </summary>
        public const string PredecessorsName = "Predecessors";

        /// <summary>
        /// The priority field name
        /// </summary>
        public const string PriorityName = "Priority";

        /// <summary>
        /// The task status field name
        /// </summary>
        public const string TaskStatusName = "TaskStatus";

        /// <summary>
        /// The home phone field name
        /// </summary>
        public const string HomePhoneName = "HomePhone";

        /// <summary>
        /// The work fax field name
        /// </summary>
        public const string WorkFaxName = "WorkFax";

        /// <summary>
        /// The work address field name
        /// </summary>
        public const string WorkAddressName = "WorkAddress";

        /// <summary>
        /// The work country field name
        /// </summary>
        public const string WorkCountryName = "WorkCountry";

        /// <summary>
        /// The work city field name
        /// </summary>
        public const string WorkCityName = "WorkCity";

        /// <summary>
        /// The work state field name
        /// </summary>
        public const string WorkStateName = "WorkState";

        /// <summary>
        /// The work zip field name
        /// </summary>
        public const string WorkZipName = "WorkZip";

        /// <summary>
        /// The web page field name
        /// </summary>
        public const string WebpageName = "WebPage";

        /// <summary>
        /// The comments field name
        /// </summary>
        public const string CommentsName = "Comments";

        /// <summary>
        /// The department field name
        /// </summary>
        public const string DepartmentName = "Department";

        /// <summary>
        /// The role field name
        /// </summary>
        public const string RoleName = "Role";

        /// <summary>
        /// The related items field name
        /// </summary>
        public const string RelatedItemsName = "RelatedItems";

        /// <summary>
        /// The display order field name
        /// </summary>
        public const string DisplayOrderName = "DisplayOrder";

        /// <summary>
        /// The master page URL field name
        /// </summary>
        public const string MasterPageUrlName = "MasterPageUrl";

        /// <summary>
        /// The theme URL field name
        /// </summary>
        public const string ThemeUrlName = "ThemeUrl";

        /// <summary>
        /// The image URL field name
        /// </summary>
        public const string ImageUrlName = "ImageUrl";

        /// <summary>
        /// The font scheme URL field name
        /// </summary>
        public const string FontSchemeUrlName = "FontSchemeUrl";

        /// <summary>
        /// The enterprise keywords field name
        /// </summary>
        public const string EnterpriseKeywordsName = "TaxKeyword";

        /// <summary>
        /// The cell phone field name
        /// </summary>
        public const string CellphoneName = "CellPhone";

        /// <summary>
        /// The full name field name
        /// </summary>
        public const string FullNameName = "FullName";

        /// <summary>
        /// The email field name
        /// </summary>
        public const string EmailName = "EMail";

        /// <summary>
        /// The first name field name
        /// </summary>
        public const string FirstNameName = "FirstName";

        /// <summary>
        /// The work phone field name
        /// </summary>
        public const string WorkPhoneName = "WorkPhone";

        /// <summary>
        /// The job title field name
        /// </summary>
        public const string JobTitleName = "JobTitle";

        /// <summary>
        /// The company field name
        /// </summary>
        public const string CompanyName = "Company";

        /// <summary>
        /// PublishingLinks list : the url of the publishing link
        /// </summary>
        public const string PublishedLinksUrlName = "PublishedLinksURL";

        /// <summary>
        /// Author : The creator of the item
        /// </summary>
        public const string AuthorName = "Author";

        /// <summary>
        /// Created : The date of creation of the item
        /// </summary>
        public const string CreatedName = "Created";

        /// <summary>
        /// Editor : The latest modifier of the item
        /// </summary>
        public const string EditorName = "Editor";

        /// <summary>
        /// Modified : The date of the last modification
        /// </summary>
        public const string ModifiedName = "Modified";

        /// <summary>
        /// Body: Note field containing rich text.
        /// </summary>
        public const string BodyName = "Body";

        /// <summary>
        /// Expires: Date and time field for content expiration.
        /// </summary>
        public const string ExpiresName = "Expires";

        /// <summary>
        /// DocIcon: The view field to display the document icon.
        /// </summary>
        public const string DocIconName = "DocIcon";

        /// <summary>
        /// LinkFilename: The view field to display the link to the file.
        /// </summary>
        public const string LinkFileNameName = "LinkFilename";

        /// <summary>
        /// CheckoutUser: The view field to display the user who checked out the file.
        /// </summary>
        public const string CheckoutUserName = "CheckoutUser";

        /// <summary>
        /// ModerationStatus: The approval status.
        /// </summary>
        public const string ModerationStatusName = "ModerationStatus";

        #endregion

        #region FieldInfo

        /// <summary>
        /// Title field info (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo Title
        {
            get 
            { 
                return new MinimalFieldInfo<string>(TitleName, SPBuiltInFieldId.Title); 
            }
        }

        /// <summary>
        /// Title linked to item with edit menu (OOTB type = Computed)
        /// </summary>
        public static BaseFieldInfo TitleLink
        { 
            get { return new MinimalFieldInfo<string>(TitleLinkName, SPBuiltInFieldId.LinkTitle); } 
        }

        /// <summary>
        /// Title linked to item with no edit menu (OOTB type = Computed)
        /// </summary>
        public static BaseFieldInfo TitleLinkNoMenu 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(TitleLinkNoMenuName, SPBuiltInFieldId.LinkTitleNoMenu); 
            } 
        }

        /// <summary>
        /// FileRef (i.e. File Url Path) field info (OOTB type = Lookup)
        /// </summary>
        public static BaseFieldInfo FileRef 
        { 
            get 
            { 
                return new MinimalFieldInfo<LookupValue>(FileRefName, SPBuiltInFieldId.FileRef); 
            } 
        }

        /// <summary>
        /// FileLeafRef (i.e. Document Name) field info (OOTB type = File)
        /// </summary>
        public static BaseFieldInfo FileLeafRef 
        { 
            get 
            {
                return new MinimalFieldInfo<LookupValue>(FileLeafRefName, SPBuiltInFieldId.FileLeafRef); 
            } 
        }

        /// <summary>
        /// ContentType field info (OOTB type = Computed)
        /// </summary>
        public static BaseFieldInfo ContentType 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(ContentTypeName, SPBuiltInFieldId.ContentType); 
            } 
        }

        /// <summary>
        /// ContentTypeId field info (OOTB type = ContentTypeId)
        /// </summary>
        public static BaseFieldInfo ContentTypeId 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(ContentTypeIdName, SPBuiltInFieldId.ContentTypeId); 
            } 
        }

        /// <summary>
        /// URL field info (OOTB type = URL)
        /// </summary>
        public static BaseFieldInfo Url 
        { 
            get 
            { 
                return new MinimalFieldInfo<UrlValue>(UrlName, SPBuiltInFieldId.URL); 
            } 
        }

        /// <summary>
        /// TaxCatchAll field info (OOTB type = LookupMulti)
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "This is the actual SharePoint field name")]
        public static BaseFieldInfo TaxCatchAll 
        { 
            get 
            { 
                return new MinimalFieldInfo<LookupValueCollection>(TaxCatchAllName, new Guid("f3b0adf9-c1a2-4b02-920d-943fba4b3611")); 
            } 
        }

        /// <summary>
        /// TaxCatchAllLabel field info (OOTB type = LookupMulti)
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll", Justification = "This is the actual SharePoint field name")]
        public static BaseFieldInfo TaxCatchAllLabel 
        { 
            get 
            { 
                return new MinimalFieldInfo<LookupValueCollection>(TaxCatchAllLabelName, new Guid("8f6b6dd8-9357-4019-8172-966fcd502ed2")); 
            } 
        }

        /// <summary>
        /// The assigned to field info (OOTB type = User)
        /// </summary>
        public static BaseFieldInfo AssignedTo 
        { 
            get 
            { 
                return new MinimalFieldInfo<UserValue>(AssignedToName, SPBuiltInFieldId.AssignedTo); 
            } 
        }

        /// <summary>
        /// The percent complete field info (OOTB type = Number, Min = 0, Max = 1, Percentage = TRUE)
        /// </summary>
        public static BaseFieldInfo PercentComplete 
        { 
            get 
            { 
                return new MinimalFieldInfo<double?>(PercentCompleteName, SPBuiltInFieldId.PercentComplete); 
            } 
        }

        /// <summary>
        /// The predecessors field info (OOTB type = LookupMulti)
        /// </summary>
        public static BaseFieldInfo Predecessors 
        { 
            get 
            { 
                return new MinimalFieldInfo<LookupValueCollection>(PredecessorsName, SPBuiltInFieldId.Predecessors); 
            } 
        }

        /// <summary>
        /// The priority field info (OOTB type = Choice)
        /// </summary>
        public static BaseFieldInfo Priority 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(PriorityName, SPBuiltInFieldId.Priority); 
            } 
        }

        /// <summary>
        /// The task status field info (OOTB type = Choice)
        /// </summary>
        public static BaseFieldInfo TaskStatus
        {
            get
            {
                return new MinimalFieldInfo<string>(TaskStatusName, SPBuiltInFieldId.TaskStatus);
            }
        }

        /// <summary>
        /// The home phone field info (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo HomePhone 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(HomePhoneName, SPBuiltInFieldId.HomePhone); 
            } 
        }

        /// <summary>
        /// The work fax field info (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo WorkFax 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(WorkFaxName, SPBuiltInFieldId.WorkFax); 
            } 
        }

        /// <summary>
        /// The work address field info (OOTB type = Note)
        /// </summary>
        public static BaseFieldInfo WorkAddress 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(WorkAddressName, SPBuiltInFieldId.WorkAddress); 
            } 
        }

        /// <summary>
        /// The work country field info (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo WorkCountry 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(WorkCountryName, SPBuiltInFieldId.WorkCountry); 
            } 
        }

        /// <summary>
        /// The work city field info (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo WorkCity 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(WorkCityName, SPBuiltInFieldId.WorkCity); 
            } 
        }

        /// <summary>
        /// The work state field info (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo WorkState 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(WorkStateName, SPBuiltInFieldId.WorkState); 
            } 
        }

        /// <summary>
        /// The work zip field info (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo WorkZip 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(WorkZipName, SPBuiltInFieldId.WorkZip); 
            } 
        }

        /// <summary>
        /// The web page field info (OOTB type = URL)
        /// </summary>
        public static BaseFieldInfo Webpage 
        { 
            get 
            { 
                return new MinimalFieldInfo<UrlValue>(WebpageName, SPBuiltInFieldId.WebPage); 
            } 
        }

        /// <summary>
        /// The comments field info (OOTB type = Note)
        /// </summary>
        public static BaseFieldInfo Comments 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(CommentsName, SPBuiltInFieldId.Comments); 
            } 
        }

        /// <summary>
        /// The department field info (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo Department 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(DepartmentName, SPBuiltInFieldId.Department); 
            } 
        }

        /// <summary>
        /// The role field info (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo Role 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(RoleName, SPBuiltInFieldId.Role); 
            } 
        }

        /// <summary>
        /// The related items field info (OOTB type = RelatedItems)
        /// </summary>
        public static BaseFieldInfo RelatedItems 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(RelatedItemsName, SPBuiltInFieldId.RelatedItems); 
            } 
        }

        /// <summary>
        /// The display order field info - used in composed looks (OOTB type = Number)
        /// </summary>
        public static BaseFieldInfo DisplayOrder 
        { 
            get 
            { 
                return new MinimalFieldInfo<double?>(DisplayOrderName, new Guid("2cc33755-5880-44c7-925c-fd41fd76cefb")); 
            } 
        }

        /// <summary>
        /// The master page URL field info (OOTB type = URL)
        /// </summary>
        public static BaseFieldInfo MasterPageUrl 
        { 
            get 
            { 
                return new MinimalFieldInfo<UrlValue>(MasterPageUrlName, new Guid("b65d5645-28c4-44b5-8f87-c49250c5c98c")); 
            } 
        }

        /// <summary>
        /// The theme URL field info (OOTB type = URL)
        /// </summary>
        public static BaseFieldInfo ThemeUrl 
        { 
            get 
            { 
                return new MinimalFieldInfo<UrlValue>(ThemeUrlName, new Guid("f0490cd6-93e0-42bd-8de3-1be68e3045f1")); 
            } 
        }

        /// <summary>
        /// The image URL field info (OOTB type = URL)
        /// </summary>
        public static BaseFieldInfo ImageUrl 
        { 
            get 
            { 
                return new MinimalFieldInfo<UrlValue>(ImageUrlName, new Guid("833cb87d-835f-4fa7-8927-e781c890f023")); 
            } 
        }

        /// <summary>
        /// The font scheme URL field info (OOTB type = URL)
        /// </summary>
        public static BaseFieldInfo FontSchemeUrl 
        { 
            get 
            { 
                return new MinimalFieldInfo<UrlValue>(ImageUrlName, new Guid("b5dfc328-900e-4306-93e1-43c74a847320")); 
            } 
        }

        /// <summary>
        /// The enterprise keywords field info (OOTB type = TaxonomyMulti)
        /// </summary>
        public static BaseFieldInfo EnterpriseKeywords 
        { 
            get 
            {
                return new MinimalFieldInfo<TaxonomyValueCollection>(EnterpriseKeywordsName, new Guid("23f27201-bee3-471e-b2e7-b64fd8b7ca38")); 
            } 
        }

        /// <summary>
        /// The cell phone (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo Cellphone 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(CellphoneName, new Guid("2a464df1-44c1-4851-949d-fcd270f0ccf2")); 
            } 
        }

        /// <summary>
        /// The full name (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo FullName 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(FullNameName, new Guid("475c2610-c157-4b91-9e2d-6855031b3538")); 
            } 
        }

        /// <summary>
        /// The email (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo Email 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(EmailName, SPBuiltInFieldId.EMail); 
            } 
        }

        /// <summary>
        /// The first name (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo FirstName 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(FirstNameName, SPBuiltInFieldId.FirstName); 
            } 
        }

        /// <summary>
        /// The work phone (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo WorkPhone 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(WorkPhoneName, SPBuiltInFieldId.WorkPhone); 
            } 
        }

        /// <summary>
        /// The job title (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo JobTitle 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(JobTitleName, SPBuiltInFieldId.JobTitle); 
            } 
        }

        /// <summary>
        /// The company (OOTB type = Text)
        /// </summary>
        public static BaseFieldInfo Company 
        { 
            get 
            { 
                return new MinimalFieldInfo<string>(CompanyName, SPBuiltInFieldId.Company); 
            } 
        }

        /// <summary>
        /// The Author (OOTB type = User)
        /// </summary>
        public static BaseFieldInfo CreatedBy 
        { 
            get 
            { 
                return new MinimalFieldInfo<UserValue>(AuthorName, SPBuiltInFieldId.Author); 
            } 
        }

        /// <summary>
        /// The Created date (OOTB type = DateTime)
        /// </summary>
        public static BaseFieldInfo Created 
        { 
            get 
            { 
                return new MinimalFieldInfo<DateTime?>(CreatedName, SPBuiltInFieldId.Created); 
            } 
        }

        /// <summary>
        /// The Editor (OOTB type = User)
        /// </summary>
        public static BaseFieldInfo ModifiedBy
        {
            get
            {
                return new MinimalFieldInfo<UserValue>(EditorName, SPBuiltInFieldId.Editor);
            }
        }

        /// <summary>
        /// The Modified date (OOTB type = DateTime)
        /// </summary>
        public static BaseFieldInfo Modified 
        { 
            get 
            { 
                return new MinimalFieldInfo<DateTime?>(ModifiedName, SPBuiltInFieldId.Modified); 
            } 
        }

        /// <summary>
        /// The body note field.
        /// </summary>
        public static BaseFieldInfo Body
        {
            get
            {
                return new MinimalFieldInfo<string>(BodyName, SPBuiltInFieldId.Body);
            }
        }

        /// <summary>
        /// The content expiration date time field.
        /// </summary>
        public static BaseFieldInfo Expires
        {
            get
            {
                return new MinimalFieldInfo<DateTime?>(ExpiresName, SPBuiltInFieldId.Expires);
            }
        }

        /// <summary>
        /// Document icon field info
        /// </summary>
        public static BaseFieldInfo DocIcon
        {
            get
            {
                return new MinimalFieldInfo<string>(DocIconName, SPBuiltInFieldId.DocIcon);
            }
        }

        /// <summary>
        /// Link file name field info
        /// </summary>
        public static BaseFieldInfo LinkFileName
        {
            get
            {
                return new MinimalFieldInfo<string>(LinkFileNameName, SPBuiltInFieldId.LinkFilename);
            }
        }

        /// <summary>
        /// Checkout user name field info
        /// </summary>
        public static BaseFieldInfo CheckoutUser
        {
            get
            {
                return new MinimalFieldInfo<string>(CheckoutUserName, SPBuiltInFieldId.CheckoutUser);
            }
        }

        /// <summary>
        /// Moderation status field info
        /// </summary>
        public static BaseFieldInfo ModerationStatus
        {
            get
            {
                return new MinimalFieldInfo<string>(CheckoutUserName, SPBuiltInFieldId._ModerationStatus);
            }
        }

        #endregion
    }
}
