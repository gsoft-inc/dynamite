using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GSoft.Dynamite.Configuration;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Logging;
using Microsoft.Office.DocumentManagement.MetadataNavigation;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Navigation;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// Helper class to manage lists.
    /// </summary>
    public class ListHelper : IListHelper
    {
        private const string PagesLibraryRootFolder = "Pages";

        private readonly IContentTypeHelper contentTypeBuilder;
        private readonly IResourceLocator resourceLocator;
        private readonly IFieldHelper fieldHelper;
        private readonly ILogger logger;
        private readonly IListLocator listLocator;
        private readonly IPropertyBagHelper propertyBagHelper;

        /// <summary>Creates a list helper</summary>
        /// <param name="contentTypeBuilder">The content Type Builder.</param>
        /// <param name="fieldHelper">Field creator utility</param>
        /// <param name="resourceLocator">The resource locator</param>
        /// <param name="logger">The logger</param>
        /// <param name="listLocator">List locator</param>
        /// <param name="propertyBagHelper">Property bag helper</param>
        public ListHelper(
            IContentTypeHelper contentTypeBuilder,
            IFieldHelper fieldHelper,
            IResourceLocator resourceLocator,
            ILogger logger,
            IListLocator listLocator,
            IPropertyBagHelper propertyBagHelper)
        {
            this.contentTypeBuilder = contentTypeBuilder;
            this.fieldHelper = fieldHelper;
            this.resourceLocator = resourceLocator;
            this.logger = logger;
            this.listLocator = listLocator;
            this.propertyBagHelper = propertyBagHelper;
        }

        /// <summary>
        /// Finds the list template corresponding to the specified name
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the template does not exist</exception>
        /// <param name="web">The current web</param>
        /// <param name="templateName">The list template name</param>
        /// <returns>The list template</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPListTemplate GetListTemplate(SPWeb web, string templateName)
        {
            var template = web.ListTemplates.Cast<SPListTemplate>().FirstOrDefault(i => i.Name == templateName);
            if (template == null)
            {
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "List template {0} is not available in the web.", templateName));
            }

            return template;
        }

        /// <summary>
        /// Creates the list or returns the existing one.
        /// </summary>
        /// <remarks>The list name and description will not be translated</remarks>
        /// <exception cref="SPException">If the list already exists but doesn't have the specified list template.</exception>
        /// <param name="web">The current web</param>
        /// <param name="listInfo">The list to create</param>
        /// <returns>The new list or the existing list</returns>
        public SPList EnsureList(SPWeb web, ListInfo listInfo)
        {
            // First, try to find an already existing list with the same name but at a different Web-relative URL
            var list = this.listLocator.GetByNameResourceKey(web, listInfo.DisplayNameResourceKey);
            if (list != null)
            {
                if (!list.RootFolder.Url.Equals(listInfo.WebRelativeUrl.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    // A list with the same name already exists on a different web relative URL, throwing exception
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture, "A list with the name {0} already exists on a different web relative URL. Try using a different name.", listInfo.DisplayNameResourceKey));
                }
            }

            list = this.listLocator.TryGetList(web, listInfo.WebRelativeUrl.ToString());
            
            // Ensure the list
            if (list == null)
            {
                list = this.CreateList(web, listInfo);
            }
            else
            {
                // If it isn't the pages library
                if (string.CompareOrdinal(list.RootFolder.Name, PagesLibraryRootFolder) != 0)
                {
                    this.logger.Info("List " + listInfo.WebRelativeUrl.ToString() + " already exists");

                    // If the Overwrite parameter is set to true, delete and recreate the catalog
                    if (listInfo.Overwrite)
                    {
                        this.logger.Info("Overwrite is set to true, recreating the list " + listInfo.WebRelativeUrl.ToString());

                        web.Lists.Delete(list.ID);
                        
                        // gotta force a re-fetch on the parent SPWeb, otherwise the 
                        // the new list will be create with the same ID as the old one (don't ask, weird SP API behavior).
                        using (var freshWebToMakeSureNewListGuidIsGiven = web.Site.OpenWeb(web.ID))
                        {
                            list = this.CreateList(freshWebToMakeSureNewListGuidIsGiven, listInfo);
                        }
                    }
                }                
            }

            // Update the display name and description, if need be
            this.SetTitleAndDescriptionValues(web, listInfo, list);

            // Remove Item Content Type
            if (listInfo.RemoveDefaultContentType)
            {
                this.logger.Info("Removing the default Item Content Type");

                // If content type is direct child of item, remove it
                this.RemoveItemContentType(list);
            }

            // Add All Content Types
            if (listInfo.ContentTypes != null && listInfo.ContentTypes.Count >= 1)
            {
                this.contentTypeBuilder.EnsureContentType(list.ContentTypes, listInfo.ContentTypes);

                // Re-fetch, list was updated through another object instance, so we gotta refresh our object instance
                list = web.Lists[list.ID];

                // Set the unique content type order on the root folder.
                IList<SPContentType> contentTypeOrder = new List<SPContentType>();

                foreach (var contentTypeInfo in listInfo.ContentTypes)
                {
                    var listContentType = GetListContentType(list, contentTypeInfo);
                    if (listContentType != null)
                    {
                        // If we find a content type on the list that matches the content type info in the list info we add it to the collection.
                        contentTypeOrder.Add(listContentType);
                    }
                }

                if (contentTypeOrder.Count >= 1)
                {
                    // If we have content types in our new list, we set the new list.
                    list.RootFolder.UniqueContentTypeOrder = contentTypeOrder;
                    list.RootFolder.Update();
                }

                list.Update();
            }

            // Write the list ID in a property bag key-value pair
            if (!string.IsNullOrEmpty(listInfo.PropertyBagKeyForListId))             
            {
                this.SetPropertyBagKeyForListId(web, listInfo.PropertyBagKeyForListId, list.ID.ToString());
            }

            list = web.Lists[list.ID];

            // Draft VisibilityType
            if (listInfo.HasDraftVisibilityType)
            {
                list.EnableModeration = true;
                list.DraftVersionVisibility = listInfo.DraftVisibilityType;
            }

            // Versioning settings
            this.SetVersioning(
                list, 
                listInfo.IsVersioningEnabled, 
                listInfo.AreMinorVersionsEnabled, 
                listInfo.MajorVersionLimit, 
                listInfo.MinorVersionLimit);

            // Ratings
            this.SetRatings(list, listInfo.RatingType, listInfo.EnableRatings);

            // Set list Write Security
            this.SetWriteSecurity(list, listInfo.WriteSecurity);

            // Quick Launch Navigation
            if (listInfo.AddToQuickLaunch)
            {
                this.AddtoQuickLaunch(list);
            }

            // Attachements (only possible if not a DocLib or Survey list)
            if (listInfo.EnableAttachements)
            {
                if (list.BaseType != SPBaseType.Survey
                    && list.BaseType != SPBaseType.DocumentLibrary)
                {
                    list.EnableAttachments = listInfo.EnableAttachements;
                }
                else
                {
                    this.logger.Warn(
                        "EnsureList - Skipped setting EnableAttachment=TRUE on list (url={0}) because list is a Document Library or a Survey list, which don't support attachements." +
                        " Provide a ListInfo without EnableAttachment=TRUE to avoid this warning.", 
                        listInfo.WebRelativeUrl);
                }
            }
            else if (!ListContainsAttachments(web, listInfo))
            {
                // Want to disable attachments on a list with no attachments already created
                list.EnableAttachments = listInfo.EnableAttachements;
            }
            else
            {
                // Case where you would like to disable attachments on a list which contains attachments. It is not allowed
                // because it could delete all attachments.
                throw new ArgumentException(
                    string.Format(
                    CultureInfo.InvariantCulture, 
                    "Not allowed to disable attachments on list '{0}' because it contains attachement(s). Attachments on it would be lost.", 
                    list.TitleResource.Value));
            }

            list.Update();

            // Get the updated list object because we have to reference previous added fields that the old list object didn't have (cause NullReferenceException).
            list = this.listLocator.TryGetList(web, listInfo.WebRelativeUrl.ToString());

            // Ensure the field definitions to make sure that all fields are present and to override/apply column default Values
            this.fieldHelper.EnsureField(list.Fields, listInfo.FieldDefinitions);

            // List Validation Settings
            if (listInfo.ValidationSettings.Any())
            {
                ListValidationInfo currentLocaleSettings;

                if (listInfo.ValidationSettings.TryGetValue(web.Locale.Name, out currentLocaleSettings))
                {
                    this.ConfigureValidationSettings(list, currentLocaleSettings.ValidationFormula, currentLocaleSettings.ValidationMessage);
                }
                else
                {
                    this.logger.Warn("No validation settings found in the dictionnary corresponding to the current web locale {0}. Skipping this step.", web.Locale.Name);
                }
            }

            // Default View Fields
            this.AddFieldsToDefaultView(list, listInfo.DefaultViewFields);

            // Save changes.
            list.Update();

            return list;
        }

        /// <summary>
        /// Ensure a list of lists in the web
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="listInfos">The list information</param>
        /// <returns>List of lists</returns>
        public IEnumerable<SPList> EnsureList(SPWeb web, ICollection<ListInfo> listInfos)
        {
            var lists = new List<SPList>();

            foreach (ListInfo list in listInfos)
            {
                lists.Add(this.EnsureList(web, list));
            }

            return lists;
        }

        /// <summary>
        /// Configures the validation settings.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="validationFormula">The validation formula.</param>
        /// <param name="validationMessage">The validation message.</param>
        public void ConfigureValidationSettings(SPList list, string validationFormula, string validationMessage)
        {
            try
            {
                list.ValidationFormula = validationFormula;
                list.ValidationMessage = validationMessage;
                list.Update();
            }
            catch (SPException invalidFormulaException)
            {
                throw new ArgumentException("The validation formula is not valid. Check the syntax and make sure you have the right field's display name(s).", invalidFormulaException);
            }
        }

        /// <summary>
        /// Sets the versioning on the list or library.
        /// Note: The minor versioning enabling/disabling is only available on document libraries.
        /// </summary>
        /// <param name="list">The list or library.</param>
        /// <param name="isVersioningEnabled">if set to <c>true</c> [is versioning enabled].</param>
        /// <param name="areMinorVersionsEnabled">if set to <c>true</c> [are minor versions enabled].</param>
        /// <param name="majorVersionLimit">The major version limit (0 is unlimited).</param>
        /// <param name="minorVersionLimit">The minor version limit (0 is unlimited).</param>
        public void SetVersioning(
            SPList list, 
            bool isVersioningEnabled, 
            bool areMinorVersionsEnabled, 
            int majorVersionLimit, 
            int minorVersionLimit)
        {
            list.EnableVersioning = isVersioningEnabled;
            if (isVersioningEnabled)
            {
                list.MajorVersionLimit = majorVersionLimit;

                // If the list is a document library, minor versioning can be controlled.
                // Else, if it's a normal list, minor versions can
                // only be set if the moderation (drafts) is enabled.
                if (list.BaseType == SPBaseType.DocumentLibrary)
                {
                    list.EnableMinorVersions = areMinorVersionsEnabled;
                    list.MajorWithMinorVersionsLimit = minorVersionLimit;
                }
                else if (list.EnableModeration)
                {
                    list.MajorWithMinorVersionsLimit = minorVersionLimit;
                }
            }
        }

        /// <summary>
        /// Enable or disable ratings on a SPList.
        /// This method does not call SPList.Update(). Your code should handle this.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="ratingType">The rating type. Can be "Likes" or "Ratings" </param>
        /// <param name="ratingStatus">True to enable. False to disable.</param>
        public void SetRatings(SPList list, string ratingType, bool ratingStatus)
        {
            // Retrieve assembly from a puplib class
            Assembly assembly = typeof(Microsoft.SharePoint.Portal.RatingsSettingsPage).Assembly;

            // Get ReputationHelper type
            Type reputationHelper = assembly.GetType("Microsoft.SharePoint.Portal.ReputationHelper");

            MethodInfo enableMethod = reputationHelper.GetMethod("EnableReputation", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo disableMethod = reputationHelper.GetMethod("DisableReputation", BindingFlags.Static | BindingFlags.NonPublic);

            if (ratingStatus && !string.IsNullOrEmpty(ratingType))
            {
                try
                {
                    enableMethod.Invoke(null, new object[] { list, ratingType, false });
                }
                catch (TargetInvocationException targetInvocationException)
                {
                    throw new SPException(
                        "Could not enable ratings on this list. Make sure the Ratings Feature is enabled on your site, by using a Publishing Site for example.", targetInvocationException);
                }
            }
            else
            {
                disableMethod.Invoke(null, new object[] { list });
            }
        }

        /// <summary>
        /// Add the list to the quick launch bar
        /// </summary>
        /// <param name="list">The list</param>
        public void AddtoQuickLaunch(SPList list)
        {
            var web = list.ParentWeb;

            // Check for an existing link to the list.
            var listNode = web.Navigation.GetNodeByUrl(list.DefaultViewUrl);

            // No link, so create one.
            if (listNode == null)
            {
                // Create the node.
                listNode = new SPNavigationNode(list.Title, list.DefaultViewUrl);

                // Add it to Quick Launch.
                web.Navigation.AddToQuickLaunch(listNode, SPQuickLaunchHeading.Lists);
            }
        }

        /// <summary>
        ///  Set WriteSecurity on a SPList.
        ///  This method does not call SPList.Update(). Your code should handle this.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="writeSecurityOptions">The Write Security option</param>
        public void SetWriteSecurity(SPList list, WriteSecurityOptions writeSecurityOptions)
        {
            list.WriteSecurity = (int)writeSecurityOptions;
        }

        /// <summary>
        /// Method to remove the Item Content Type from the List
        /// </summary>
        /// <param name="list">The current List</param>
        public void RemoveItemContentType(SPList list)
        {
            // If content type is direct child of item, remove it
            var bestMatchItem = list.ContentTypes.BestMatch(SPBuiltInContentTypeId.Item);
            if (bestMatchItem.Parent == SPBuiltInContentTypeId.Item)
            {
                list.ContentTypes.Delete(bestMatchItem);
            }
        }

        /// <summary>
        /// Set the metadata navigation settings for the current list
        /// </summary>
        /// <param name="web">The web that store the list</param>
        /// <param name="settings">The metadata settings</param>
        public void SetMetadataNavigation(SPWeb web, MetadataNavigationSettingsInfo settings)
        {
            // Get the list
            var list = this.EnsureList(web, settings.List);

            // Get the MDN settings object for the SPList that was passed in.
            var mdnSettings = MetadataNavigationSettings.GetMetadataNavigationSettings(list);

            // Clear the hierarchy configuration
            mdnSettings.ClearConfiguredHierarchies();

            // Add the default folder hierarchy to get it work. It is mandatory to display the tree view. You can hide hide later with SetHideFoldersNode
            var folderHierarchyToAdd = MetadataNavigationHierarchy.CreateFolderHierarchy();
            if (mdnSettings.FindConfiguredHierarchy(folderHierarchyToAdd.FieldId) == null)
            {
                mdnSettings.AddConfiguredHierarchy(folderHierarchyToAdd);
            }

            if (settings.AddFolderDefaultHierarchy)
            {
                SetHideFoldersNode(mdnSettings, false);
            }
            else
            {
                SetHideFoldersNode(mdnSettings, true);
            }

            if (settings.AddContentTypeDefaultHierarchy)
            {
                // Remove the default folder hierarchy
                var contentTypeHierarchyToAdd = MetadataNavigationHierarchy.CreateContentTypeHierarchy();
                if (mdnSettings.FindConfiguredHierarchy(contentTypeHierarchyToAdd.FieldId) == null)
                {
                    mdnSettings.AddConfiguredHierarchy(contentTypeHierarchyToAdd);
                }
            }

            if (settings.Hierarchies != null)
            {
                // Add custom hierachies
                foreach (var fieldName in settings.Hierarchies)
                {
                    var field = list.Fields.TryGetFieldByStaticName(fieldName);
                    if (field != null)
                    {
                        var hierarchy = new MetadataNavigationHierarchy(field);
                        mdnSettings.AddConfiguredHierarchy(hierarchy);
                    }
                }
            }

            // Clear the key filters configuration
            mdnSettings.ClearConfiguredKeyFilters();

            if (settings.AddContentTypeDefaultKeyFilter)
            {
                var contentTypeKeyFilter = MetadataNavigationKeyFilter.CreateContentTypeKeyFilter();
                if (null == mdnSettings.FindConfiguredKeyFilter(contentTypeKeyFilter.FieldId))
                {
                    mdnSettings.AddConfiguredKeyFilter(contentTypeKeyFilter);
                }
            }

            if (settings.KeyFilters != null)
            {
                // Add custom key filters
                foreach (var fieldName in settings.KeyFilters)
                {
                    var field = list.Fields.TryGetFieldByStaticName(fieldName);
                    if (field != null)
                    {
                        var keyFilter = new MetadataNavigationKeyFilter(field);
                        mdnSettings.AddConfiguredKeyFilter(keyFilter);
                    }
                }
            }

            // Set the MDN settings back into the list and automatically adjust indexing.
            MetadataNavigationSettings.SetMetadataNavigationSettings(list, mdnSettings, true);

            list.RootFolder.Update();
        }

        #region List View

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        public void AddFieldsToDefaultView(SPList list, ICollection<BaseFieldInfo> fields)
        {
            this.AddFieldsToDefaultView(list, fields, false);
        }

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        /// <param name="removeExistingViewFields">if set to <c>true</c> [remove existing view fields].</param>
        public void AddFieldsToDefaultView(SPList list, ICollection<BaseFieldInfo> fields, bool removeExistingViewFields)
        {
            var defaultView = list.DefaultView;
            var fieldCollection = defaultView.ViewFields;

            // Remove default view fields
            if (removeExistingViewFields)
            {
                fieldCollection.DeleteAll();
            }

            foreach (BaseFieldInfo field in fields)
            {
                if (list.Fields.Contains(field.Id))
                {
                    this.EnsureFieldInView(fieldCollection, list.Fields[field.Id].InternalName);
                }
                else if (list.Fields.ContainsFieldWithStaticName(field.InternalName))
                {
                    this.EnsureFieldInView(fieldCollection, list.Fields.GetFieldByInternalName(field.InternalName).InternalName);
                }
                else
                {
                    if (list.Fields.Contains(field.Id))
                    {
                        this.EnsureFieldInView(fieldCollection, list.Fields[field.Id].InternalName);
                    }
                    else
                    {
                        this.logger.Warn("Field with ID {0} was not found in list '{1}' fields", field.Id, list.Title);
                    }
                }

                defaultView.Update();
            }
        }

        /// <summary>
        /// Ensure the field in the view
        /// </summary>
        /// <param name="fieldCollection">the collection of fields</param>
        /// <param name="fieldInternalName">the current field internal name</param>
        public void EnsureFieldInView(SPViewFieldCollection fieldCollection, string fieldInternalName)
        {
            if (!string.IsNullOrEmpty(fieldInternalName))
            {
                if (!fieldCollection.Exists(fieldInternalName))
                {
                    fieldCollection.Add(fieldInternalName);
                }
            }
        }

        #endregion List View

        /// <summary>
        /// Actually creates a list in SharePoint. For internal use in this class only. 
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="listInfo">The list information contains all the necessary data to create the list</param>
        /// <returns>The created List</returns>
        private SPList CreateList(SPWeb web, ListInfo listInfo)
        {
            // Throw an exception if the proposed web-relative URL conflicts with an existing subsite/folder URL or a managed path
            Guid id;
            try
            {
                var title = this.resourceLocator.GetResourceString(listInfo.ResourceFileName, listInfo.DisplayNameResourceKey);
                var description = this.resourceLocator.GetResourceString(listInfo.ResourceFileName, listInfo.DescriptionResourceKey);

                id = web.Lists.Add(
                    title, 
                    description, 
                    listInfo.WebRelativeUrl.ToString(), 
                    listInfo.ListTemplateInfo.FeatureId.ToString(), 
                    listInfo.ListTemplateInfo.ListTempateTypeId, 
                    null);
            }
            catch (SPException sharepointException)
            {
                var messageFormat = "The web-relative URL '{0}' for the ensured list conflicts with an existing URL "
                        + "(subweb, folder, ...). Try with a different one.";

                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, messageFormat, listInfo.WebRelativeUrl), 
                    sharepointException);
            }

            return web.Lists[id];
        }

        private void SetTitleAndDescriptionValues(SPWeb web, ListInfo listInfo, SPList list)
        {
            // end with the main language
            var availableLanguages = web.SupportedUICultures.Reverse();
            foreach (var availableLanguage in availableLanguages)
            {
                if (!string.IsNullOrEmpty(listInfo.DisplayNameResourceKey))
                {
                    var title = this.resourceLocator.Find(listInfo.ResourceFileName, listInfo.DisplayNameResourceKey, availableLanguage.LCID);
                    list.TitleResource.SetValueForUICulture(availableLanguage, title);
                    list.TitleResource.Update();
                }

                if (!string.IsNullOrEmpty(listInfo.DescriptionResourceKey))
                {
                    var description = this.resourceLocator.Find(listInfo.ResourceFileName, listInfo.DescriptionResourceKey, availableLanguage.LCID);
                    list.DescriptionResource.SetValueForUICulture(availableLanguage, description);
                    list.DescriptionResource.Update();
                }
            }
        }

        private static void SetHideFoldersNode(MetadataNavigationSettings settings, bool value)
        {
            var t = settings.GetType();
            t.InvokeMember("HideFoldersNode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, settings, new object[] { value }, CultureInfo.InvariantCulture);
        }

        private static SPContentType GetListContentType(SPList list, ContentTypeInfo contentTypeInfo)
        {
            var contentTypeId = contentTypeInfo.ContentTypeId;

            // If content type is direct child of item, remove it
            var bestMatchItem = list.ContentTypes.BestMatch(contentTypeId);
            if (bestMatchItem.Parent == contentTypeId)
            {
                return list.ContentTypes[bestMatchItem];
            }

            return null;
        }

        /// <summary>
        /// Creates or overwrites a pair in the property bag with the new created list ID
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="key">The property key</param>
        /// <param name="id">The list GUID</param>
        private void SetPropertyBagKeyForListId(SPWeb web, string key, string id)
        {
            var propertiesList = new List<PropertyBagValue>();
            propertiesList.Add(new PropertyBagValue() { Key = key, Value = id, Overwrite = true });
            this.propertyBagHelper.SetWebValues(web, propertiesList);
        }

        private static bool ListContainsAttachments(SPWeb web, ListInfo listInfo)
        {
            SPFolder listFolder = null;

            // Get the list folder in the web
            var folders = listInfo.WebRelativeUrl.ToString().Split('/');
            for (var i = 0; i < folders.Count(); i++)
            {
                // If the first list folder segment, get it in the web folders collection.
                // Else, get the folder in the subfolders collection.
                if (i == 0)
                {
                    listFolder = web.Folders[folders[i]];
                }
                else if (listFolder != null)
                {
                    listFolder = listFolder.SubFolders[folders[i]];
                }
            }

            // If the list folder exists
            if (listFolder != null)
            {
                // If the attachments folder exists
                var attachmentsFolder = listFolder.SubFolders["Attachments"];
                if (attachmentsFolder != null)
                {
                    // Return true if any attachments folder contains a subfolder with files.
                    return attachmentsFolder.SubFolders.Cast<SPFolder>().Any(folder => folder.Files.Count > 0);
                }
            }

            return false;
        }
    }
}