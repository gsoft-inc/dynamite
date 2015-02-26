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
    public class TaxonomyValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
        /// </summary>
        public TaxonomyValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TaxonomyValue"/> with a 
        /// default TaxonomyContext determined by the parent term set of the TermInfo
        /// </summary>
        /// <param name="termInfo">The term metadata corresponding to the taxonomy value</param>
        public TaxonomyValue(TermInfo termInfo)
        {
            this.Term = termInfo;
            this.Context = new TaxonomyContext(termInfo.TermSet);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
        /// </summary>
        /// <remarks>This constructor will not ensure the label respect the CurrentUICulture</remarks>
        /// <param name="fieldValue">The actual taxonomy field value.</param>
        public TaxonomyValue(TaxonomyFieldValue fieldValue)
        {
            Guid termGuid;

            if (fieldValue == null)
            {
                throw new ArgumentNullException("fieldValue");
            }

            if (!Guid.TryParse(fieldValue.TermGuid, out termGuid))
            {
                throw new ArgumentException("Cannot parse the Taxonomy field value's TermGuid.", "fieldValue");
            }

            this.Term = new TermInfo(termGuid, fieldValue.Label, null);
            this.Context = null;
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
