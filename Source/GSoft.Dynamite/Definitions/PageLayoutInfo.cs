namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a page layout info
    /// </summary>
    public class PageLayoutInfo
    {
        /// <summary>
        /// Name of the Page Layout
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Names of the zones in the page layout
        /// </summary>
        public string[] ZoneNames { get; set; }

        /// <summary>
        /// The associated content type id
        /// </summary>
        public string AssociatedContentTypeId { get; set; } 
    }
}
