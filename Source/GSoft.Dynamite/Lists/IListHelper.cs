namespace GSoft.Dynamite.Lists
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using GSoft.Dynamite.Catalogs;
    using GSoft.Dynamite.Definitions;
    using GSoft.Dynamite.Lists.Entities;
    using GSoft.Dynamite.Schemas;

    using Microsoft.SharePoint;

    public interface IListHelper
    {
        /// <summary>
        /// Finds the list template corresponding to the specified name
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the template does not exist</exception>
        /// <param name="web">The current web</param>
        /// <param name="templateName">The list template name</param>
        /// <returns>The list template</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        SPListTemplate GetListTemplate(SPWeb web, string templateName);

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
        SPList EnsureList(SPWeb web, string name, string description, SPListTemplate template);

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
        SPList EnsureList(SPWeb web, string name, string description, SPListTemplateType templateType);

        /// <summary>
        /// Creates the list or returns the existing one.
        /// </summary>
        /// <remarks>The list name and description will not be translated</remarks>
        /// <exception cref="SPException">If the list already exists but doesn't have the specified list template.</exception>
        /// <param name="web">The current web</param>
        /// <param name="catalog">The Catalog to create</param>
        /// <returns>The new list or the existing list</returns>
        SPList EnsureList(SPWeb web, Catalog catalog);

        /// <summary>
        /// Adds the content type id.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="contentTypeId">The content type id.</param>
        /// <exception cref="System.ArgumentNullException">Any null parameters.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">contentTypeId;Content Type not available in the lists parent web.</exception>
        void AddContentType(SPList list, SPContentTypeId contentTypeId);

        /// <summary>
        /// Adds the content type.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">Any null parameter.</exception>
        void AddContentType(SPList list, SPContentType contentType);

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
        SPList GetListByRootFolderUrl(SPWeb web, string listRootFolderUrl);

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
        SPField CreateListField(SPList list, GenericFieldSchema genericField, string fieldInternalName, string fieldDisplayName, string fieldDescription, string fieldGroup);

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
        SPField CreateListTaxonomyField(SPList list, string fieldInternalName, string fieldDisplayName, string fieldDescription, string fieldGroup, bool isMultiple, bool isOpen);

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
        SPField CreateTextField(SPList list, string fieldInternalName, string fieldDisplayName, string fieldDescription, string fieldGroup, bool isMultiLines);

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
        SPField CreateGuidField(SPList list, string fieldInternalName, string fieldDisplayName, string fieldDescription, string fieldGroup);

        /// <summary>
        /// Enable or disable ratings on a SPList
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="ratingType">The rating type. Can be "Likes" or "Ratings" </param>
        /// <param name="ratingStatus">True to enable. False to disable.</param>
        void SetRatings(SPList list, string ratingType, bool ratingStatus);

        /// <summary>
        ///  Set WriteSecurity on a SPList
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="option">The Write Security option</param>
        void SetWriteSecurity(SPList list, WriteSecurityOptions option);

        /// <summary>
        /// Enforce the unique value(s) for a list field. In case the field is reused in the site collection, we can make that change on the list scope.
        /// </summary>
        /// <param name="list">The list who owns the field</param>
        /// <param name="field">The field to enforce</param>
        void EnforceUniqueValuesToField(SPList list, FieldInfo field);

        /// <summary>
        /// Method to remove the Item Content Type from the List
        /// </summary>
        /// <param name="list">The current List</param>
        void RemoveItemContentType(SPList list);

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="catalog">the current catalog</param>
        /// <param name="fields">the collection of fields</param>
        void AddFieldsToDefaultView(SPWeb web, Catalog catalog, ICollection<FieldInfo> fields);

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="catalog">the current catalog</param>
        /// <param name="fields">the collection of fields</param>
        /// <param name="removeExistingViewFields">if set to <c>true</c> [remove existing view fields].</param>
        void AddFieldsToDefaultView(SPWeb web, Catalog catalog, ICollection<FieldInfo> fields, bool removeExistingViewFields);

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        void AddFieldsToDefaultView(SPWeb web, SPList list, ICollection<FieldInfo> fields);

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        /// <param name="removeExistingViewFields">if set to <c>true</c> [remove existing view fields].</param>
        void AddFieldsToDefaultView(SPWeb web, SPList list, ICollection<FieldInfo> fields, bool removeExistingViewFields);

        /// <summary>
        /// Ensure the field in the view
        /// </summary>
        /// <param name="fieldCollection">the collection of fields</param>
        /// <param name="field">the current field</param>
        void EnsureFieldInView(SPViewFieldCollection fieldCollection, SPField field);

        /// <summary>
        /// Method to create if not exist the publishing link in a Publishing link list of the site
        /// </summary>
        /// <param name="site">The current Site to create the publishing link.</param>
        /// <param name="publishedLink">The publishing link to create</param>
        void EnsurePublishedLinks(SPSite site, PublishedLink publishedLink);
    }
}
