using System;
using System.Collections.Generic;
using System.Linq;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
{
    using GSoft.Dynamite.Collections;

    /// <summary>
    /// Helper class for interacting with the Managed Metadata Service
    /// </summary>
    /// <remarks>
    /// For all methods: if a term or a term set is not found by its default label 
    /// in the term store's default working language, the other alternate available 
    /// languages should be attempted.
    /// </remarks>
    public class CachedTaxonomyService : ITaxonomyService
    {
        private readonly ITaxonomyService decorated;

        private readonly Dictionary<Guid, Term> termDictionaryByGuid = new Dictionary<Guid, Term>();

        private readonly Dictionary<string, IList<Term>> termsDictionaryBytermSetNameTermLabel = new Dictionary<string, IList<Term>>();

        private readonly Dictionary<string, Term> termDictionaryBytermSetNameTermLabel = new Dictionary<string, Term>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedTaxonomyService"/> class.
        /// </summary>
        /// <param name="decorated">The taxonomy service to decorate.</param>
        public CachedTaxonomyService(ITaxonomyService decorated)
        {
            this.decorated = decorated;
        }

        /// <summary>
        /// The get taxonomy value for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreName">
        /// The term store name.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="TaxonomyValue"/>.
        /// </returns>
        public TaxonomyValue GetTaxonomyValueForLabel(
            SPSite site,
            string termStoreName,
            string termStoreGroupName,
            string termSetName,
            string termLabel)
        {
            return this.decorated.GetTaxonomyValueForLabel(site, termStoreName, termStoreGroupName, termSetName, termLabel);
        }

        /// <summary>
        /// The get taxonomy value for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="TaxonomyValue"/>.
        /// </returns>
        public TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            return this.decorated.GetTaxonomyValueForLabel(site, termStoreGroupName, termSetName, termLabel);
        }

        /// <summary>
        /// The get taxonomy value for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="TaxonomyValue"/>.
        /// </returns>
        public TaxonomyValue GetTaxonomyValueForLabel(SPSite site, string termSetName, string termLabel)
        {
            return this.decorated.GetTaxonomyValueForLabel(site, termSetName, termLabel);
        }

        /// <summary>
        /// The get term for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreName">
        /// The term store name.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="Term"/>.
        /// </returns>
        public Term GetTermForLabel(SPSite site, string termStoreName, string termStoreGroupName, string termSetName, string termLabel)
        {
            Term term;
            var key = string.Format("{0}|{1}|{2}|{3}", termStoreName, termStoreGroupName, termSetName, termLabel);
            if (this.termDictionaryBytermSetNameTermLabel.TryGetValue(key, out term))
            {
                return term;
            }

            term = this.decorated.GetTermForLabel(site, termStoreName, termStoreGroupName, termSetName, termLabel);
            this.termDictionaryBytermSetNameTermLabel.Add(key, term);

            return term;
        }

        /// <summary>
        /// The get term for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="Term"/>.
        /// </returns>
        public Term GetTermForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            Term term;
            var key = string.Format("{0}|{1}|{2}", termStoreGroupName, termSetName, termLabel);
            if (this.termDictionaryBytermSetNameTermLabel.TryGetValue(key, out term))
            {
                return term;
            }

            term = this.decorated.GetTermForLabel(site, termStoreGroupName, termSetName, termLabel);
            this.termDictionaryBytermSetNameTermLabel.Add(key, term);

            return term;
        }

        /// <summary>
        /// The get term for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="Term"/>.
        /// </returns>
        public Term GetTermForLabel(SPSite site, string termSetName, string termLabel)
        {
            Term term;
            var key = string.Format("{0}|{1}", termSetName, termLabel);
            if (this.termDictionaryBytermSetNameTermLabel.TryGetValue(key, out term))
            {
                return term;
            }

            term = this.decorated.GetTermForLabel(site, termSetName, termLabel);
            this.termDictionaryBytermSetNameTermLabel.Add(key, term);

            return term;
        }

        /// <summary>
        /// The get term for id.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Term"/>.
        /// </returns>
        public Term GetTermForId(SPSite site, Guid id)
        {
            Term term;
            if (this.termDictionaryByGuid.TryGetValue(id, out term))
            {
                return term;
            }

            term = this.decorated.GetTermForId(site, id);
            this.termDictionaryByGuid.Add(id, term);

            return term;
        }

        /// <summary>
        /// The get taxonomy values for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreName">
        /// The term store name.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<TaxonomyValue> GetTaxonomyValuesForLabel(
            SPSite site,
            string termStoreName,
            string termStoreGroupName,
            string termSetName,
            string termLabel)
        {
            return this.decorated.GetTaxonomyValuesForLabel(
                site,
                termStoreName,
                termStoreGroupName,
                termSetName,
                termLabel);
        }

        /// <summary>
        /// The get taxonomy values for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<TaxonomyValue> GetTaxonomyValuesForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            return this.decorated.GetTaxonomyValuesForLabel(site, termStoreGroupName, termSetName, termLabel);
        }

        /// <summary>
        /// The get taxonomy values for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<TaxonomyValue> GetTaxonomyValuesForLabel(SPSite site, string termSetName, string termLabel)
        {
            return this.decorated.GetTaxonomyValuesForLabel(site, termSetName, termLabel);
        }

        /// <summary>
        /// The get terms for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreName">
        /// The term store name.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<Term> GetTermsForLabel(
            SPSite site,
            string termStoreName,
            string termStoreGroupName,
            string termSetName,
            string termLabel)
        {
            IList<Term> terms;
            var key = string.Format("{0}|{1}|{2}|{3}", termStoreName, termStoreGroupName, termSetName, termLabel);
            if (this.termsDictionaryBytermSetNameTermLabel.TryGetValue(key, out terms))
            {
                return terms;
            }

            terms = this.decorated.GetTermsForLabel(site, termStoreName, termStoreGroupName, termSetName, termLabel);
            this.termsDictionaryBytermSetNameTermLabel.Add(key, terms);

            return terms;
        }

        /// <summary>
        /// The get terms for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<Term> GetTermsForLabel(SPSite site, string termStoreGroupName, string termSetName, string termLabel)
        {
            IList<Term> terms;
            var key = string.Format("{0}|{1}|{2}", termStoreGroupName, termSetName, termLabel);
            if (this.termsDictionaryBytermSetNameTermLabel.TryGetValue(key, out terms))
            {
                return terms;
            }

            terms = this.decorated.GetTermsForLabel(site, termStoreGroupName, termSetName, termLabel);
            this.termsDictionaryBytermSetNameTermLabel.Add(key, terms);

            return terms;
        }

        /// <summary>
        /// The get terms for label.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <param name="termLabel">
        /// The term label.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<Term> GetTermsForLabel(SPSite site, string termSetName, string termLabel)
        {
            IList<Term> terms;
            var key = string.Format("{0}|{1}", termSetName, termLabel);
            if (this.termsDictionaryBytermSetNameTermLabel.TryGetValue(key, out terms))
            {
                return terms;
            }

            terms = this.decorated.GetTermsForLabel(site, termSetName, termLabel);
            this.termsDictionaryBytermSetNameTermLabel.Add(key, terms);

            return terms;
        }

        /// <summary>
        /// The get taxonomy values for term set.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreName">
        /// The term store name.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termStoreName, string termStoreGroupName, string termSetName)
        {
            return this.decorated.GetTaxonomyValuesForTermSet(site, termStoreName, termStoreGroupName, termSetName);
        }

        /// <summary>
        /// The get taxonomy values for term set.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termStoreGroupName, string termSetName)
        {
            return this.decorated.GetTaxonomyValuesForTermSet(site, termStoreGroupName, termSetName);
        }

        /// <summary>
        /// The get taxonomy values for term set.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<TaxonomyValue> GetTaxonomyValuesForTermSet(SPSite site, string termSetName)
        {
            return this.decorated.GetTaxonomyValuesForTermSet(site, termSetName);
        }

        /// <summary>
        /// The get terms for term set.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreName">
        /// The term store name.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<Term> GetTermsForTermSet(SPSite site, string termStoreName, string termStoreGroupName, string termSetName)
        {
            return this.decorated.GetTermsForTermSet(site, termStoreName, termStoreGroupName, termSetName);
        }

        /// <summary>
        /// The get terms for term set.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreGroupName">
        /// The term store group name.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<Term> GetTermsForTermSet(SPSite site, string termStoreGroupName, string termSetName)
        {
            return this.decorated.GetTermsForTermSet(site, termStoreGroupName, termSetName);
        }

        /// <summary>
        /// The get terms for term set.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termSetName">
        /// The term set name.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<Term> GetTermsForTermSet(SPSite site, string termSetName)
        {
            return this.decorated.GetTermsForTermSet(site, termSetName);
        }
    }
}
