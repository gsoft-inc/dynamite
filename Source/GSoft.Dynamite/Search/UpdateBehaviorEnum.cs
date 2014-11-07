namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// The update mode for the result source
    /// </summary>
    public enum UpdateBehavior
    {
        /// <summary>
        /// Delete and recreate the result source if already exists
        /// </summary>
        OverwriteResultSource,

        /// <summary>
        /// Overwrite only the query string of the result source
        /// </summary>
        OverwriteQuery,

        /// <summary>
        /// Append string to the existing query
        /// </summary>
        AppendToQuery,

        /// <summary>
        /// Rollback the query to its previous state
        /// </summary>
        RevertQuery,

        /// <summary>
        /// Don't make any changes on the result source if already exists
        /// </summary>
        NoChangesIfAlreadyExists
    }
}
