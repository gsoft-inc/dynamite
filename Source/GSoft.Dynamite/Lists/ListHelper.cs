using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using GSoft.Dynamite.Catalogs;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Schemas;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;
using FieldInfo = GSoft.Dynamite.Definitions.FieldInfo;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// Helper class to manage lists.
    /// </summary>
    public class ListHelper
    {
        private readonly ContentTypeBuilder contentTypeBuilder;
        private readonly IResourceLocator resourceLocator;
        private readonly FieldHelper fieldHelper;
        private readonly ILogger logger;

        /// <summary>
        /// Creates a list helper
        /// </summary>
        /// <param name="contentTypeBuilder">A content type helper</param>
        /// <param name="fieldHelper">The field helper.</param>
        /// <param name="resourceLocator">The resource locator</param>
        /// <param name="logger">The logger</param>
        public ListHelper(ContentTypeBuilder contentTypeBuilder, FieldHelper fieldHelper, IResourceLocator resourceLocator, ILogger logger)
        {
            this.contentTypeBuilder = contentTypeBuilder;
            this.fieldHelper = fieldHelper;
            this.resourceLocator = resourceLocator;
            this.logger = logger;
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
        /// Creates the list or returns the existing one.
        /// </summary>
        /// <remarks>The list name and description will not be translated</remarks>
        /// <exception cref="SPException">If the list already exists but doesn't have the specified list template.</exception>
        /// <param name="web">The current web</param>
        /// <param name="catalog">The Catalog to create</param>
        /// <returns>The new list or the existing list</returns>
        public SPList EnsureList(SPWeb web, Catalog catalog)
        {
            return this.EnsureList(web, catalog.RootFolderUrl, catalog.Description, catalog.ListTemplate);
        }

        /// <summary>
        /// Adds the content type id.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="contentTypeId">The content type id.</param>
        /// <exception cref="System.ArgumentNullException">Any null parameters.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">contentTypeId;Content Type not available in the lists parent web.</exception>
        public void AddContentType(SPList list, SPContentTypeId contentTypeId)
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
                this.AddContentType(list, contentType);
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
        public void AddContentType(SPList list, SPContentType contentType)
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

            this.contentTypeBuilder.EnsureContentType(list.ContentTypes, contentType.Id, contentType.Name);
            list.Update(true);
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
            return

                (from SPList list in web.Lists
                 where list.RootFolder.Name.Equals(listRootFolderUrl, StringComparison.Ordinal)
                 select list).FirstOrDefault();
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

            var fieldName = this.fieldHelper.AddField(list.Fields, genericField.ToXElement());

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
            //Retrieve assembly from a puplib class
            Assembly assembly = typeof(Microsoft.SharePoint.Portal.RatingsSettingsPage).Assembly;
            //  Get ReputationHelper type
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
        /// <param name="catalog">the current catalog</param>
        /// <param name="fields">the collection of fields</param>
        public void AddFieldsToDefaultView(SPWeb web, Catalog catalog, ICollection<FieldInfo> fields)
        {
            var list = this.GetListByRootFolderUrl(web, catalog.RootFolderUrl);
            this.AddFieldsToDefaultView(web, list, fields);
        }

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        public void AddFieldsToDefaultView(SPWeb web, SPList list, ICollection<FieldInfo> fields)
        {
            // get the default view of the list
            var defaulView = web.GetViewFromUrl(list.DefaultViewUrl);
            var fieldCollection = defaulView.ViewFields;

            foreach (FieldInfo field in fields)
            {
                if (list.Fields.Contains(field.ID))
                {
                    this.EnsureFieldInView(fieldCollection, list.Fields[field.ID]);
                }
                else
                {
                    this.logger.Warn("Field with ID {0} was not found in list '{1}' fields", field.ID, list.Title);
                }
            }

            defaulView.Update();
        }

        /// <summary>
        /// Ensure the field in the view
        /// </summary>
        /// <param name="fieldCollection">the collection of fields</param>
        /// <param name="field">the current field</param>
        public void EnsureFieldInView(SPViewFieldCollection fieldCollection, SPField field)
        {
            if (!fieldCollection.Exists(field.InternalName))
            {
                fieldCollection.Add(field.InternalName);
            }
        }
        #endregion

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

                if (list == null)
                {
                    // finally, try to handle the name as a resource key string
                    string[] resourceStringSplit = titleOrUrlOrResourceString.Split(',');
                    string nameFromResourceString = string.Empty;

                    if (resourceStringSplit.Length > 1)
                    {
                        // We're dealing with a resource string which looks like this: $Resources:Some.Namespace,Resource_Key
                        nameFromResourceString = this.resourceLocator.Find(resourceStringSplit[1], web.UICulture.LCID);
                    }
                    else
                    {
                        // let's try to find a resource with that string directly as key
                        nameFromResourceString = this.resourceLocator.Find(titleOrUrlOrResourceString, web.UICulture.LCID);
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
