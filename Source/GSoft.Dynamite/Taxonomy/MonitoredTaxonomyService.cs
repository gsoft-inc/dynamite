using System;
using System.Collections.Generic;
using System.Linq;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;
using GSoft.Dynamite.Monitoring;

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

        public MonitoredTaxonomyService(ITaxonomyService decorated, IAggregateTimeTracker timeTracker)
        {
            this.decorated = decorated;
            this.timeTracker = timeTracker;
        }

        public TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValueForLabel(site, termStoreName, termStoreGroupName, termSetName, termLabel);
            }
        }

        public TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValueForLabel(site, termStoreGroupName, termSetName, termLabel);
            }
        }

        public TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValueForLabel(site, termSetName, termLabel);
            }
        }

        public Term GetTermForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForLabel(site, termStoreName, termStoreGroupName, termSetName, termLabel);
            }
        }

        public Term GetTermForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForLabel(site, termStoreGroupName, termSetName, termLabel);
            }
        }

        public Term GetTermForLabel(SPSite site, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForLabel(site, termSetName, termLabel);
            }
        }

        public Term GetTermForId(SPSite site, Guid id)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForId(site, id);
            }
        }

        public Term GetTermForId(SPSite site, string termStoreName, Guid id)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForId(site, termStoreName, id);
            }
        }

        public IList<TaxonomyValue> GetTaxonomyValuesForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForLabel(site, termStoreName, termStoreGroupName, termSetName, termLabel);
            }
        }

        public IList<TaxonomyValue> GetTaxonomyValuesForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForLabel(site, termStoreGroupName, termSetName, termLabel);
            }
        }

        public IList<TaxonomyValue> GetTaxonomyValuesForLabel(SPSite site, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForLabel(site, termSetName, termLabel);
            }
        }

        public IList<Term> GetTermsForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForLabel(site, termStoreName, termStoreGroupName, termSetName, termLabel);
            }
        }

        public IList<Term> GetTermsForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForLabel(site, termStoreGroupName, termSetName, termLabel);
            }
        }

        public IList<Term> GetTermsForLabel(SPSite site, string termSetName, string termLabel)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForLabel(site, termSetName, termLabel);
            }
        }

        public IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termStoreName, string termStoreGroupName, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForTermSet(site, termStoreName, termStoreGroupName, termSetName);
            }
        }

        public IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termStoreGroupName, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForTermSet(site, termStoreGroupName, termSetName);
            }
        }

        public IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTaxonomyValuesForTermSet(site, termSetName);
            }
        }

        public IList<Term> GetTermsForTermSet(SPSite site, string termStoreName, string termStoreGroupName, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForTermSet(site, termStoreName, termStoreGroupName, termSetName);
            }
        }

        public IList<Term> GetTermsForTermSet(SPSite site, string termStoreGroupName, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForTermSet(site, termStoreGroupName, termSetName);
            }
        }

        public IList<Term> GetTermsForTermSet(SPSite site, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermsForTermSet(site, termSetName);
            }
        }

        public TermSet GetTermSetFromGroup(TermStore termStore, Group group, string termSetName)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermSetFromGroup(termStore, group, termSetName);
            }
        }

        public Term GetTermForIdInTermSet(SPSite site, string termSetName, Guid id)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForIdInTermSet(site, termSetName, id);
            }
        }

        public Term GetTermForIdInTermSet(SPSite site, string termStoreGroupName, string termSetName, Guid id)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermForIdInTermSet(site, termStoreGroupName, termSetName, id);
            }
        }

        public IList<Term> GetTermPathFromRootToTerm(SPSite site, Guid termSetId, Guid termId, bool parentFirst = false)
        {
            using (var timeTracker = this.timeTracker.BeginTimeTrackerScope(TimeTrackerKey))
            {
                return this.decorated.GetTermPathFromRootToTerm(site, termSetId, termId, parentFirst);
            }
        }
    }
}
