using System;
using System.Collections.Generic;
using System.Linq;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Monitoring;
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
    public class MonitoredTaxonomyService : ITaxonomyService
    {
        private const string TimeTrackerKey = "TaxonomyService";

        private ITaxonomyService decorated;
        private IAggregateTimeTracker timeTracker;

        /// <summary>
        /// Monitored taxonomy service implementation
        /// </summary>
        /// <param name="decorated">The decorated object</param>
        /// <param name="timeTracker">The time tracking object</param>
        public MonitoredTaxonomyService(ITaxonomyService decorated, IAggregateTimeTracker timeTracker)
        {
            this.decorated = decorated;
            this.timeTracker = timeTracker;
        }

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
        public TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValueForLabel(site, termStoreName, termStoreGroupName, termSetName, termLabel);
            }
        }

        /// <summary>
        /// Retrieves a TaxonomyValue corresponding to a term label within the default term store
        /// </summary>
        /// <remarks>If many terms are found with the corresponding label, a root term is returned if found.</remarks>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The taxonomy value or null if not found</returns>
        public TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValueForLabel(site, termStoreGroupName, termSetName, termLabel);
            }
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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValueForLabel(site, termSetName, termLabel);
            }
        }

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
        public Term GetTermForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForLabel(site, termStoreName, termStoreGroupName, termSetName, termLabel);
            }
        }

        /// <summary>
        /// Retrieves a Term corresponding to a term label within the default term store
        /// </summary>
        /// <remarks>If many terms are found with the corresponding label, a root term is returned if found.</remarks>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <param name="termLabel">The default label of the term</param>
        /// <returns>The term or null if not found</returns>
        public Term GetTermForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForLabel(site, termStoreGroupName, termSetName, termLabel);
            }
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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForLabel(site, termSetName, termLabel);
            }
        }

        /// <summary>
        /// Gets the term for identifier.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>The term</returns>
        public Term GetTermForId(SPSite site, Guid id)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForId(site, id);
            }
        }

        /// <summary>
        /// Gets the term for identifier.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="termStoreName">Name of the term store.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>The specific term</returns>
        public Term GetTermForId(SPSite site, string termStoreName, Guid id)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForId(site, termStoreName, id);
            }
        }

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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForLabel(site, termStoreName, termStoreGroupName, termSetName, termLabel);
            }
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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForLabel(site, termStoreGroupName, termSetName, termLabel);
            }
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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForLabel(site, termSetName, termLabel);
            }
        }

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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForLabel(site, termStoreName, termStoreGroupName, termSetName, termLabel);
            }
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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForLabel(site, termStoreGroupName, termSetName, termLabel);
            }
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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForLabel(site, termSetName, termLabel);
            }
        }

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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForTermSet(site, termStoreName, termStoreGroupName, termSetName);
            }
        }

        /// <summary>
        /// Retrieves all TaxonomyValues corresponding to a term set in the default term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        public IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termStoreGroupName, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForTermSet(site, termStoreGroupName, termSetName);
            }
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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForTermSet(site, termSetName);
            }
        }

        /// <summary>
        /// Retrieves all Terms corresponding to a term store term set
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreName">The term store name</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        public IList<Term> GetTermsForTermSet(SPSite site, string termStoreName, string termStoreGroupName, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForTermSet(site, termStoreName, termStoreGroupName, termSetName);
            }
        }

        /// <summary>
        /// Retrieves all Terms corresponding to a term set in the default term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termStoreGroupName">The group name</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        public IList<Term> GetTermsForTermSet(SPSite site, string termStoreGroupName, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForTermSet(site, termStoreGroupName, termSetName);
            }
        }

        /// <summary>
        /// Retrieves all Terms corresponding to a term set in the default term store
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="termSetName">The term set name</param>
        /// <returns>A list of taxonomy values</returns>
        public IList<Term> GetTermsForTermSet(SPSite site, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForTermSet(site, termSetName);
            }
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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsAsSimpleLinkNavNodeForTermSet(site, termStoreName, termStoreGroupName, termSetName);
            }
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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsAsSimpleLinkNavNodeForTermSet(site, termStoreGroupName, termSetName);
            }
        }

        /// <summary>
        /// Gets the term set from group.
        /// </summary>
        /// <param name="termStore">The term store.</param>
        /// <param name="group">The group.</param>
        /// <param name="termSetName">Name of the term set.</param>
        /// <returns>The term set for the specified store, group and term set name.</returns>
        public TermSet GetTermSetFromGroup(TermStore termStore, Group group, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermSetFromGroup(termStore, group, termSetName);
            }
        }

        /// <summary>
        /// Gets the term for identifier within site collection specific term store group.
        /// </summary>
        /// <param name="site">The Site.</param>
        /// <param name="termSetName">The name of the term set containing the term</param>
        /// <param name="id">The GUID of the term to get.</param>
        /// <returns>The term found</returns>
        public Term GetTermForIdInTermSet(SPSite site, string termSetName, Guid id)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForIdInTermSet(site, termSetName, id);
            }
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
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForIdInTermSet(site, termStoreGroupName, termSetName, id);
            }
        }

        /// <summary>
        /// Get all parent terms from source term to root term in the term set
        /// </summary>
        /// <param name="site">The current site collection.</param>
        /// <param name="termStoreId">The parent term store</param>
        /// <param name="termSetId">The term set id.</param>
        /// <param name="termId">The term.</param>
        /// <param name="parentFirst">if set to <c>true</c>, includes the [parent first].</param>
        /// <returns>
        /// List of terms.
        /// </returns>
        public IList<Term> GetTermPathFromRootToTerm(SPSite site, Guid termStoreId, Guid termSetId, Guid termId, bool parentFirst)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermPathFromRootToTerm(site, termStoreId, termSetId, termId, parentFirst);
            }
        }

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
        public IList<Term> GetTermPathFromRootToTerm(SPSite site, Guid termSetId, Guid termId, bool parentFirst)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermPathFromRootToTerm(site, termSetId, termId, parentFirst);
            }
        }

        /// <summary>
        /// Gets the term set group from the term store.
        /// </summary>
        /// <param name="termStore">The term store.</param>
        /// <param name="groupName">The term set group name, in the term store's default working language.</param>
        /// <returns>The term set group</returns>
        public Group GetTermGroupFromStore(TermStore termStore, string groupName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermGroupFromStore(termStore, groupName);
            }
        }
    }
}
