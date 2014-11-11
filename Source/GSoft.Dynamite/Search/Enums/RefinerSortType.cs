namespace GSoft.Dynamite.Search.Enums
{
    /// <summary>
    /// Sort by type for a refiner
    /// </summary>
    public enum RefinerSortBy
    {
        /// <summary>
        /// Sort by name (for text refiners)
        /// </summary>
        Name = 1,
        
        /// <summary>
        /// Sort by count (i.e. the number of results that match this refiner)
        /// </summary>
        Count = 0,

        /// <summary>
        /// Sort by number (for integer refiners)
        /// </summary>
        Number = 2
    }
}
