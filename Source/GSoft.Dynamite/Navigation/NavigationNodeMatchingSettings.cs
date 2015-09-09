namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Navigation node matching settings.
    /// </summary>
    public class NavigationNodeMatchingSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether [restrict to current navigation level].
        /// </summary>
        /// <value>
        /// <c>true</c> if [restrict to current navigation level]; otherwise, <c>false</c>.
        /// </value>
        public bool RestrictToCurrentNavigationLevel { get; set; }

        /// <summary>
        /// Gets a value indicating whether [restrict to reachable target items].
        /// </summary>
        /// <value>
        /// <c>true</c> if [restrict to reachable target items]; otherwise, <c>false</c>.
        /// </value>
        public bool RestrictToReachableTargetItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include catalog items].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include catalog items]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeCatalogItems { get; set; }
    }
}
