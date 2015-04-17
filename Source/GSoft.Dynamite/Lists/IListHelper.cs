using System.Collections.Generic;
using GSoft.Dynamite.Fields;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// Helper to manage lists.
    /// </summary>
    public interface IListHelper
    {
        /// <summary>Creates the list or returns the existing one.</summary>
        /// <remarks>The list name and description will not be translated</remarks>
        /// <exception cref="SPException">If the list already exists but doesn't have the specified list template.</exception>
        /// <param name="web">The current web</param>
        /// <param name="listInfo">The list Info.</param>
        /// <returns>The new list or the existing list</returns>
        SPList EnsureList(SPWeb web, ListInfo listInfo);

        /// <summary>The ensure list.</summary>
        /// <param name="web">The web.</param>
        /// <param name="listInfos">The list information.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        IEnumerable<SPList> EnsureList(SPWeb web, ICollection<ListInfo> listInfos);

        /// <summary>
        /// Enable or disable ratings on a SPList
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="ratingType">The rating type. Can be "Likes" or "Ratings" </param>
        /// <param name="ratingStatus">True to enable. False to disable.</param>
        void SetRatings(SPList list, string ratingType, bool ratingStatus);

        /// <summary>
        /// Sets the versioning on the list or library.
        /// Note: The minor versioning enabling/disabling is only available on document libraries.
        /// </summary>
        /// <param name="list">The list or library.</param>
        /// <param name="isVersioningEnabled">if set to <c>true</c> [is versioning enabled].</param>
        /// <param name="areMinorVersionsEnabled">if set to <c>true</c> [are minor versions enabled].</param>
        /// <param name="majorVersionLimit">The major version limit (0 is unlimited).</param>
        /// <param name="minorVersionLimit">The minor version limit (0 is unlimited).</param>
        void SetVersioning(
            SPList list,
            bool isVersioningEnabled,
            bool areMinorVersionsEnabled,
            int majorVersionLimit,
            int minorVersionLimit);

        /// <summary>
        ///  Set WriteSecurity on a SPList
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="writeSecurityOptions">The Write Security option</param>
        void SetWriteSecurity(SPList list, WriteSecurityOptions writeSecurityOptions);

        /// <summary>
        /// Method to remove the Item Content Type from the List
        /// </summary>
        /// <param name="list">The current List</param>
        void RemoveItemContentType(SPList list);

        /// <summary>
        /// Set the metadata navigation settings for the current list
        /// </summary>
        /// <param name="web">The web that store the list</param>
        /// <param name="settings">The metadata settings</param>
        void SetMetadataNavigation(SPWeb web, MetadataNavigationSettingsInfo settings);

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        void AddFieldsToDefaultView(SPList list, ICollection<BaseFieldInfo> fields);

        /// <summary>
        /// Add fields in the default view of the list
        /// </summary>
        /// <param name="list">the current list</param>
        /// <param name="fields">the collection of fields</param>
        /// <param name="removeExistingViewFields">if set to <c>true</c> [remove existing view fields].</param>
        void AddFieldsToDefaultView(SPList list, ICollection<BaseFieldInfo> fields, bool removeExistingViewFields);

        /// <summary>
        /// Ensure the field in the view
        /// </summary>
        /// <param name="fieldCollection">the collection of fields</param>
        /// <param name="fieldInternalName">the current field</param>
        void EnsureFieldInView(SPViewFieldCollection fieldCollection, string fieldInternalName);

        /// <summary>
        /// Makes sure the list appears in Quick Launch links on its parent web
        /// </summary>
        /// <param name="list">List that should be added to Quick Launch</param>
        void AddtoQuickLaunch(SPList list);
    }
}
