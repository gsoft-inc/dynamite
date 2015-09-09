using System;
using System.Runtime.Serialization;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Navigation query parameters.
    /// </summary>
    public class NavigationQueryParameters
    {
        /// <summary>
        /// ID of the term store where we'll find your restricted term set.
        /// If left empty, we'll assume you're using the DefaultSiteCollectionTermStore.
        /// </summary>
        public Guid TermStoreId { get; set; }

        /// <summary>
        /// Gets or sets the restricted term set identifier.
        /// </summary>
        /// <value>
        /// The restricted term set identifier.
        /// </value>
        public Guid RestrictedTermSetId { get; set; }

        /// <summary>
        /// Gets the search settings.
        /// </summary>
        /// <value>
        /// The search settings.
        /// </value>
        public NavigationSearchSettings SearchSettings { get; set; }

        /// <summary>
        /// Gets the node matching settings.
        /// </summary>
        /// <value>
        /// The node matching settings.
        /// </value>
        public NavigationNodeMatchingSettings NodeMatchingSettings { get; set; }
    }
}
