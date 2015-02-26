using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.ValueTypes
{
    ///// <summary>
    ///// Multiple taxonomy values.
    ///// </summary>
    //public class TaxonomyValueCollection : Collection<TaxonomyValue>
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="TaxonomyValueCollection"/> class.
    //    /// </summary>
    //    public TaxonomyValueCollection()
    //    {
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
    //    /// </summary>
    //    /// <param name="taxonomyValues">The taxonomy value.</param>
    //    public TaxonomyValueCollection(IList<TaxonomyValue> taxonomyValues) :
    //        base(taxonomyValues)
    //    {
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
    //    /// </summary>
    //    /// <param name="termsCollection">The taxonomy values.</param>
    //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "SharePoint is the dirty culprit in exposing Generic Lists, isn't it?")]
    //    public TaxonomyValueCollection(IList<Term> termsCollection) :
    //        this(new TaxonomyValueCollection(termsCollection.Select(term => new TaxonomyValue(term)).ToList()))
    //    {
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
    //    /// </summary>
    //    /// <remarks>This constructor will not ensure that the labels respect the CurrentUICulture</remarks>
    //    /// <param name="taxonomyFieldValueCollection">The taxonomy values.</param>
    //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "SharePoint is the dirty culprit in exposing Generic Lists, isn't it?")]
    //    public TaxonomyValueCollection(TaxonomyFieldValueCollection taxonomyFieldValueCollection) :
    //        this(new TaxonomyValueCollection(taxonomyFieldValueCollection.Select(taxFieldValue => new TaxonomyValue(taxFieldValue)).ToList()))
    //    {
    //    }
    //}
}
