namespace GSoft.Dynamite.Lists
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using GSoft.Dynamite.Catalogs;
    using GSoft.Dynamite.Fields;
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

        /// <summary>Creates the list or returns the existing one.</summary>
        /// <remarks>The list name and description will not be translated</remarks>
        /// <exception cref="SPException">If the list already exists but doesn't have the specified list template.</exception>
        /// <param name="web">The current web</param>
        /// <param name="listInfo">The list Info.</param>
        /// <returns>The new list or the existing list</returns>
        SPList EnsureList(SPWeb web, ListInfo listInfo);

        /// <summary>The ensure list.</summary>
        /// <param name="web">The web.</param>
        /// <param name="listInfos">The list infos.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        IEnumerable<SPList> EnsureList(SPWeb web, ICollection<ListInfo> listInfos);

        /// <summary>The ensure list.</summary>
        /// <param name="web">The web.</param>
        /// <param name="rootFolderUrl">The root folder url.</param>
        /// <param name="titleResourceKey">The title resource key.</param>
        /// <param name="descriptionResourceKey">The description resource key.</param>
        /// <param name="templateType">The template type.</param>
        /// <returns>The <see cref="SPList"/>.</returns>
        SPList EnsureList(
            SPWeb web,
            string rootFolderUrl,
            string titleResourceKey,
            string descriptionResourceKey,
            SPListTemplateType templateType);

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
        void EnforceUniqueValuesToField(SPList list, IFieldInfo field);

        /// <summary>
        /// Method to remove the Item Content Type from the List
        /// </summary>
        /// <param name="list">The current List</param>
        void RemoveItemContentType(SPList list);

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        void AddFieldsToDefaultView(SPList list, ICollection<IFieldInfo> fields);

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        /// <param name="removeExistingViewFields">if set to <c>true</c> [remove existing view fields].</param>
        void AddFieldsToDefaultView(SPList list, ICollection<IFieldInfo> fields, bool removeExistingViewFields);

        /// <summary>
        /// Ensure the field in the view
        /// </summary>
        /// <param name="fieldCollection">the collection of fields</param>
        /// <param name="fieldInternalName">the current field</param>
        void EnsureFieldInView(SPViewFieldCollection fieldCollection, string fieldInternalName);

        /// <summary>
        /// Makes sure the list appears in Quick Launch links on its parent web
        /// </summary>
        /// <param name="list"></param>
        void AddtoQuickLaunch(SPList list);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="listInfo"></param>
        void SetDefaultValues(SPList list, ListInfo listInfo);
    }
}
