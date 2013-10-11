using System.Collections.Generic;
using System.Linq;
using GSoft.Dynamite.Sharepoint.ValueTypes;

namespace GSoft.Dynamite.Sharepoint.Taxonomy
{
    /// <summary>
    /// Helper class for interacting with the Managed Metadata Service
    /// </summary>
    public class TaxonomyService : ITaxonomyService
    {       
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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.TermStores[termStoreName];

            return GetTaxonomyValue(termStore, termStoreGroupName, termSetName, termLabel);
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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTaxonomyValue(termStore, termStoreGroupName, termSetName, termLabel);
        }

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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.TermStores[termStoreName];

            return GetTerm(termStore, termStoreGroupName, termSetName, termLabel);
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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTerm(termStore, termStoreGroupName, termSetName, termLabel);
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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.TermStores[termStoreName];
            
            return GetTaxonomyValues(termStore, termStoreGroupName, termSetName, termLabel);
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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.TermStores[termStoreName];    

            return GetTaxonomyValues(termStore, termStoreGroupName, termSetName);
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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTaxonomyValues(termStore, termStoreGroupName, termSetName);
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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTaxonomyValues(termStore, termStoreGroupName, termSetName, termLabel);
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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.TermStores[termStoreName];

            return GetTerms(termStore, termStoreGroupName, termSetName, termLabel);
        }

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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.TermStores[termStoreName];

            return GetTerms(termStore, termStoreGroupName, termSetName);
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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTerms(termStore, termStoreGroupName, termSetName);
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
            TaxonomySession session = new TaxonomySession(site);
            TermStore termStore = session.DefaultSiteCollectionTermStore;

            return GetTerms(termStore, termStoreGroupName, termSetName, termLabel);
        }

        private static TaxonomyValue GetTaxonomyValue(TermStore termStore, string termStoreGroupName, string termSetName, string termLabel)
        {
            Term term = GetTerm(termStore, termStoreGroupName, termSetName, termLabel);

            TaxonomyValue value = null;
            if (term != null)
            {
                value = new TaxonomyValue(term);
            }

            return value;
        }

        private static Term GetTerm(TermStore termStore, string termStoreGroupName, string termSetName, string termLabel)
        {
            Term term = null;
            IList<Term> terms = GetTerms(termStore, termStoreGroupName, termSetName, termLabel);

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

        private static IList<TaxonomyValue> GetTaxonomyValues(TermStore termStore, string termStoreGroupName, string termSetName, string termLabel)
        {
            IList<Term> terms = GetTerms(termStore, termStoreGroupName, termSetName, termLabel);
            return terms.Select(term => new TaxonomyValue(term)).ToList();
        }
        
        private static IList<TaxonomyValue> GetTaxonomyValues(TermStore termStore, string termStoreGroupName, string termSetName)
        {
            IList<TaxonomyValue> termsList = new List<TaxonomyValue>();
            IList<Term> terms = GetTerms(termStore, termStoreGroupName, termSetName);

            if (terms != null && terms.Count > 0)
            {
                termsList = terms.Select(term => new TaxonomyValue(term)).ToList();
            }

            return termsList;
        }

        private static IList<Term> GetTerms(TermStore termStore, string termStoreGroupName, string termSetName, string termLabel)
        {
            // Always interact with the term sets in the term store's default language
            int originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = termStore.DefaultLanguage;

            Group group = termStore.Groups[termStoreGroupName];
            TermSet termSet = group.TermSets[termSetName];

            termStore.WorkingLanguage = originalWorkingLanguage;

            // Attempt to find the terms assuming the label is in the term store default language
            TermCollection termCollection = termSet.GetTerms(termLabel, termStore.DefaultLanguage, true);

            if (termCollection == null || termCollection.Count == 0)
            {
                // Failed to resolve some terms, look among the other term store languages
                foreach (int lcid in termStore.Languages)
                {
                    if (lcid != termStore.DefaultLanguage)
                    {
                        termCollection = termSet.GetTerms(termLabel, lcid, true);

                        if (termCollection != null && termCollection.Count != 0)
                        {
                            break;
                        }
                    }
                }
            }

            return termCollection.Cast<Term>().ToList();
        }

        private static IList<Term> GetTerms(TermStore termStore, string termStoreGroupName, string termSetName)
        {
            IList<Term> termsList = new List<Term>();

            // Always interact with the term sets in the term store's default language
            int originalWorkingLanguage = termStore.WorkingLanguage;
            termStore.WorkingLanguage = termStore.DefaultLanguage;

            Group group = termStore.Groups[termStoreGroupName];
            TermSet termSet = group.TermSets[termSetName];
            
            termStore.WorkingLanguage = originalWorkingLanguage;

            if (termSet.Terms.Count() > 0)
            {
                termsList = termSet.Terms.Cast<Term>().ToList();
            }

            return termsList;
        }
    }
}
