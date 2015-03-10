namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Navigation related configuration.
    /// </summary>
    public interface INavigationConfig
    {
        /// <summary>
        /// Gets the taxonomy configuration.
        /// </summary>
        /// <value>
        /// The taxonomy configuration.
        /// </value>
        INavigationTaxonomyConfig TaxonomyConfig { get; }

        /// <summary>
        /// Gets the search configuration.
        /// </summary>
        /// <value>
        /// The search configuration.
        /// </value>
        INavigationSearchConfig SearchConfig { get; }
    }
}
