using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.FieldTypes;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Lists.Entities;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Schemas;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Navigation;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Helpers
{
    /// <summary>
    /// Helper class to manage lists.
    /// </summary>
    public class ListHelper
    {
        private readonly ContentTypeHelper _contentTypeHelper;
        private readonly IResourceLocator _resourceLocator;
        private readonly FieldHelper _fieldHelper;
        private readonly ILogger _logger;
        private readonly ISharePointEntityBinder _binder;
        private readonly TaxonomyHelper _taxonomyHelper;

        /// <summary>
        /// Creates a list helper
        /// </summary>
        /// <param name="contentTypeHelper">A content type helper</param>
        /// <param name="fieldHelper">The field helper.</param>
        /// <param name="taxonomyHelper">The taxonomy helper</param>
        /// <param name="resourceLocator">The resource locator</param>
        /// <param name="logger">The logger</param>
        /// <param name="binder">The entity binder</param>
        public ListHelper(ContentTypeHelper contentTypeHelper, FieldHelper fieldHelper, TaxonomyHelper taxonomyHelper, IResourceLocator resourceLocator, ILogger logger, ISharePointEntityBinder binder)
        {
            this._contentTypeHelper = contentTypeHelper;
            this._fieldHelper = fieldHelper;
            this._resourceLocator = resourceLocator;
            this._logger = logger;
            this._binder = binder;
            this._taxonomyHelper = taxonomyHelper;
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
        /// <param name="name">The name of the list</param>
        /// <param name="description">The description of the list</param>
        /// <param name="template">The desired list template to use to instantiate the list</param>
        /// <returns>The new list or the existing list</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPList EnsureList(SPWeb web, string name, string description, SPListTemplate template)
        {
            var list = this.TryGetList(web, name);

            if (list != null)
            {
                // List already exists, check for correct template
                if (list.BaseTemplate != template.Type)
                {
                    throw new SPException(string.Format(CultureInfo.InvariantCulture, "List {0} has list template type {1} but should have list template type {2}.", name, list.BaseTemplate, template.Type));
                }
            }
            else
            {
                // Create new list
                var id = web.Lists.Add(name, description, template);

                list = web.Lists[id];
            }

            return list;
        }

        /// <summary>
        /// Creates the list or returns the existing one.
        /// </summary>
        /// <remarks>The list name and description will not be translated</remarks>
        /// <exception cref="SPException">If the list already exists but doesn't have the specified list template.</exception>
        /// <param name="web">The current web</param>
        /// <param name="name">The name of the list</param>
        /// <param name="description">The description of the list</param>
        /// <param name="templateType">The desired list template type to use to instantiate the list</param>
        /// <returns>The new list or the existing list</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPList EnsureList(SPWeb web, string name, string description, SPListTemplateType templateType)
        {
            var list = this.TryGetList(web, name);

            if (list != null)
            {
                // List already exists, check for correct template
                if (list.BaseTemplate != templateType)
                {
                    throw new SPException(string.Format(CultureInfo.InvariantCulture, "List {0} has list template type {1} but should have list template type {2}.", name, list.BaseTemplate, templateType));
                }
            }
            else
            {
                // Create new list
                var id = web.Lists.Add(name, description, templateType);

                list = web.Lists[id];
            }

            return list;
        }

        /// <summary>
        /// Ensure the list in the web
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="rootFolderUrl">The root folder URL of the list</param>
        /// <param name="titleResourceKey">Titles' resource key</param>
        /// <param name="descriptionResourceKey">Descriptions' resource key</param>
        /// <param name="templateType">The template type of the list</param>
        /// <returns>The list object</returns>
        public SPList EnsureList(SPWeb web, string rootFolderUrl, string titleResourceKey, string descriptionResourceKey, SPListTemplateType templateType)
        {
            var list = this.GetListByRootFolderUrl(web, rootFolderUrl);

            if (list != null)
            {
                // List already exists, check for correct template
                if (list.BaseTemplate != templateType)
                {
                    throw new SPException(string.Format(CultureInfo.InvariantCulture, "List with root folder url {0} has list template type {1} but should have list template type {2}.", rootFolderUrl, list.BaseTemplate, templateType));
                }
            }
            else
            {
                // Create new list
                var id = web.Lists.Add(rootFolderUrl, string.Empty, templateType);
                list = web.Lists[id];
            }

            // Update title and description
            // Note that the variations synchronization process for a list doesn't copy the resources settings in the target sites
            var availableLanguages = web.SupportedUICultures.Reverse();   // end with the main language
            foreach (var availableLanguage in availableLanguages)
            {
                var title = this._resourceLocator.Find(titleResourceKey, availableLanguage.LCID);
                var description = this._resourceLocator.Find(descriptionResourceKey, availableLanguage.LCID);

                list.TitleResource.SetValueForUICulture(availableLanguage, title);
                list.DescriptionResource.SetValueForUICulture(availableLanguage, description);
            }

            list.Update();

            return list;
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
            var list = this.GetListByRootFolderUrl(web, listInfo.RootFolderUrl);

            // Ensure the list
            if (list == null)
            {
                list = this.EnsureList(web, listInfo.RootFolderUrl, listInfo.DisplayNameResourceKey, listInfo.DescriptionResourceKey, listInfo.ListTemplate);
            }
            else
            {
                this._logger.Info("List " + listInfo.RootFolderUrl + " already exists");

                // If the Overwrite parameter is set to true, celete and recreate the catalog
                if (listInfo.Overwrite)
                {
                    this._logger.Info("Overwrite is set to true, recreating the list " + listInfo.RootFolderUrl);

                    list.Delete();
                    list = this.EnsureList(web, listInfo.RootFolderUrl, listInfo.DisplayNameResourceKey, listInfo.DescriptionResourceKey, listInfo.ListTemplate);
                }
                else
                {
                    // Get the existing list
                    list = this.EnsureList(web, listInfo.RootFolderUrl, listInfo.DisplayNameResourceKey, listInfo.DescriptionResourceKey, listInfo.ListTemplate);
                }
            }

            // Remove Item Content Type
            if (listInfo.RemoveDefaultContentType)
            {
                this._logger.Info("Removing the default Item Content Type");

                // If content type is direct child of item, remove it
                this.RemoveItemContentType(list);
            }

            // Add All Content Types
            this.EnsureContentType(list, listInfo.ContentTypes);

            // Draft VisibilityType
            if (listInfo.HasDraftVisibilityType)
            {
                list.EnableModeration = true;
                list.DraftVersionVisibility = listInfo.DraftVisibilityType;

                list.Update();
            }

            // Ratings
            this.SetRatings(list, listInfo.RatingType, listInfo.EnableRatings);

            // Set list Write Security
            this.SetWriteSecurity(list, listInfo.WriteSecurity);

            // Quick Launch Navigation
            if (listInfo.AddToQuickLaunch)
            {
                this.AddtoQuickLaunch(list);
            }

            // Attachements
            if (!listInfo.EnableAttachements)
            {
                list.EnableAttachments = listInfo.EnableAttachements;
                list.Update();
            }

            // Default View Fields
            this.AddFieldsToDefaultView(web, list, listInfo.DefaultViewFields);

            // Get the updated list object because we have to reference previous added fields that the old list object didn't have (cause NullReferenceException).    
            list = this.GetListByRootFolderUrl(web, listInfo.RootFolderUrl);

            // Default Values
            this.SetDefaultValues(list, listInfo);

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
        /// Adds the content type id.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="contentTypeId">The content type id.</param>
        /// <exception cref="System.ArgumentNullException">Any null parameters.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">contentTypeId;Content Type not available in the lists parent web.</exception>
        public void EnsureContentType(SPList list, SPContentTypeId contentTypeId)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (contentTypeId == null)
            {
                throw new ArgumentNullException("contentTypeId");
            }

            SPContentType contentType = list.ParentWeb.AvailableContentTypes[contentTypeId];

            if (contentType != null)
            {
                this._contentTypeHelper.EnsureContentType(list.ContentTypes, contentType);
            }
            else
            {
                throw new ArgumentOutOfRangeException("contentTypeId", "Content Type not available in the lists parent web.");
            }
        }

        /// <summary>
        /// Adds the content type.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">Any null parameter.</exception>
        public void EnsureContentType(SPList list, ContentTypeInfo contentType)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }

            // Enable content types if not yet done.
            if (!list.ContentTypesEnabled)
            {
                list.ContentTypesEnabled = true;
                list.Update(true);
            }

            this._contentTypeHelper.EnsureContentType(list.ContentTypes, contentType);
            list.Update(true);
        }

        /// <summary>
        /// Ensure a list of content types for a list
        /// </summary>
        /// <param name="list">The list</param>
        /// <param name="contentTypes">The content type list</param>
        public void EnsureContentType(SPList list, ICollection<ContentTypeInfo> contentTypes)
        {
            foreach (ContentTypeInfo contentType in contentTypes)
            {
                this.EnsureContentType(list, contentType);
            }
        }

        /// <summary>
        /// Get the list by root folder url
        /// </summary>
        /// <param name="web">
        /// The web.
        /// </param>
        /// <param name="listRootFolderUrl">
        /// The list Root Folder Url.
        /// </param>
        /// <returns>
        /// The list
        /// </returns>
        public SPList GetListByRootFolderUrl(SPWeb web, string listRootFolderUrl)
        {
            return web.Lists.Cast<SPList>().Where(list => list.RootFolder.Name.ToLowerInvariant() == listRootFolderUrl.ToLowerInvariant()).FirstOrDefault();
        }

        /// <summary>
        /// Creates a field on the list
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="genericField">The generic field.</param>
        /// <param name="fieldInternalName">The Field internal name.</param>
        /// <param name="fieldDisplayName">The field display name.</param>
        /// <param name="fieldDescription">The field description.</param>
        /// <param name="fieldGroup">The field group.</param>
        /// <returns>
        /// The internal name of newly created field.
        /// </returns>
        public SPField CreateListField(SPList list, GenericFieldSchema genericField, string fieldInternalName, string fieldDisplayName, string fieldDescription, string fieldGroup)
        {
            // TODO: Make this EnsureListField and prefer using ContentTypeHelper.EnsureField instead of using list columns (i.e. use CTs)
            genericField.FieldName = fieldInternalName;

            // Here is a trick: We have to pass the internal name as display name and set the display name after creation
            genericField.FieldDisplayName = fieldInternalName;
            genericField.FieldDescription = fieldDescription;
            genericField.FieldStaticName = fieldInternalName;
            genericField.FieldGroup = fieldGroup;

            var fieldName = this._fieldHelper.EnsureField(list.Fields, genericField.ToXElement());

            if (!string.IsNullOrEmpty(fieldName))
            {
                // When you set title, need to be in the same Culture as Current web Culture 
                // Thanks to http://www.sharepointblues.com/2011/11/14/splist-title-property-spfield-displayname-property-not-updating/
                Thread.CurrentThread.CurrentUICulture =
                    new CultureInfo((int)list.ParentWeb.Language);

                // Get the new field - Be careful, return the display name    
                var field = list.Fields.GetFieldByInternalName(fieldInternalName);
                field.Title = fieldDisplayName;
                field.Description = fieldDescription;
                field.Update(true);

                list.Update();
            }

            return list.Fields.GetFieldByInternalName(fieldInternalName);
        }

        /// <summary>
        /// Create a taxonomy Field in a SharePoint list
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="fieldInternalName">The Field internal name.</param>f
        /// <param name="fieldDisplayName">The field display name.</param>
        /// <param name="fieldDescription">The field description.</param>
        /// <param name="fieldGroup">The field group.</param>
        /// <param name="isMultiple">True if the field must allow multiple values. False otherwise.</param>
        /// <param name="isOpen">True is the the field is an open term creation. False otherwise.</param>
        /// <returns>The newly created field.</returns>
        [Obsolete]
        public SPField CreateListTaxonomyField(SPList list, string fieldInternalName, string fieldDisplayName, string fieldDescription, string fieldGroup, bool isMultiple, bool isOpen)
        {
            // TODO: Combine this with EnsureListField and prefer using ContentTypeHelper.EnsureField instead of using list columns (i.e. use CTs)
            // To support all this, make FieldInfo more complete to document all field metadata (instead of polluting ListHelper)

            // Create the schema 
            // TODO: inject this properly through Registration on Container
            var taxonomySchema = new TaxonomyFieldSchema();
            taxonomySchema.IsMultiple = false;

            // Dont'use CreateNewField method because of its doesn't generate the Field ID
            var field = this.CreateListField(list, taxonomySchema, fieldInternalName, fieldDisplayName, fieldDescription, fieldGroup) as TaxonomyField;

            field.Open = isOpen;
            field.AllowMultipleValues = isMultiple;
            field.TargetTemplate = string.Empty;
            field.Update(true);
            list.Update();

            return field;
        }

        /// <summary>
        /// Create a text field in the list
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="fieldInternalName">The Field internal name.</param>
        /// <param name="fieldDisplayName">The field display name.</param>
        /// <param name="fieldDescription">The field description.</param>
        /// <param name="fieldGroup">The field group.</param>
        /// <param name="isMultiLines">if set to <c>true</c> [is multi lines].</param>
        /// <returns>
        /// The newly created field.
        /// </returns>
        [Obsolete]
        public SPField CreateTextField(SPList list, string fieldInternalName, string fieldDisplayName, string fieldDescription, string fieldGroup, bool isMultiLines)
        {
            // TODO: See CreateTaxonomyField comment above, this needs to be refactored/moved because Lists should have to know about all this (since want to share 
            // these concepts with ContentTypeHelper...

            // Create the schema 
            var textFieldSchema = new TextFieldSchema { IsMultiLine = false };
            var field = this.CreateListField(list, textFieldSchema, fieldInternalName, fieldDisplayName, fieldDescription, fieldGroup);

            return field;
        }

        /// <summary>
        /// Create a GUID field in the list
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="fieldInternalName">The Field internal name.</param>
        /// <param name="fieldDisplayName">The field display name.</param>
        /// <param name="fieldDescription">The field description.</param>
        /// <param name="fieldGroup">The field group.</param>
        /// <returns>
        /// The newly created field.
        /// </returns>
        public SPField CreateGuidField(SPList list, string fieldInternalName, string fieldDisplayName, string fieldDescription, string fieldGroup)
        {
            // TODO: See CreateTaxonomyField comment above, this needs to be refactored/moved because Lists should have to know about all this (since want to share 
            // these concepts with ContentTypeHelper...

            // Create the schema 
            var textFieldSchema = new GuidFieldSchema();
            var field = this.CreateListField(list, textFieldSchema, fieldInternalName, fieldDisplayName, fieldDescription, fieldGroup);

            return field;
        }

        /// <summary>
        /// Enable or disable ratings on a SPList
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
                enableMethod.Invoke(null, new object[] { list, ratingType, false });
            }
            else
            {
                disableMethod.Invoke(null, new object[] { list });
            }

            list.Update();
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
        ///  Set WriteSecurity on a SPList
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="option">The Write Security option</param>
        public void SetWriteSecurity(SPList list, WriteSecurityOptions option)
        {
            list.WriteSecurity = (int)option;
            list.Update();
        }

        /// <summary>
        /// Enforce the unique value(s) for a list field. In case the field is reused in the site collection, we can make that change on the list scope.
        /// </summary>
        /// <param name="list">The list who owns the field</param>
        /// <param name="field">The field to enforce</param>
        public void EnforceUniqueValuesToField(SPList list, IFieldInfo field)
        {
            if (list != null && field != null)
            {
                var listField = this._fieldHelper.GetFieldById(list.Fields, field.Id);

                if (listField != null)
                {
                    listField.EnforceUniqueValues = true;
                    listField.Indexed = true;
                    listField.Update();
                }
            }
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

        #region List View

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        public void AddFieldsToDefaultView(SPWeb web, SPList list, ICollection<IFieldInfo> fields)
        {
            this.AddFieldsToDefaultView(web, list, fields, false);
        }

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        /// <param name="removeExistingViewFields">if set to <c>true</c> [remove existing view fields].</param>
        public void AddFieldsToDefaultView(SPWeb web, SPList list, ICollection<IFieldInfo> fields, bool removeExistingViewFields)
        {
            // get the default view of the list
            var defaulView = web.GetViewFromUrl(list.DefaultViewUrl);
            var fieldCollection = defaulView.ViewFields;

            // Remove default view fields
            if (removeExistingViewFields)
            {
                fieldCollection.DeleteAll();
            }

            foreach (IFieldInfo field in fields)
            {
                if (list.Fields.ContainsFieldWithStaticName(field.InternalName))
                {
                    this.EnsureFieldInView(fieldCollection, field.InternalName);
                }
                else
                {
                    this._logger.Warn("Field with InternalName {0} was not found in list '{1}' fields", field.Id, list.Title);
                }

                defaulView.Update();
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
        #endregion

        #region PublishedLinks
        /// <summary>
        /// Method to create if not exist the publishing link in a Publishing link list of the site
        /// </summary>
        /// <param name="site">The current Site to create the publishing link.</param>
        /// <param name="publishedLink">The publishing link to create</param>
        public void EnsurePublishedLinks(SPSite site, PublishedLink publishedLink)
        {
            var publishedLinksList = this.TryGetList(site.RootWeb, "/PublishedLinks");

            if (publishedLinksList != null && !publishedLinksList.Items.Cast<SPListItem>().Any(link => link.Title == publishedLink.Title))
            {
                var item = publishedLinksList.Items.Add();
                this._binder.FromEntity(publishedLink, item);

                item.Update();
            }
        }

        #endregion PublishedLinks

        /// <summary>
        /// Set default values for a list info objects
        /// </summary>
        /// <param name="list">The list object to configure</param>
        /// <param name="listInfo">The list configuration object</param>
        public void SetDefaultValues(SPList list, ListInfo listInfo)
        {
            if (listInfo.FieldDefinitions.Count > 0)
            {
                foreach (IFieldInfo fieldDefinition in listInfo.FieldDefinitions)
                {
                    // Get the field in the list
                    var field = list.Fields.GetFieldByInternalName(fieldDefinition.InternalName);
                    if (field != null && field.GetType() == typeof(TaxonomyField) && ((fieldDefinition.GetType() == typeof(TaxonomyFieldInfo))))
                    {
                        var taxonomyField = fieldDefinition as TaxonomyFieldInfo;
                       
                        // Get mapping informations
                        var termGroupName = taxonomyField.TermStoreMapping.Group.Name;

                        // Get the term sets according to the default term store language 
                        var termStoreDefaultLanguage =
                            this._taxonomyHelper.GetTermStoreDefaultLanguage(list.ParentWeb.Site);

                        var termSetName = taxonomyField.TermStoreMapping.TermSet.Labels[new CultureInfo(termStoreDefaultLanguage)];
                        var termSubsetName = taxonomyField.TermStoreMapping.TermSubset != null
                            ? taxonomyField.TermStoreMapping.TermSubset.Labels[new CultureInfo(termStoreDefaultLanguage)]
                            : string.Empty;

                        // Get default value informations
                        var taxonomyValue = taxonomyField.DefaultValue;

                        // Change managed metadata mapping
                        this._taxonomyHelper.AssignTermSetToListColumn(list, field.Id, termGroupName, termSetName, termSubsetName);
                        
                        // Set the default value for the field
                        this._taxonomyHelper.SetDefaultTaxonomyFieldValue(list.ParentWeb, field as TaxonomyField, taxonomyValue);
                    }
                    else if (field.GetType() == typeof(SPFieldText) && (fieldDefinition.GetType() == typeof(TextFieldInfo)))
                    {
                        var textField = fieldDefinition as TextFieldInfo;

                        field.DefaultValue = (string)textField.DefaultValue;
                        field.Update();
                    }
                }
            }
        }

        private SPList TryGetList(SPWeb web, string titleOrUrlOrResourceString)
        {
            // first try finding the list by name, simple
            var list = web.Lists.TryGetList(titleOrUrlOrResourceString);

            if (list == null)
            {
                try
                {
                    // second, try to find the list by its web-relative URL
                    list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, titleOrUrlOrResourceString));
                }
                catch (FileNotFoundException)
                {
                    // ignore exception, we need to try a third attempt that assumes the string parameter represents a resource string
                }

                if (list == null && !titleOrUrlOrResourceString.Contains("Lists"))
                {
                    try
                    {
                        // third, try to find the list by its Lists-relative URL by adding Lists if its missing
                        list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, SPUtility.ConcatUrls("Lists", titleOrUrlOrResourceString)));
                    }
                    catch (FileNotFoundException)
                    {
                        // ignore exception, we need to try a third attempt that assumes the string parameter represents a resource string
                    }
                }

                if (list == null)
                {
                    // finally, try to handle the name as a resource key string
                    string[] resourceStringSplit = titleOrUrlOrResourceString.Split(',');
                    string nameFromResourceString = string.Empty;

                    if (resourceStringSplit.Length > 1)
                    {
                        // We're dealing with a resource string which looks like this: $Resources:Some.Namespace,Resource_Key
                        string resourceFileName = resourceStringSplit[0].Replace("$Resources:", string.Empty);
                        nameFromResourceString = this._resourceLocator.Find(resourceFileName, resourceStringSplit[1], web.UICulture.LCID);
                    }
                    else
                    {
                        // let's try to find a resource with that string directly as key
                        nameFromResourceString = this._resourceLocator.Find(titleOrUrlOrResourceString, web.UICulture.LCID);
                    }

                    if (!string.IsNullOrEmpty(nameFromResourceString))
                    {
                        list = web.Lists.TryGetList(nameFromResourceString);
                    }
                }
            }

            return list;
        }
    }
}
