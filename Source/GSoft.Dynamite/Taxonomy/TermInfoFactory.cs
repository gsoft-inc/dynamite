using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Creates easily serializable <see cref="TermInfo"/> objects from the typical SharePoint taxonomy term representations
    /// </summary>
    public class TermInfoFactory
    {
        private ITaxonomyService taxonomyService;
        private ILogger logger;

        public TermInfoFactory(ITaxonomyService taxonomyService, ILogger logger)
        {
            this.taxonomyService = taxonomyService;
            this.logger = logger;
        }

        /// <summary>
        /// Initializes a <see cref="TermInfo"/> instance from a taxonomy term instance.
        /// </summary>
        /// <param name="term">The taxonomy term</param>
        /// <returns>The easily serializable <see cref="TermInfo"/> object</returns>
        public TermInfo CreateFromTerm(Term term)
        {
            if (term == null)
            {
                throw new ArgumentNullException("term");
            }

            var termSetInfo = this.CreateFromTermSet(term.TermSet);

            return new TermInfo(term.Id, this.GetTermDefaultLabelsForWorkingLanguages(term), termSetInfo);
        }

        /// <summary>
        /// Initializes a <see cref="TermInfo"/> instance from a taxonomy field value and definition.
        /// </summary>
        /// <param name="field">The list field from which the TaxonomyFieldValue was extracted. This is needed to extract the full TaxonomyContext.</param>
        /// <param name="fieldValue">The actual taxonomy field value.</param>
        /// <returns>The easily serializable <see cref="TermInfo"/> object</returns>
        public TermInfo CreateFromTaxonomyFieldValue(TaxonomyField field, TaxonomyFieldValue fieldValue)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            if (fieldValue == null)
            {
                throw new ArgumentNullException("fieldValue");
            }

            var termInfo = new TermInfo();

            return termInfo;
        }

        /// <summary>
        /// Initializes a <see cref="TermSetInfo"/> instance from a taxonomy term set.
        /// </summary>
        /// <param name="termSet">The term set</param>
        /// <returns>The easily serializable <see cref="TermSetInfo"/> object</returns>
        public TermSetInfo CreateFromTermSet(TermSet termSet)
        {
            if (termSet == null)
            {
                throw new ArgumentNullException("termSet");
            }

            TermGroupInfo groupInfo = this.CreateFromTermGroup(termSet.Group);

            return new TermSetInfo(termSet.Id, this.GetTermSetLabelsForAllWorkingLanguages(termSet), groupInfo);
        }

        /// <summary>
        /// Initializes a <see cref="TermGroupInfo"/> instance from a taxonomy term set group.
        /// </summary>
        /// <param name="termSetGroup">The taxonomy group</param>
        /// <returns>The easily serializable <see cref="TermGroupInfo"/> object</returns>
        public TermGroupInfo CreateFromTermGroup(Group termSetGroup)
        {
            if (termSetGroup == null)
            {
                throw new ArgumentNullException("termSetGroup");
            }

            TermStoreInfo termStoreInfo = this.CreateFromTermStore(termSetGroup.TermStore);

            return new TermGroupInfo(termSetGroup.Id, termSetGroup.Name, termStoreInfo)
                {
                    IsSiteCollectionSpecificTermGroup = termSetGroup.IsSiteCollectionGroup
                };
        }

        /// <summary>
        /// Initializes a <see cref="TermStoreInfo"/> instance from a taxonomy managed metadata service
        /// </summary>
        /// <param name="store">The term store</param>
        /// <returns>The easily serializable <see cref="TermStoreInfo"/> object</returns>
        public TermStoreInfo CreateFromTermStore(TermStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            return new TermStoreInfo(store.Id, store.Name);
        }

        private Dictionary<CultureInfo, string> GetTermSetLabelsForAllWorkingLanguages(TermSet termSet)
        {
            var dictionary = new Dictionary<CultureInfo, string>();

            var termStore = termSet.Group.TermStore;
            var cultures = termStore.Languages.Select(lang => new CultureInfo(lang));

            foreach (CultureInfo culture in cultures)
            {
                // Store the original working language
                int originalWorkingLanguage = termStore.WorkingLanguage;

                // Switch working language to the desired culture
                termStore.WorkingLanguage = culture.LCID;

                var labelForCulture = termSet.Name;     // accessor should respect the current working language

                if (!string.IsNullOrEmpty(labelForCulture))
                {
                    dictionary[culture] = labelForCulture;
                }

                // Reset working language to its original value
                termStore.WorkingLanguage = originalWorkingLanguage;
            }

            return dictionary;
        }

        private Dictionary<CultureInfo, string> GetTermDefaultLabelsForWorkingLanguages(Term term)
        {
            var dictionary = new Dictionary<CultureInfo, string>();

            var cultures = term.TermSet.Group.TermStore.Languages.Select(lang => new CultureInfo(lang));

            foreach (CultureInfo culture in cultures)
            {
                var labelForCulture = term.GetDefaultLabel(culture.LCID);

                if (!string.IsNullOrEmpty(labelForCulture))
                {
                    dictionary[culture] = labelForCulture; 
                }
            }

            return dictionary;
        }

    }
}
