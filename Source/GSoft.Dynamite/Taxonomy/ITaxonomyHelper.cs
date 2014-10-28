namespace GSoft.Dynamite.Taxonomy
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Taxonomy;

    public interface ITaxonomyHelper
    {
        /// <summary>
        /// Assigns a term set to a site column.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreName">The name of the term store.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termStoreName, string termStoreGroupName, string termSetName, string termSubsetName);

        /// <summary>
        /// Assigns a term set to a site column in the default site collection
        /// term store.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termStoreGroupName, string termSetName, string termSubsetName);

        /// <summary>
        /// Assigns a term set to a site column in the default term store from the site collection's reserved group
        /// term store.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termSetName, string termSubsetName);

        /// <summary>
        /// Assigns a term set to a site column in the default site collection
        /// term store.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetId">The ID of the term sub set the term is attached to.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termStoreGroupName, string termSetName, Guid termSubsetId);

        /// <summary>
        /// Assigns a term set to a list column.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreName">The name of the term store.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AssignTermSetToListColumn(SPList list, Guid fieldId, string termStoreName, string termStoreGroupName, string termSetName, string termSubsetName);

        /// <summary>
        /// Assigns a term set to a list column in the default site collection
        /// term store.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetId">The ID of the term sub set the term is attached to.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AssignTermSetToListColumn(SPList list, Guid fieldId, string termStoreGroupName, string termSetName, Guid termSubsetId);

        /// <summary>
        /// Assigns a term set to a site column in the default site collection
        /// term store.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AssignTermSetToListColumn(SPList list, Guid fieldId, string termStoreGroupName, string termSetName, string termSubsetName);

        /// <summary>
        /// Ensures the taxonomy event receivers.
        /// </summary>
        /// <param name="eventReceivers">The event receivers definition collection.</param>
        /// <exception cref="System.ArgumentNullException">All null parameters.</exception>
        void EnsureTaxonomyEventReceivers(SPEventReceiverDefinitionCollection eventReceivers);

        /// <summary>
        /// Changes the Enterprise Keywords setting on a list
        /// </summary>
        /// <remarks>To disable Enterprise Keywords, delete the field from the list manually.</remarks>
        /// <param name="list">The list</param>
        /// <param name="keywordsAsSocialTags">Whether the list's keywords should be used as MySite social tags</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void EnableListEnterpriseKeywordsSetting(SPList list, bool keywordsAsSocialTags);

        /// <summary>
        /// Get the validated string for a Taxonomy Field
        /// </summary>
        /// <param name="web">Web to look for</param>
        /// <param name="fieldName">Field to search</param>
        /// <param name="termGroup">The term group</param>
        /// <param name="termSet">The term set</param>
        /// <param name="termLabel">The term label</param>
        /// <returns>The validated string.</returns>
        string GetTaxonomyFieldValueValidatedString(SPWeb web, string fieldName, string termGroup, string termSet, string termLabel);

        /// <summary>
        /// Set default value for a taxonomy site column
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="field">The field.</param>
        /// <param name="termGroupName">The term group name.</param>
        /// <param name="termSetName">the term set name.</param>
        /// <param name="termLabel">The term label.</param>
        void SetDefaultTaxonomyValue(SPWeb web, SPField field, string termGroupName, string termSetName, string termLabel);

        /// <summary>
        /// Set default value for a multi valued taxonomy site column
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="field">The field.</param>
        /// <param name="termGroupName">Term group name</param>
        /// <param name="termSetName">Term set name</param>
        /// <param name="terms">Term label</param>
        void SetDefaultTaxonomyMultiValue(
            SPWeb web, SPField field, string termGroupName, string termSetName, string[] terms);

        /// <summary>
        /// Gets the term group by name.
        /// </summary>
        /// <param name="termStore">The term store.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>
        /// The term group.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        Group GetTermGroupByName(TermStore termStore, string groupName);

        /// <summary>
        /// Gets the term set by name.
        /// </summary>
        /// <param name="termStore">The term store.</param>
        /// <param name="group">The term group.</param>
        /// <param name="termSetName">Name of the term set.</param>
        /// <returns>The term set.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        TermSet GetTermSetByName(TermStore termStore, Group group, string termSetName);

        /// <summary>
        /// Set a taxonomy value for a SPListItem
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="item">The SPListItem.</param>
        /// <param name="fieldName">Field name to update.</param>
        /// <param name="termGroupName">Term group name.</param>
        /// <param name="termSetName">Term Set Name.</param>
        /// <param name="termLabel">Term Label.</param>
        void SetTaxonomyFieldValue(SPWeb web, SPListItem item, string fieldName, string termGroupName, string termSetName, string termLabel);
    }
}