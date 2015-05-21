namespace GSoft.Dynamite.Taxonomy
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using GSoft.Dynamite.ValueTypes;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Taxonomy;

    /// <summary>
    /// Helper for managing Taxonomy.
    /// </summary>
    public interface ITaxonomyHelper
    {
        /// <summary>
        /// Applies a term store mapping to a SharePoint field
        /// </summary>
        /// <param name="site">The current site collection</param>
        /// <param name="field">The site or list column to map to the term store</param>
        /// <param name="columnTermStoreMapping">
        /// The term set or sub-term-specific anchor which will determine what's available in the field's taxonomy picker
        /// </param>
        void AssignTermStoreMappingToField(SPSite site, SPField field, TaxonomyContext columnTermStoreMapping);

        /// <summary>
        /// Assigns a term set to a site column.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreName">The name of the term store.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
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
        void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termStoreGroupName, string termSetName, string termSubsetName);

        /// <summary>
        /// Assigns a term set to a site column in the default term store from the site collection's reserved group
        /// term store.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, string termSetName, string termSubsetName);

        /// <summary>
        /// Assigns a term set to a site column in the default site collection
        /// term store.
        /// </summary>
        /// <param name="web">The web containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupId">The term store group identifier.</param>
        /// <param name="termSetId">The term set identifier.</param>
        /// <param name="termSubsetId">The ID of the term sub set the term is attached to.</param>
        void AssignTermSetToSiteColumn(SPWeb web, Guid fieldId, Guid termStoreGroupId, Guid termSetId, Guid termSubsetId);

        /// <summary>
        /// Assigns a term set to a list column.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreName">The name of the term store.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        void AssignTermSetToListColumn(SPList list, Guid fieldId, string termStoreName, string termStoreGroupName, string termSetName, string termSubsetName);

        /// <summary>
        /// Assigns a term set to a list column in the default site collection
        /// term store.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupId">The term store group identifier.</param>
        /// <param name="termSetId">The term set identifier.</param>
        /// <param name="termSubsetId">The ID of the term sub set the term is attached to.</param>
        void AssignTermSetToListColumn(SPList list, Guid fieldId, Guid termStoreGroupId, Guid termSetId, Guid termSubsetId);

        /// <summary>
        /// Assigns a global farm-wide term set to a list column
        /// term store.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termStoreGroupName">The name of the term store group.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        void AssignTermSetToListColumn(SPList list, Guid fieldId, string termStoreGroupName, string termSetName, string termSubsetName);

        /// <summary>
        /// Assigns a local site-collection-specific term set to a list column
        /// term store.
        /// </summary>
        /// <param name="list">The list containing the field.</param>
        /// <param name="fieldId">The field to associate with the term set.</param>
        /// <param name="termSetName">The name of the term set to assign to the column.</param>
        /// <param name="termSubsetName">The name of the term sub set the term is attached to. This parameter can be null.</param>
        void AssignTermSetToListColumn(SPList list, Guid fieldId, string termSetName, string termSubsetName);

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
        void EnableListEnterpriseKeywordsSetting(SPList list, bool keywordsAsSocialTags);

        /// <summary>
        /// Gets the default term store for the site collection.
        /// </summary>
        /// <param name="session">The taxonomy session.</param>
        TermStore GetDefaultSiteCollectionTermStore(TaxonomySession session);
    }
}