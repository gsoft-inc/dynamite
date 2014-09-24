using System;
using System.Collections.Generic;
using System.Globalization;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a Taxonomy Term 
    /// </summary>
    public class TermInfo
    {
        /// <summary>
        /// Default constructor for TermInfo
        /// </summary>
        public TermInfo()
        {           
        }

        /// <summary>
        /// GUID of the term
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Default term label in the current MUI language
        /// </summary>
        public string Label
        {
            get
            {
                return this.DefaultLabels.ContainsKey(CultureInfo.CurrentUICulture) ?
                    this.DefaultLabels[CultureInfo.CurrentUICulture] : string.Empty;
            }
        }

        /// <summary>
        /// Default labels by language (LCID) for the Term
        /// </summary>
        public IDictionary<CultureInfo, string> DefaultLabels { get; set; }

        /// <summary>
        /// Parent Term Set definition
        /// </summary>
        public TermSetInfo TermSet { get; set; }

        /// <summary>
        /// Gets or sets the custom sort position.
        /// </summary>
        public int CustomSortPosition { get; set; }
    }
}
