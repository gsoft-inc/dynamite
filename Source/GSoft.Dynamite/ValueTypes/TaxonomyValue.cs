using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using GSoft.Dynamite.Extensions;
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

            if (!GuidExtension.TryParse(taxonomyValue.TermGuid, out termGuid))
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

            // Respect the current user's MUI language selection
            this.Label = term.GetDefaultLabel(CultureInfo.CurrentUICulture.LCID);
            
            this.CustomSortPosition = GetCustomSortOrderFromParent(term);
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the custom sort position.
        /// </summary>
        public int CustomSortPosition { get; set; }

        private static int GetCustomSortOrderFromParent(Term term)
        {
            int sortPosition = 0;
            string parentCustomSortOrder = string.Empty;

            if (term.Parent != null)
            {
                // Parent term holds the custom sort order
                parentCustomSortOrder = term.Parent.CustomSortOrder;
            }
            else
            {
                // At root of term set the TermSet object holds the wacky ordering string
                parentCustomSortOrder = term.TermSet.CustomSortOrder;
            }

            if (!string.IsNullOrEmpty(parentCustomSortOrder))
            {
                // Format is {GUID}:{GUID}:{GUID} and so on for all child terms
                string[] split = parentCustomSortOrder.Split(':');

                var currentPosition = 0;
                foreach (string guid in split)
                {
                    currentPosition++;

                    if (new Guid(guid) == term.Id)
                    {
                        sortPosition = currentPosition;
                        break;
                    }
                }
            }

            return sortPosition;
        }
    }
}
