using System;
using System.Globalization;
using GSoft.Dynamite.Extensions;
using Microsoft.SharePoint.Taxonomy;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// A taxonomy value.
    /// </summary>
    public class TaxonomyFullValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyFullValue"/> class.
        /// </summary>
        public TaxonomyFullValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TaxonomyFullValue"/> with a 
        /// default TaxonomyContext determined by the parent term set of the TermInfo
        /// </summary>
        /// <param name="termInfo">The term metadata corresponding to the taxonomy value</param>
        public TaxonomyFullValue(TermInfo termInfo)
        {
            this.Term = termInfo;
            this.Context = new TaxonomyContext(termInfo.TermSet);
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="TaxonomyFullValue"/> class.
        ///// </summary>
        ///// <remarks>This constructor will not ensure the label respect the CurrentUICulture</remarks>
        ///// <param name="field">The list field from which the TaxonomyFieldValue was extracted. This is needed to extract the full TaxonomyContext.</param>
        ///// <param name="fieldValue">The actual taxonomy field value.</param>
        //public TaxonomyFullValue(TaxonomyField field, TaxonomyFieldValue fieldValue)
        //{
            
        //    field.

        //    //Guid termGuid;

        //    //if (taxonomyValue == null)
        //    //{
        //    //    throw new ArgumentNullException("taxonomyValue");
        //    //}

        //    //if (!GuidExtension.TryParse(taxonomyValue.TermGuid, out termGuid))
        //    //{
        //    //    throw new ArgumentException("Cannot parse the Taxonomy field value's TermGuid.", "taxonomyValue");
        //    //}

        //    //this.Id = termGuid;
        //    //this.Label = taxonomyValue.Label;
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="TaxonomyFullValue"/> class.
        ///// </summary>
        ///// <param name="term">The term.</param>
        //public TaxonomyFullValue(Term term)
        //{
        //    throw new NotImplementedException();

        //    //if (term == null)
        //    //{
        //    //    throw new ArgumentNullException("term");
        //    //}

        //    //this.Id = term.Id;

        //    //// Respect the current user's MUI language selection
        //    //string currentUiLabel = term.GetDefaultLabel(CultureInfo.CurrentUICulture.LCID);

        //    //if (!string.IsNullOrEmpty(currentUiLabel))
        //    //{
        //    //    this.Label = currentUiLabel;
        //    //}
        //    //else if (term.Labels.Count > 0)
        //    //{
        //    //    // if no label exists in the current UI language, just fall back on the first of the bunch 
        //    //    this.Label = term.Labels[0].Value;
        //    //}
            
        //    //this.CustomSortPosition = GetCustomSortOrderFromParent(term);
        //}

        /// <summary>
        /// Gets or sets the Term definition
        /// </summary>
        public TermInfo Term { get; set; }

        /// <summary>
        /// Gets or sets the Term's parent context objects.
        /// </summary>
        public TaxonomyContext Context { get; set; }
    }
}
