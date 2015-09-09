namespace GSoft.Dynamite.Search.Enums
{
    /// <summary>
    /// The update mode for the ensuring managed properties
    /// </summary>
    public enum ManagedPropertyUpdateBehavior
    {
        /// <summary>
        /// Don't make any changes to the managed property if it already exists
        /// </summary>
        NoChangesIfAlreadyExists = 0,

        /// <summary>
        /// Delete and recreate the managed property if it already exists
        /// </summary>
        OverwriteIfAlreadyExists = 1,

        /// <summary>
        /// Append the crawled properties to the existing mappings.
        /// If the appended crawled property is already mapped, it will not be changed.
        /// </summary>
        AppendCrawledProperties = 2,

        /// <summary>
        /// Overwrites all crawled property mappings.
        /// </summary>
        OverwriteCrawledProperties = 3,

        /// <summary>
        /// Updates the configuration (sortable, refinable, etc.) on the managed property,
        /// without changing the crawled property mappings or recreating the managed property.
        /// </summary>
        UpdateConfiguration = 4
    }
}
