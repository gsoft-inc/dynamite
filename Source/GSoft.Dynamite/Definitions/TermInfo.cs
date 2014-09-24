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
        /// Default constructor for TermInfo for serialization purposes
        /// </summary>
        public TermInfo()
        {
            this.Labels = new Dictionary<CultureInfo, string>();
        }

        /// <summary>
        /// Constructor for single language (CurrentUICulture) Termnfo belonging to specfic term set
        /// </summary>
        public TermInfo(Guid id, string label, TermSetInfo termSet)
            : this()
        {
            this.Id = id;
            this.Label = label;
            this.TermSet = termSet;
        }
        
        /// <summary>
        /// Constructor for fully translated TermInfo belonging to specfic term set
        /// </summary>
        public TermInfo(Guid id, IDictionary<CultureInfo, string> labels, TermSetInfo termSet)
            : this()
        {
            this.Id = id;
            this.Labels = labels;
            this.TermSet = termSet;
        }

        /// <summary>
        /// GUID of the term
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Default term label in the current MUI language
        /// </summary>
        public string Label
        {
            get
            {
                return this.Labels.ContainsKey(CultureInfo.CurrentUICulture) ?
                    this.Labels[CultureInfo.CurrentUICulture] : string.Empty;
            }

            set
            {
                // set the label for the current UI thread culture
                this.Labels[CultureInfo.CurrentUICulture] = value;
            }
        }

        /// <summary>
        /// Default labels by language (LCID) for the Term
        /// </summary>
        public IDictionary<CultureInfo, string> Labels { get; set; }

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
