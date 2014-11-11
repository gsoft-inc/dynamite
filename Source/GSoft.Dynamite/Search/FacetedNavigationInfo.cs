using System.Collections.Generic;
using GSoft.Dynamite.Taxonomy;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Represents faceted navigation configuration for a taxonomy term
    /// </summary>
    public class FacetedNavigationInfo
    {
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
        public TermInfo Term { get; private set; }

        /// <summary>
        /// The refiners list
        /// </summary>
        public IList<RefinerInfo> Refiners { get; private set; } 
    }
}
