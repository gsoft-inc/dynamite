using Microsoft.SharePoint;

namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// The variation director knows the order of calls to get Variations up and running
    /// </summary>
    public class DefaultVariationDirector : IVariationDirector
    {
        /// <summary>
        /// The construct.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <param name="site">
        /// The site.
        /// </param>
        public void Construct(IVariationBuilder builder, SPSite site)
        {
            builder.ConfigureVariationsSettings(site);

            builder.CreateVariations(site);

            builder.CreateHierarchies(site);
        }
    }
}
