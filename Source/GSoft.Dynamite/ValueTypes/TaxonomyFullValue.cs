using System;
using System.Globalization;
using GSoft.Dynamite.Extensions;
using Microsoft.SharePoint.Taxonomy;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Taxonomy;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// A taxonomy value.
    /// </summary>
    public class TaxonomyFullValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
        /// </summary>
        public TaxonomyFullValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
        /// </summary>
        /// <remarks>This constructor will not ensure the label respect the CurrentUICulture</remarks>
        /// <param name="taxonomyValue">The taxonomy value.</param>
        public TaxonomyFullValue(TaxonomyFieldValue taxonomyValue)
        {
            //Guid termGuid;

            //if (taxonomyValue == null)
            //{
            //    throw new ArgumentNullException("taxonomyValue");
            //}

            //if (!GuidExtension.TryParse(taxonomyValue.TermGuid, out termGuid))
            //{
            //    throw new ArgumentException("Cannot parse the Taxonomy field value's TermGuid.", "taxonomyValue");
            //}

            //this.Id = termGuid;
            //this.Label = taxonomyValue.Label;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
        /// </summary>
        /// <param name="term">The term.</param>
        public TaxonomyFullValue(Term term)
        {
            //if (term == null)
            //{
            //    throw new ArgumentNullException("term");
            //}

            //this.Id = term.Id;

            //// Respect the current user's MUI language selection
            //string currentUiLabel = term.GetDefaultLabel(CultureInfo.CurrentUICulture.LCID);

            //if (!string.IsNullOrEmpty(currentUiLabel))
            //{
            //    this.Label = currentUiLabel;
            //}
            //else if (term.Labels.Count > 0)
            //{
            //    // if no label exists in the current UI language, just fall back on the first of the bunch 
            //    this.Label = term.Labels[0].Value;
            //}
            
            //this.CustomSortPosition = GetCustomSortOrderFromParent(term);
        }

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
