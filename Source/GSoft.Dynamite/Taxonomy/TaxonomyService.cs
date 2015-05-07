using System;
using System.Collections.Generic;
using System.Linq;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Helper class for interacting with the Managed Metadata Service
    /// </summary>
    /// <remarks>
    /// For all methods: if a term or a term set is not found by its default label 
    /// in the term store's default working language, the other alternate available 
    /// languages should be attempted.
    /// </remarks>
    public class TaxonomyService : ITaxonomyService
    {
        private ISiteTaxonomyCacheManager taxonomyCacheManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyService" /> class.
        /// </summary>
        /// <param name="taxonomyCacheManager">The taxonomy cache manager</param>
        public TaxonomyService(ISiteTaxonomyCacheManager taxonomyCacheManager)
        {
            this.taxonomyCacheManager = taxonomyCacheManager;
        }

        #region GetTaxonomyValueForLabel overloads

        /// <summary>
        /// Retrieves a TaxonomyValue corresponding to a term label within a desired term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The taxonomy value or null if not found</returns>
        public TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, termStoreName).TaxonomySession;
            TermStore termStore = session.TermStores[termStoreName];

            return GetTaxonomyValueForLabelInternal(termStore, termStoreGroupName, termSetName, termLabel);
        }

        /// <summary>
        /// Retrieves a TaxonomyValue corresponding to a term label within the default term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The taxonomy value or null if not found</returns>
        public TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null).TaxonomySession;
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTaxonomyValueForLabelInternal(termStore, termStoreGroupName, termSetName, termLabel);
        }

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
        public TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termSetName, string termLabel)
        {
            SiteTaxonomyCache taxCache = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null);
            TermStore termStore = taxCache.TaxonomySession.DefaultSiteCollectionTermStore;
            Group siteCollectionGroup = taxCache.SiteCollectionGroup;
            TermSet termSet = this.GetTermSetFromGroup(termStore, siteCollectionGroup, termSetName);

            return GetTaxonomyValueForLabelInternal(termStore, siteCollectionGroup, termSet, termLabel);
        }

        #endregion

        #region GetTermForLabel overloads

        /// <summary>
        /// Retrieves a Term corresponding to a term label within a desired term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The term or null if not found</returns>
        public Term GetTermForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, termStoreName).TaxonomySession;
            TermStore termStore = session.TermStores[termStoreName];

            return GetTermForLabelInternal(termStore, termStoreGroupName, termSetName, termLabel);
        }

        /// <summary>
        /// Retrieves a Term corresponding to a term label within the default term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The term or null if not found</returns>
        public Term GetTermForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null).TaxonomySession;
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTermForLabelInternal(termStore, termStoreGroupName, termSetName, termLabel);
        }

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
        public Term GetTermForLabel(SPSite site, string termSetName, string termLabel)
        {
            SiteTaxonomyCache taxCache = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null);
            TermStore termStore = taxCache.TaxonomySession.DefaultSiteCollectionTermStore;
            Group siteCollectionGroup = taxCache.SiteCollectionGroup;
            TermSet termSet = this.GetTermSetFromGroup(termStore, siteCollectionGroup, termSetName);

            return GetTermForLabelInternal(termStore, siteCollectionGroup, termSet, termLabel);
        }

        /// <summary>
        /// Gets the term for identifier.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>The term</returns>
        public Term GetTermForId(SPSite site, Guid id)
        {
            return this.GetTermForId(site, null, id);
        }

        /// <summary>
        /// Gets the term for identifier.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="termStoreName">Name of the term store.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>The specific term.</returns>
        public Term GetTermForId(SPSite site, string termStoreName, Guid id)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, termStoreName).TaxonomySession;
            return session.GetTerm(id);
        }

        /// <summary>
        /// Gets the term for identifier within site collection specific term store group.
        /// </summary>
        /// <param name="site">The Site.</param>
        /// <param name="termSetName">The name of the term set containing the term</param>
        /// <param name="id">The GUID of the term to get.</param>
        /// <returns>The term</returns>
        public Term GetTermForIdInTermSet(SPSite site, string termSetName, Guid id)
        {
            SiteTaxonomyCache taxCache = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null);
            TermStore termStore = taxCache.TaxonomySession.DefaultSiteCollectionTermStore;
            Group siteCollectionGroup = taxCache.SiteCollectionGroup;
            TermSet termSet = this.GetTermSetFromGroup(termStore, siteCollectionGroup, termSetName);

            return termSet.GetTerm(id);
        }

        /// <summary>
        /// Gets the term for identifier
        /// </summary>
        /// <param name="site">The Site.</param>
        /// <param name="termStoreGroupName">The Group Name in the term store</param>
        /// <param name="termSetName">The name of the term set containing the term</param>
        /// <param name="id">The GUID of the term to get.</param>
        /// <returns>The term</returns>
        public Term GetTermForIdInTermSet(SPSite site, string termStoreGroupName, string termSetName, Guid id)
        {
            SiteTaxonomyCache taxCache = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null);
            TermStore termStore = taxCache.TaxonomySession.DefaultSiteCollectionTermStore;
            Group siteCollectionGroup = GetGroupFromTermStore(termStore, termStoreGroupName);
            TermSet termSet = this.GetTermSetFromGroup(termStore, siteCollectionGroup, termSetName);

            return termSet.GetTerm(id);
        }

        #endregion

        #region GetTaxonomyValuesForLabel overloads

        /// <summary>
        /// Retrieves all TaxonomyValues corresponding to a term label within a desired term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>A list of taxonomy values</returns>
        public IList<TaxonomyValue> GetTaxonomyValuesForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, termStoreName).TaxonomySession;
            TermStore termStore = session.TermStores[termStoreName];

            return GetTaxonomyValuesForLabelInternal(termStore, termStoreGroupName, termSetName, termLabel);
        }

        /// <summary>
        /// Retrieves all TaxonomyValues corresponding to a term label within the default term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>A list of taxonomy values</returns>
        public IList<TaxonomyValue> GetTaxonomyValuesForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null).TaxonomySession;
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTaxonomyValuesForLabelInternal(termStore, termStoreGroupName, termSetName, termLabel);
        }

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
        public IList<TaxonomyValue> GetTaxonomyValuesForLabel(SPSite site, string termSetName, string termLabel)
        {
            SiteTaxonomyCache taxCache = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null);
            TermStore termStore = taxCache.TaxonomySession.DefaultSiteCollectionTermStore;
            Group siteCollectionGroup = taxCache.SiteCollectionGroup;
            TermSet termSet = this.GetTermSetFromGroup(termStore, siteCollectionGroup, termSetName);

            return GetTaxonomyValuesForLabelInternal(termStore, siteCollectionGroup, termSet, termLabel);
        }

        #endregion

        #region GetTaxonomyValuesForTermSet overloads

        /// <summary>
        /// Retrieves all TaxonomyValues corresponding to a term store term set
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        public IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termStoreName, string termStoreGroupName, string termSetName)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, termStoreName).TaxonomySession;
            TermStore termStore = session.TermStores[termStoreName];

            return GetTaxonomyValuesForTermSetInternal(termStore, termStoreGroupName, termSetName);
        }

        /// <summary>
        /// Retrieves all TaxonomyValues corresponding to a term store term set
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        public IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termStoreGroupName, string termSetName)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null).TaxonomySession;
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTaxonomyValuesForTermSetInternal(termStore, termStoreGroupName, termSetName);
        }

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
        public IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termSetName)
        {
            SiteTaxonomyCache taxCache = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null);
            TermStore termStore = taxCache.TaxonomySession.DefaultSiteCollectionTermStore;
            Group siteCollectionGroup = taxCache.SiteCollectionGroup;
            TermSet termSet = this.GetTermSetFromGroup(termStore, siteCollectionGroup, termSetName);

            return GetTaxonomyValuesForTermSetInternal(termStore, siteCollectionGroup, termSet);
        }

        #endregion

        #region GetTermsForLabel overloads

        /// <summary>
        /// Retrieves all terms corresponding to a term label within a desired term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>A list of terms</returns>
        public IList<Term> GetTermsForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, termStoreName).TaxonomySession;
            TermStore termStore = session.TermStores[termStoreName];

            return GetTermsForLabelInternal(termStore, termStoreGroupName, termSetName, termLabel);
        }

        /// <summary>
        /// Retrieves a Term corresponding to a term label within the default term store
        /// </summary>
        /// <remarks>If many terms are found with the corresponding label, a root term is returned if found.</remarks>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>A list of terms</returns>
        public IList<Term> GetTermsForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null).TaxonomySession;
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTermsForLabelInternal(termStore, termStoreGroupName, termSetName, termLabel);
        }

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
        public IList<Term> GetTermsForLabel(SPSite site, string termSetName, string termLabel)
        {
            SiteTaxonomyCache taxCache = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null);
            TermStore termStore = taxCache.TaxonomySession.DefaultSiteCollectionTermStore;
            Group siteCollectionGroup = taxCache.SiteCollectionGroup;
            TermSet termSet = this.GetTermSetFromGroup(termStore, siteCollectionGroup, termSetName);

            return GetTermsForLabelInternal(termStore, siteCollectionGroup, termSet, termLabel);
        }

        #endregion

        #region GetTermsForTermSet overloads

        /// <summary>
        /// Retrieves all terms corresponding to a term label within a desired term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of terms</returns>
        public IList<Term> GetTermsForTermSet(SPSite site, string termStoreName, string termStoreGroupName, string termSetName)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, termStoreName).TaxonomySession;
            TermStore termStore = session.TermStores[termStoreName];

            return GetTermsForTermSetInternal(termStore, termStoreGroupName, termSetName);
        }

        /// <summary>
        /// Retrieves all terms corresponding to a term label within a desired term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of terms</returns>
        public IList<Term> GetTermsForTermSet(SPSite site, string termStoreGroupName, string termSetName)
        {
            TaxonomySession session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null).TaxonomySession;
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTermsForTermSetInternal(termStore, termStoreGroupName, termSetName);
        }

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
        public IList<Term> GetTermsForTermSet(SPSite site, string termSetName)
        {
            SiteTaxonomyCache taxCache = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null);
            TermStore termStore = taxCache.TaxonomySession.DefaultSiteCollectionTermStore;
            Group siteCollectionGroup = taxCache.SiteCollectionGroup;
            TermSet termSet = this.GetTermSetFromGroup(termStore, siteCollectionGroup, termSetName);

            return GetTermsForTermSetInternal(termStore, siteCollectionGroup, termSet);
        }

        /// <summary>
        /// Retrieves all terms used as simple link navigation nodes corresponding to a term set within a desired term store.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="termStoreName">Name of the term store.</param>
        /// <param name="termStoreGroupName">Name of the term store group.</param>
        /// <param name="termSetName">Name of the term set.</param>
        /// <returns>A list of terms used as simple link navigation nodes.</returns>
        public IList<SimpleLinkTermInfo> GetTermsAsSimpleLinkNavNodeForTermSet(SPSite site, string termStoreName, string termStoreGroupName, string termSetName)
        {
            var session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, termStoreName).TaxonomySession;
            var termStore = session.TermStores[termStoreName];

            return this.GetTermsAsSimpleLinkNavNodeForTermSetInternal(termStore, termStoreGroupName, termSetName);
        }

        /// <summary>
        /// Retrieves all terms used as simple link navigation nodes corresponding to a term set within the default term store.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="termStoreGroupName">Name of the term store group.</param>
        /// <param name="termSetName">Name of the term set.</param>
        /// <returns>A list of terms used as simple link navigation nodes.</returns>
        public IList<SimpleLinkTermInfo> GetTermsAsSimpleLinkNavNodeForTermSet(SPSite site, string termStoreGroupName, string termSetName)
        {
            var session = this.taxonomyCacheManager.GetSiteTaxonomyCache(site, null).TaxonomySession;
            var termStore = session.DefaultSiteCollectionTermStore;

            return this.GetTermsAsSimpleLinkNavNodeForTermSetInternal(termStore, termStoreGroupName, termSetName);
        }

        #endregion

        /// <summary>
        /// Gets the term set from group.
        /// </summary>
        /// <param name="termStore">The term store.</param>
        /// <param name="group">The group.</param>
        /// <param name="termSetName">Name of the term set.</param>
        /// <returns>The term set for the specified store, group and term set name.</returns>
        /// <exception cref="System.ArgumentException">Could not find term set with name  + termSetName +  in group  + group.Name</exception>
        public TermSet GetTermSetFromGroup(TermStore termStore, Group group, string termSetName)
        {
            // Always interact with the term sets in the term store's default language
            int originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = termStore.DefaultLanguage;

            TermSet termSet = group.TermSets[termSetName];

            if (termSet == null)
            {
                throw new ArgumentException("Could not find term set with name " + termSetName + " in group " + group.Name);
            }

            termStore.WorkingLanguage = originalWorkingLanguage;

            return termSet;
        }
        
        /// <summary>
        /// Gets the term set group from the term store.
        /// </summary>
        /// <param name="termStore">The term store.</param>
        /// <param name="groupName">The term set group name, in the term store's default working language.</param>
        /// <returns>The term set group</returns>
        public Group GetTermGroupFromStore(TermStore termStore, string groupName)
        {
            int originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = termStore.DefaultLanguage;

            Group group = termStore.Groups[groupName];

            if (group == null)
            {
                throw new ArgumentException("Could not find term set group with name " + groupName + " in term store " + termStore.Name);
            }

            termStore.WorkingLanguage = originalWorkingLanguage;

            return group;
        }

        #region GetTermPathFromRootToTerm
        /// <summary>
        /// Get all parent terms from a source term to root term in the term set.
        /// </summary>
        /// <param name="site">The current site collection.</param>
        /// <param name="termSetId">The term set id.</param>
        /// <param name="termId">The term.</param>
        /// <param name="parentFirst">if set to <c>true</c>, includes the [parent first].</param>
        /// <returns>
        /// List of terms.
        /// </returns>
        public IList<Term> GetTermPathFromRootToTerm(SPSite site, Guid termSetId, Guid termId, bool parentFirst)
        {
            IList<Term> termHierarchy = new List<Term>();

            var session = new TaxonomySession(site);
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            // Always interact with the term sets in the term store's default language
            int originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = termStore.DefaultLanguage;

            var rootTermReached = false;

            // Get the original term
            var term = termStore.GetTerm(termSetId, termId);

            while (term != null && !rootTermReached)
            {
                termHierarchy.Add(term);
                if (term.Parent != null)
                {
                    term = termStore.GetTerm(termSetId, term.Parent.Id);
                }
                else
                {
                    rootTermReached = true;
                }
            }

            termStore.WorkingLanguage = originalWorkingLanguage;
            return parentFirst ? termHierarchy.Reverse().ToList() : termHierarchy;
        }
        #endregion GetTermPathFromRootToTerm

        #region Private utility methods     

        private static TaxonomyValue GetTaxonomyValueForLabelInternal(TermStore termStore, Group termStoreGroup, TermSet termSet, string termLabel)
        {
            Term term = GetTermForLabelInternal(termStore, termStoreGroup, termSet, termLabel);

            TaxonomyValue value = null;
            if (term != null)
            {
                value = new TaxonomyValue(term);
            }

            return value;
        }

        private static Term GetTermForLabelInternal(TermStore termStore, Group termStoreGroup, TermSet termSet, string termLabel)
        {
            Term term = null;
            IList<Term> terms = GetTermsForLabelInternal(termStore, termStoreGroup, termSet, termLabel);

            if (terms.Count > 1)
            {
                // More than one hit, we'd prefer a root term
                term = terms.FirstOrDefault(maybeRootTerm => maybeRootTerm.IsRoot);
            }

            // A root term was not found, let's just use the first one we find
            if (term == null)
            {
                term = terms.FirstOrDefault();
            }

            return term;
        }

        private static IList<TaxonomyValue> GetTaxonomyValuesForLabelInternal(TermStore termStore, Group termStoreGroup, TermSet termSet, string termLabel)
        {
            IList<Term> terms = GetTermsForLabelInternal(termStore, termStoreGroup, termSet, termLabel);
            return terms.Select(term => new TaxonomyValue(term)).ToList();
        }

        private static IList<TaxonomyValue> GetTaxonomyValuesForTermSetInternal(TermStore termStore, Group termStoreGroup, TermSet termSet)
        {
            IList<TaxonomyValue> termsList = new List<TaxonomyValue>();
            IList<Term> terms = GetTermsForTermSetInternal(termStore, termStoreGroup, termSet);

            if (terms != null && terms.Count > 0)
            {
                termsList = terms.Select(term => new TaxonomyValue(term)).ToList();
            }

            return termsList;
        }

        private static IList<Term> GetTermsForLabelInternal(TermStore termStore, Group termStoreGroup, TermSet termSet, string termLabel)
        {
            if (termStore == null)
            {
                throw new ArgumentNullException("termStore");
            }

            if (termStoreGroup == null)
            {
                throw new ArgumentNullException("termStoreGroup");
            }

            if (termSet == null)
            {
                throw new ArgumentNullException("termSet");
            }

            if (string.IsNullOrEmpty(termLabel))
            {
                throw new ArgumentNullException("termLabel");
            }

            TermCollection termCollection = termSet.GetAllTerms();
            return termCollection.Where((term) =>
            {
                return term.Labels.Any(label => label.Value == termLabel);
            }).ToList();
        }

        private static IList<Term> GetTermsForTermSetInternal(TermStore termStore, Group termStoreGroup, TermSet termSet)
        {
            if (termStore == null)
            {
                throw new ArgumentNullException("termStore");
            }

            if (termStoreGroup == null)
            {
                throw new ArgumentNullException("termStoreGroup");
            }

            if (termSet == null)
            {
                throw new ArgumentNullException("termSet");
            }

            IList<Term> termsList = new List<Term>();

            if (termSet.Terms.Any())
            {
                // If custom sort order is set, build term list in sorted order
                var customSortOrder = termSet.CustomSortOrder;
                if (!string.IsNullOrEmpty(customSortOrder))
                {
                    var terms = termSet.Terms.ToList();
                    var sortedIds = customSortOrder.Split(':').Select(id => new Guid(id)).ToList();
                    foreach (var sortedId in sortedIds)
                    {
                        var sortedTerm = terms.SingleOrDefault(term => term.Id.Equals(sortedId));
                        if (sortedTerm != null)
                        {
                            termsList.Add(sortedTerm);
                        }
                    }
                }
                else
                {
                    termsList = termSet.Terms.ToList();
                }
            }

            return termsList;
        }

        private static Group GetGroupFromTermStore(TermStore termStore, string groupName)
        {
            // Always interact with the term sets in the term store's default language
            int originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = termStore.DefaultLanguage;

            Group group = termStore.Groups[groupName];

            if (group == null)
            {
                throw new ArgumentException("Could not find term store group with name " + groupName);
            }

            termStore.WorkingLanguage = originalWorkingLanguage;

            return group;
        }

        private TaxonomyValue GetTaxonomyValueForLabelInternal(TermStore termStore, string termStoreGroupName, string termSetName, string termLabel)
        {
            Group termStoreGroup = GetGroupFromTermStore(termStore, termStoreGroupName);
            TermSet termSet = this.GetTermSetFromGroup(termStore, termStoreGroup, termSetName);

            return GetTaxonomyValueForLabelInternal(termStore, termStoreGroup, termSet, termLabel);
        }

        private Term GetTermForLabelInternal(TermStore termStore, string termStoreGroupName, string termSetName, string termLabel)
        {
            Group termStoreGroup = GetGroupFromTermStore(termStore, termStoreGroupName);
            TermSet termSet = this.GetTermSetFromGroup(termStore, termStoreGroup, termSetName);

            return GetTermForLabelInternal(termStore, termStoreGroup, termSet, termLabel);
        }

        private IList<TaxonomyValue> GetTaxonomyValuesForLabelInternal(TermStore termStore, string termStoreGroupName, string termSetName, string termLabel)
        {
            Group termStoreGroup = GetGroupFromTermStore(termStore, termStoreGroupName);
            TermSet termSet = this.GetTermSetFromGroup(termStore, termStoreGroup, termSetName);

            return GetTaxonomyValuesForLabelInternal(termStore, termStoreGroup, termSet, termLabel);
        }

        private IList<TaxonomyValue> GetTaxonomyValuesForTermSetInternal(TermStore termStore, string termStoreGroupName, string termSetName)
        {
            Group termStoreGroup = GetGroupFromTermStore(termStore, termStoreGroupName);
            TermSet termSet = this.GetTermSetFromGroup(termStore, termStoreGroup, termSetName);

            return GetTaxonomyValuesForTermSetInternal(termStore, termStoreGroup, termSet);
        }

        private IList<Term> GetTermsForLabelInternal(TermStore termStore, string termStoreGroupName, string termSetName, string termLabel)
        {
            if (termStore == null)
            {
                throw new ArgumentNullException("termStore");
            }

            if (string.IsNullOrEmpty(termStoreGroupName))
            {
                throw new ArgumentNullException("termStoreGroupName");
            }

            if (string.IsNullOrEmpty(termSetName))
            {
                throw new ArgumentNullException("termSetName");
            }

            if (string.IsNullOrEmpty(termLabel))
            {
                throw new ArgumentNullException("termLabel");
            }

            Group group = GetGroupFromTermStore(termStore, termStoreGroupName);
            TermSet termSet = this.GetTermSetFromGroup(termStore, group, termSetName);

            return GetTermsForLabelInternal(termStore, group, termSet, termLabel);
        }

        private IList<Term> GetTermsForTermSetInternal(TermStore termStore, string termStoreGroupName, string termSetName)
        {
            if (termStore == null)
            {
                throw new ArgumentNullException("termStore");
            }

            if (string.IsNullOrEmpty(termStoreGroupName))
            {
                throw new ArgumentNullException("termStoreGroupName");
            }

            if (string.IsNullOrEmpty(termSetName))
            {
                throw new ArgumentNullException("termSetName");
            }

            Group group = GetGroupFromTermStore(termStore, termStoreGroupName);
            TermSet termSet = this.GetTermSetFromGroup(termStore, group, termSetName);

            return GetTermsForTermSetInternal(termStore, group, termSet);
        }

        private IList<SimpleLinkTermInfo> GetTermsAsSimpleLinkNavNodeForTermSetInternal(TermStore termStore, string termStoreGroupName, string termSetName)
        {
            if (termStore == null)
            {
                throw new ArgumentNullException("termStore");
            }

            if (string.IsNullOrEmpty(termStoreGroupName))
            {
                throw new ArgumentNullException("termStoreGroupName");
            }

            if (string.IsNullOrEmpty(termSetName))
            {
                throw new ArgumentNullException("termSetName");
            }

            var group = GetGroupFromTermStore(termStore, termStoreGroupName);
            var termSet = this.GetTermSetFromGroup(termStore, group, termSetName);

            return GetTermsAsSimpleLinkNavNodeForTermSetInternal(termStore, group, termSet);
        }

        private static IList<SimpleLinkTermInfo> GetTermsAsSimpleLinkNavNodeForTermSetInternal(TermStore termStore, Group termStoreGroup, TermSet termSet)
        {
            if (termStore == null)
            {
                throw new ArgumentNullException("termStore");
            }

            if (termStoreGroup == null)
            {
                throw new ArgumentNullException("termStoreGroup");
            }

            if (termSet == null)
            {
                throw new ArgumentNullException("termSet");
            }

            IList<SimpleLinkTermInfo> termsList = new List<SimpleLinkTermInfo>();

            if (termSet.Terms.Any())
            {
                // If custom sort order is set, build term list in sorted order
                var customSortOrder = termSet.CustomSortOrder;
                if (!string.IsNullOrEmpty(customSortOrder))
                {
                    var terms = termSet.Terms.ToList();
                    var sortedIds = customSortOrder.Split(':').Select(id => new Guid(id)).ToList();
                    foreach (var sortedId in sortedIds)
                    {
                        var sortedTerm = terms.SingleOrDefault(term => term.Id.Equals(sortedId));
                        if (sortedTerm != null)
                        {
                            termsList.Add(new SimpleLinkTermInfo(sortedTerm));
                        }
                    }
                }
                else
                {
                    termsList = termSet.Terms.Select(x => new SimpleLinkTermInfo(x)).ToList();
                }
            }

            return termsList;
        }

        #endregion
    }
}
