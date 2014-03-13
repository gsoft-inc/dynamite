namespace GSoft.Dynamite.Variations
{
    using Microsoft.SharePoint;

    /// <summary>
    /// The Variation director interface
    /// </summary>
    public interface IVariationDirector
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
        void Construct(IVariationBuilder builder, SPSite site);
    }
}