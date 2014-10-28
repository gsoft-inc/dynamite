namespace GSoft.Dynamite.Branding
{
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Publishing;

    public interface IImageRenditionHelper
    {
        /// <summary>
        /// Method to ensure an image matchingRenditions in the current site
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="imageRendition">The image matchingRenditions to add/update</param>
        void EnsureImageRendition(SPSite site, ImageRendition imageRendition);

        /// <summary>
        /// Method to remove an image matchingRenditions if is exist
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="imageRendition">The image matchingRenditions to remove</param>
        void RemoveImageRendition(SPSite site, ImageRendition imageRendition);

        /// <summary>
        /// Method to remove an image matchingRenditions if is exist
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="containsPattern">Pattern to match to remove Image Rendition</param>
        void RemoveImageRendition(SPSite site, string containsPattern);
    }
}