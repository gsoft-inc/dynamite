using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GSoft.Dynamite.Extensions;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint.Taxonomy;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyFullValue"/> class.
        /// </summary>
        /// <remarks>This constructor will not ensure the label respect the CurrentUICulture</remarks>
        /// <param name="field">The list field from which the TaxonomyFieldValue was extracted. This is needed to extract the full TaxonomyContext.</param>
        /// <param name="fieldValue">The actual taxonomy field value.</param>
        public TaxonomyFullValue(TaxonomyFieldValue fieldValue)
        {
            Guid termGuid;

            if (fieldValue == null)
            {
                throw new ArgumentNullException("fieldValue");
            }

            if (!Guid.TryParse(fieldValue.TermGuid, out termGuid))
            {
                throw new ArgumentException("Cannot parse the Taxonomy field value's TermGuid.", "taxonomyValue");
            }

            this.Term = new TermInfo(termGuid, fieldValue.Label, null);
            this.Context = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyFullValue"/> class.
        /// </summary>
        /// <param name="term">The term.</param>
        public TaxonomyFullValue(Term term)
        {
            if (term == null)
            {
                throw new ArgumentNullException("term");
            }

            this.Term = new TermInfo(term);
            this.Context = new TaxonomyContext(new TermSetInfo(term.TermSet));
        }

        /// <summary>
        /// Gets or sets the Term definition
        /// </summary>
        public TermInfo Term { get; set; }

        /// <summary>
        /// Gets or sets the Term's parent context objects.
        /// </summary>
        public TaxonomyContext Context { get; set; }

        /// <summary>
        /// Gets the Term's unique Id
        /// </summary>
        public Guid Id 
        { 
            get
            {
                return this.Term.Id;
            }
        }

        /// <summary>
        /// Gets the Term's label in the current UI culture
        /// </summary>
        public string Label
        {
            get
            {
                return this.Term.Label;
            }
        }
    }
}
