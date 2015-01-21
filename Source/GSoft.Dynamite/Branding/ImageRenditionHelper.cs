using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Branding
{
    /// <summary>
    /// Helper class to work with image matchingRenditions
    /// </summary>
    public class ImageRenditionHelper : IImageRenditionHelper
    {
        private readonly ILogger logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">The logger</param>
        public ImageRenditionHelper(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Method to ensure an image matchingRenditions in the current site
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="imageRenditionInfo">The image matchingRenditions to add/update</param>
        public void EnsureImageRendition(SPSite site, ImageRenditionInfo imageRenditionInfo)
        {
            var imageRendition = SetImageRenditionProperties(imageRenditionInfo);

            // Error checking
            if (site == null || imageRendition == null || !imageRendition.IsValid)
            {
                this.logger.Error("Error ensuring image rendition. Argument is null or invalid.");
                throw new ArgumentException("ImageRenditionHelper : argument is null of invalid.");
            }

            // Get the image existingImageRendition collection of the current site.
            var imageRenditionCollection = SiteImageRenditions.GetRenditions(site);

            if (!imageRenditionCollection.Any(x => x.Name == imageRendition.Name))
            {
                this.logger.Info("Adding image rendition '{0}' with width '{1}' and height '{2}'", imageRendition.Name, imageRendition.Width, imageRendition.Height);
                imageRenditionCollection.Add(imageRendition);
            }
            else
            {
                this.logger.Info("Updating image rendition '{0}' with width '{1}' and height '{2}'", imageRendition.Name, imageRendition.Width, imageRendition.Height);

                var existingImageRendition = imageRenditionCollection.First(x => x.Name == imageRendition.Name);

                existingImageRendition.Width = imageRendition.Width;
                existingImageRendition.Height = imageRendition.Height;
            }

            imageRenditionCollection.Update();
        }

        /// <summary>
        /// Method to remove an image matchingRenditions if is exist
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="imageRenditionInfo">The image matchingRenditions to remove</param>
        public void RemoveImageRendition(SPSite site, ImageRenditionInfo imageRenditionInfo)
        {
            var imageRendition = SetImageRenditionProperties(imageRenditionInfo);

            if (site == null || imageRendition == null || !imageRendition.IsValid)
            {
                this.logger.Error("Error removing image rendition. Argument is null or invalid.");
                throw new ArgumentException("ImageRenditionHelper : argument is null of invalid.");
            }

            // Get the image existingImageRendition collection of the current site.
            var imageRenditionCollection = SiteImageRenditions.GetRenditions(site);

            var existingRendition = imageRenditionCollection.FirstOrDefault(x => x.Name == imageRendition.Name && x.Width == imageRendition.Width && x.Height == imageRendition.Height);

            if (existingRendition != null)
            {
                imageRenditionCollection.Remove(existingRendition);
                this.logger.Info("Removing image rendition '{0}' with width '{1}' and height '{2}'", imageRendition.Name, imageRendition.Width, imageRendition.Height);
            }

            imageRenditionCollection.Update();
        }

        /// <summary>
        /// Method to remove an image matchingRenditions if is exist
        /// </summary>
        /// <param name="site">The current site</param>
        /// <param name="containsPattern">Pattern to match to remove Image Rendition</param>
        public void RemoveImageRendition(SPSite site, string containsPattern)
        {
            if (site == null || string.IsNullOrEmpty(containsPattern))
            {
                this.logger.Error("Error removing image rendition. Argument is null or invalid.");
                throw new ArgumentException("ImageRenditionHelper : argument is null of invalid.");
            }

            // Get the image existingImageRendition collection of the current site.
            var imageRenditionCollection = SiteImageRenditions.GetRenditions(site);

            var matchingRenditions = imageRenditionCollection.Where(x => x.Name.ToUpperInvariant().Contains(containsPattern.ToUpperInvariant())).ToList();

            if (matchingRenditions != null && matchingRenditions.Any())
            {
                foreach (var imageRendition in matchingRenditions)
                {
                    imageRenditionCollection.Remove(imageRendition);
                    this.logger.Info("Removing image rendition '{0}' with width '{1}' and height '{2}'", imageRendition.Name, imageRendition.Width, imageRendition.Height);
                }
            }

            imageRenditionCollection.Update();
        }

        private static ImageRendition SetImageRenditionProperties(ImageRenditionInfo imageRenditionInfo)
        {
            var imageRendition = new ImageRendition();
            imageRendition.Name = imageRenditionInfo.Name;
            imageRendition.Height = imageRenditionInfo.Height;
            imageRendition.Width = imageRenditionInfo.Width;

            return imageRendition;
        }
    }
}
