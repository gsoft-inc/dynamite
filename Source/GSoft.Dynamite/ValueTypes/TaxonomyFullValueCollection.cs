using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// Multiple taxonomy values.
    /// </summary>
    public class TaxonomyFullValueCollection : Collection<TaxonomyFullValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyFullValueCollection"/> class.
        /// </summary>
        public TaxonomyFullValueCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyFullValueCollection"/> class.
        /// </summary>
        /// <param name="taxonomyValues">The taxonomy value.</param>
        public TaxonomyFullValueCollection(IList<TaxonomyFullValue> taxonomyValues) :
            base(taxonomyValues)
        {
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="TaxonomyFullValueCollection"/> class.
        ///// </summary>
        ///// <param name="termsCollection">The taxonomy values.</param>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "SharePoint is the dirty culprit in exposing Generic Lists, isn't it?")]
        //public TaxonomyFullValueCollection(IList<Term> termsCollection) :
        //    this(new TaxonomyFullValueCollection(termsCollection.Select(term => new TaxonomyFullValue(term)).ToList()))
        //{
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="TaxonomyFullValueCollection"/> class.
        ///// </summary>
        ///// <remarks>This constructor will not ensure that the labels respect the CurrentUICulture</remarks>
        ///// <param name="taxonomyFieldValueCollection">The taxonomy values.</param>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "SharePoint is the dirty culprit in exposing Generic Lists, isn't it?")]
        //public TaxonomyFullValueCollection(TaxonomyFieldValueCollection taxonomyFieldValueCollection) :
        //    this(new TaxonomyFullValueCollection(taxonomyFieldValueCollection.Select(taxFieldValue => new TaxonomyFullValue(taxFieldValue)).ToList()))
        //{
        //}
    }
}
