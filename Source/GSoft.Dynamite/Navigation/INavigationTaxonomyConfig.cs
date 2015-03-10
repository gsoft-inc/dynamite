using System;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Navigation taxonomy related configuration.
    /// </summary>
    public interface INavigationTaxonomyConfig
    {
        /// <summary>
        /// Gets the filter term set identifier.
        /// </summary>
        /// <value>
        /// The filter term set identifier.
        /// </value>
        Guid FilterTermSetId { get; }

        /// <summary>
        /// Gets or sets the maximum depth to fetch the taxonomy tree.
        /// </summary>
        /// <value>
        /// The maximum depth.
        /// </value>
        int MaximumDepth { get; set; }
    }
}
