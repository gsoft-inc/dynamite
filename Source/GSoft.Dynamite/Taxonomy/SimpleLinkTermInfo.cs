using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Definition of a taxonomy term used as a simple link navigation node
    /// </summary>
    public class SimpleLinkTermInfo : TermInfo
    {
        private const string LocalCustomPropertyUrl = "_Sys_Nav_SimpleLinkUrl";

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLinkTermInfo"/> class.
        /// </summary>
        public SimpleLinkTermInfo()
            : base()
        {
            this.ChildTerms = new List<SimpleLinkTermInfo>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLinkTermInfo"/> class.
        /// </summary>
        /// <param name="simpleLinkTarget">The simple link URL.</param>
        public SimpleLinkTermInfo(string simpleLinkTarget)
            : base()
        {
            this.SimpleLinkTarget = simpleLinkTarget;
            this.ChildTerms = new List<SimpleLinkTermInfo>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLinkTermInfo"/> class.
        /// </summary>
        /// <param name="id">The term's ID</param>
        /// <param name="label">The term's default label</param>
        /// <param name="termSet">The parent term set</param>
        public SimpleLinkTermInfo(Guid id, string label, TermSetInfo termSet)
            : base(id, label, termSet)
        {
            this.ChildTerms = new List<SimpleLinkTermInfo>();
            this.SimpleLinkTarget = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLinkTermInfo"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="label">The label.</param>
        /// <param name="termSet">The term set.</param>
        /// <param name="simpleLinkTarget">The simple link URL.</param>
        public SimpleLinkTermInfo(Guid id, string label, TermSetInfo termSet, string simpleLinkTarget)
            : this(id, label, termSet)
        {
            this.SimpleLinkTarget = simpleLinkTarget;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLinkTermInfo"/> class.
        /// </summary>
        /// <param name="sharepointTerm">The sharepoint term.</param>
        public SimpleLinkTermInfo(Term sharepointTerm)
            : base(sharepointTerm)
        {
            // Try to get the local custom property holding the simple link URL. If the property is not found, use an empty string.
            if (sharepointTerm.LocalCustomProperties.ContainsKey(LocalCustomPropertyUrl))
            {
                this.SimpleLinkTarget = sharepointTerm.LocalCustomProperties[LocalCustomPropertyUrl];
            }
            else
            {
                this.SimpleLinkTarget = string.Empty;
            }

            // Create the whole terms hierarchy (with child terms)
            if (sharepointTerm.Terms.Count > 0)
            {
                this.ChildTerms = new List<SimpleLinkTermInfo>(sharepointTerm.Terms.Select(x => new SimpleLinkTermInfo(x)));
            }
            else
            {
                this.ChildTerms = new List<SimpleLinkTermInfo>();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the simple link URL.
        /// </summary>
        /// <value>
        /// The simple link URL.
        /// </value>
        public string SimpleLinkTarget { get; set; }

        /// <summary>
        /// Gets or sets the child terms.
        /// </summary>
        /// <value>
        /// The child terms.
        /// </value>
        public IEnumerable<SimpleLinkTermInfo> ChildTerms { get; set; }

        #endregion
    }
}
