namespace GSoft.Dynamite.Branding
{
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Publishing;

    /// <summary>
    /// Utility to help manage image renditions
    /// </summary>
    public interface IImageRenditionHelper
    {
        /// <summary>
        /// Method to ensure an image matchingRenditions in the current site
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="imageRenditionInfo">The image matchingRenditions to add/update</param>
        void EnsureImageRendition(SPSite site, ImageRenditionInfo imageRenditionInfo);

        /// <summary>
        /// Method to remove an image matchingRenditions if is exist
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="imageRenditionInfo">The image matchingRenditions to remove</param>
        void RemoveImageRendition(SPSite site, ImageRenditionInfo imageRenditionInfo);

        /// <summary>
        /// Method to remove an image matchingRenditions if is exist
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="containsPattern">Pattern to match to remove Image Rendition</param>
        void RemoveImageRendition(SPSite site, string containsPattern);
    }
}