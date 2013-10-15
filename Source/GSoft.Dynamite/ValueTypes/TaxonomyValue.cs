using System;
using System.Globalization;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// A taxonomy value.
    /// </summary>
    public class TaxonomyValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
        /// </summary>
        public TaxonomyValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
        /// </summary>
        /// <param name="taxonomyValue">The taxonomy value.</param>
        public TaxonomyValue(TaxonomyFieldValue taxonomyValue)
        {
            Guid termGuid;

            if (taxonomyValue == null)
            {
                throw new ArgumentNullException("taxonomyValue");
            }

            if (!Guid.TryParse(taxonomyValue.TermGuid, out termGuid))
            {
                throw new ArgumentException("Cannot parse the Taxonomy field value's TermGuid.", "taxonomyValue");
            }

            this.Id = termGuid;
            this.Label = taxonomyValue.Label;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
        /// </summary>
        /// <param name="term">The term.</param>
        public TaxonomyValue(Term term)
        {
            if (term == null)
            {
                throw new ArgumentNullException("term");
            }

            this.Id = term.Id;
            this.Label = term.GetDefaultLabel(CultureInfo.CurrentUICulture.LCID);
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public string Label { get; set; }
    }
}
