using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
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
        /// Constructor for single language (CurrentUICulture) TermInfo belonging to specific term set
        /// </summary>
        /// <param name="id">The term's ID</param>
        /// <param name="label">The term's default label</param>
        /// <param name="termSet">The parent term set</param>
        public TermInfo(Guid id, string label, TermSetInfo termSet)
            : this()
        {
            this.Id = id;
            this.Label = label;
            this.TermSet = termSet;
        }
        
        /// <summary>
        /// Constructor for fully translated TermInfo belonging to specific term set
        /// </summary>
        /// <param name="id">The term's ID</param>
        /// <param name="labels">All default labels</param>
        /// <param name="termSet">The parent term set</param>
        public TermInfo(Guid id, IDictionary<CultureInfo, string> labels, TermSetInfo termSet)
            : this()
        {
            this.Id = id;
            this.Labels = labels;
            this.TermSet = termSet;
        }

        /// <summary>
        /// Convenience constructor to create TermInfo objects out of SharePoint's Term
        /// instances
        /// </summary>
        /// <param name="sharePointTerm">The SharePoint taxonomy term</param>
        public TermInfo(Term sharePointTerm)
        {
            IDictionary<CultureInfo, string> labels = new Dictionary<CultureInfo, string>();
            sharePointTerm.Labels.Cast<Label>().ToList().ForEach(l => labels.Add(new CultureInfo(l.Language), l.Value));

            this.Id = sharePointTerm.Id;
            this.Labels = labels;
            this.TermSet = new TermSetInfo(sharePointTerm.TermSet);
            this.CustomSortPosition = GetCustomSortOrderFromParent(sharePointTerm);
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
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable easier initialization of object.")]
        public IDictionary<CultureInfo, string> Labels { get; set; }

        /// <summary>
        /// Parent Term Set definition
        /// </summary>
        public TermSetInfo TermSet { get; set; }

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
