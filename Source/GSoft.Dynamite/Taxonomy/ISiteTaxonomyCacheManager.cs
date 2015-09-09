using Microsoft.SharePoint;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Interface for a Site Taxonomy cache manager.
    /// </summary>
    public interface ISiteTaxonomyCacheManager
    {
        /// <summary>
        /// Method to get the Taxonomy Cache from a Site scope
        /// </summary>
        /// <param name="site">The site containing the cache</param>
        /// <param name="termStoreName">The name of the term store</param>
        /// <param name="taxonomyHelper">The taxonomy helper to use.</param>
        /// <returns>A site taxonomy cache</returns>
        SiteTaxonomyCache GetSiteTaxonomyCache(SPSite site, string termStoreName, ITaxonomyHelper taxonomyHelper);
    }
}
