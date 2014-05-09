using System;
using System.Collections.Generic;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Interface for interacting with the Managed Metadata Service.
    /// </summary>
    /// <remarks>
    /// For all methods: if a term or a term set is not found by its default label 
    /// in the term store's default working language, the other alternate available 
    /// languages should be attempted.
    /// </remarks>
    public interface ITaxonomyService
    {
        /// <summary>
        /// Retrieves a TaxonomyValue corresponding to a term label within a desired term store
        /// </summary>
        /// <remarks>If many terms are found with the corresponding label, a root term is returned if found.</remarks>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The taxonomy value or null if not found</returns>
        TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel);

        /// <summary>
        /// Retrieves a TaxonomyValue corresponding to a term label within the default term store
        /// </summary>
        /// <remarks>If many terms are found with the corresponding label, a root term is returned if found.</remarks>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The taxonomy value or null if not found</returns>
        TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel);

        /// <summary>
        /// Retrieves a TaxonomyValue corresponding to a term label within the default term store in the site collection's reserved group
        /// </summary>
        /// <remarks>
        /// Use other overloads and specify a group name to fetch from farm-global term sets instead of being limited 
        /// to the site collection's associated term group
        /// </remarks>
        /// <param name="site">The current site</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The taxonomy value or null if not found</returns>
        TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termSetName, string termLabel);

        /// <summary>
        /// Retrieves a Term corresponding to a term label within a desired term store
        /// </summary>
        /// <remarks>If many terms are found with the corresponding label, a root term is returned if found.</remarks>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The term or null if not found</returns>
        Term GetTermForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel);

        /// <summary>
        /// Retrieves a Term corresponding to a term label within the default term store
        /// </summary>
        /// <remarks>If many terms are found with the corresponding label, a root term is returned if found.</remarks>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The term or null if not found</returns>
        Term GetTermForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel);

        /// <summary>
        /// Retrieves a Term corresponding to a term label within the default term store in the site collection's reserved group
        /// </summary>
        /// <remarks>
        /// Use other overloads and specify a group name to fetch from farm-global term sets instead of being limited 
        /// to the site collection's associated term group
        /// </remarks>
        /// <param name="site">The current site</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The term or null if not found</returns>
        Term GetTermForLabel(SPSite site, string termSetName, string termLabel);

        /// <summary>
        /// Gets the term for identifier.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>The term</returns>
        Term GetTermForId(SPSite site, Guid id);

		/// Gets the term for identifier.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="termStoreName">Name of the term store.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>The specific term</returns>
        Term GetTermForId(SPSite site, string termStoreName, Guid id);
		
        /// <summary>
        /// Gets the term for identifier within site collection specific term store group.
        /// </summary>
        /// <param name="site">The Site.</param>
        /// <param name="termSetName">The name of the term set containing the term</param>
        /// <param name="id">The GUID of the term to get.</param>
        /// <returns>The term found</returns>
        Term GetTermForIdInTermSet(SPSite site, string termSetName, Guid id);

        /// <summary>
        /// Gets the term for identifier
        /// </summary>
        /// <param name="site">The Site.</param>
        /// <param name="termStoreGroupName">The Group Name in the term store</param>
        /// <param name="termSetName">The name of the term set containing the term</param>
        /// <param name="id">The GUID of the term to get.</param>
        /// <returns>The term</returns>
        Term GetTermForIdInTermSet(SPSite site, string termStoreGroupName, string termSetName, Guid id);

        /// <summary>
        /// Retrieves all TaxonomyValues corresponding to a term label within a desired term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>A list of taxonomy values</returns>
        IList<TaxonomyValue> GetTaxonomyValuesForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel);

        /// <summary>
        /// Retrieves all TaxonomyValues corresponding to a term label within the default term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>A list of taxonomy values</returns>
        IList<TaxonomyValue> GetTaxonomyValuesForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel);

        /// <summary>
        /// Retrieves all TaxonomyValues corresponding to a term label within the default term store in the site collection's reserved group
        /// </summary>
        /// <remarks>
        /// Use other overloads and specify a group name to fetch from farm-global term sets instead of being limited 
        /// to the site collection's associated term group
        /// </remarks>
        /// <param name="site">The current site</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>A list of taxonomy values</returns>
        IList<TaxonomyValue> GetTaxonomyValuesForLabel(SPSite site, string termSetName, string termLabel);

        /// <summary>
        /// Retrieves all terms corresponding to a term label within a desired term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>A list of terms</returns>
        IList<Term> GetTermsForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel);

        /// <summary>
        /// Retrieves a Term corresponding to a term label within the default term store
        /// </summary>
        /// <remarks>If many terms are found with the corresponding label, a root term is returned if found.</remarks>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>A list of terms</returns>
        IList<Term> GetTermsForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel);

        /// <summary>
        /// Retrieves a Term corresponding to a term label within the default term store in the site collection's reserved group
        /// </summary>
        /// <remarks>
        /// Use other overloads and specify a group name to fetch from farm-global term sets instead of being limited 
        /// to the site collection's associated term group
        /// </remarks>
        /// <param name="site">The current site</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>A list of terms</returns>
        IList<Term> GetTermsForLabel(SPSite site, string termSetName, string termLabel);

        /// <summary>
        /// Retrieves all TaxonomyValues corresponding to a term store term set
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termStoreName, string termStoreGroupName, string termSetName);

        /// <summary>
        /// Retrieves all TaxonomyValues corresponding to a term set in the default term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termStoreGroupName, string termSetName);

        /// <summary>
        /// Retrieves all TaxonomyValues corresponding to a term set in the default term store from the site collection's reserved group
        /// </summary>
        /// <remarks>
        /// Use other overloads and specify a group name to fetch from farm-global term sets instead of being limited 
        /// to the site collection's associated term group
        /// </remarks>
        /// <param name="site">The current site</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termSetName);

        /// <summary>
        /// Retrieves all Terms corresponding to a term store term set
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        IList<Term> GetTermsForTermSet(SPSite site, string termStoreName, string termStoreGroupName, string termSetName);

        /// <summary>
        /// Retrieves all Terms corresponding to a term set in the default term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        IList<Term> GetTermsForTermSet(SPSite site, string termStoreGroupName, string termSetName);

        /// <summary>
        /// Get all parent terms from source term to root term in the term set
        /// </summary>
        /// <param name="site">The current site collection.</param>
        /// <param name="termSetId">The term set id.</param>
        /// <param name="termId">The term.</param>
        /// <param name="parentFirst">if set to <c>true</c>, includes the [parent first].</param>
        /// <returns>
        /// List of terms.
        /// </returns>
        IList<Term> GetTermPathFromRootToTerm(SPSite site, Guid termSetId, Guid termId, bool parentFirst = false);

        /// <summary>
        /// Retrieves all Terms corresponding to a term set in the default term store from the site collection's reserved group
        /// </summary>
        /// <remarks>
        /// Use other overloads and specify a group name to fetch from farm-global term sets instead of being limited 
        /// to the site collection's associated term group
        /// </remarks>
        /// <param name="site">The current site</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        IList<Term> GetTermsForTermSet(SPSite site, string termSetName);
    }
}
