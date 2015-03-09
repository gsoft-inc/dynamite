using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Taxonomy;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Represents faceted navigation configuration for a taxonomy term
    /// </summary>
    public class FacetedNavigationInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public FacetedNavigationInfo()
        {
            this.Refiners = new List<RefinerInfo>();
        }

        /// <summary>
        /// Creates a new FacetedNavigationInfo object
        /// </summary>
        /// <param name="term">The term information</param>
        /// <param name="refiners">The refiners</param>
        public FacetedNavigationInfo(TermInfo term, IList<RefinerInfo> refiners)
        {
            this.Term = term;
            this.Refiners = refiners;
        }

        /// <summary>
        /// The taxonomy term
        /// </summary>
        public TermInfo Term { get; set; }

        /// <summary>
        /// The refiners list
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable easier initialization of object.")]
        public IList<RefinerInfo> Refiners { get; set; } 
    }
}
