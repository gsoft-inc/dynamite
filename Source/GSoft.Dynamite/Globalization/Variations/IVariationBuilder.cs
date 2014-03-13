using Microsoft.SharePoint;

namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// The contract for any variations builder
    /// </summary>
    public interface IVariationBuilder
    {
        /// <summary>
        /// The configure variations settings method.
        /// </summary>
        /// <param name="site">
        /// The site collection.
        /// </param>
        void ConfigureVariationsSettings(SPSite site);

        /// <summary>
        /// The create variations method.
        /// </summary>
        /// <param name="site">
        /// The site collection.
        /// </param>
        void CreateVariations(SPSite site);

        /// <summary>
        /// The create hierarchies method.
        /// </summary>
        /// <param name="site">
        /// The site collection.
        /// </param>
        void CreateHierarchies(SPSite site);
    }
}
