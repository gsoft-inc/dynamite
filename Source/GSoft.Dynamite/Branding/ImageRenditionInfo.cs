using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Branding
{
    /// <summary>
    /// A simple POCO that represent a Image Rendition.
    /// </summary>
    public class ImageRenditionInfo
    {
        /// <summary>
        /// Empty constructor for serialization purposes
        /// </summary>
        public ImageRenditionInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageRenditionInfo"/> class.
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="pixelWidth">The  Width</param>
        /// <param name="pixelHeight">The Height</param>
        public ImageRenditionInfo(string name, int pixelWidth, int pixelHeight)
        {
            this.Name = name;
            this.Width = pixelWidth;
            this.Height = pixelHeight;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageRenditionInfo"/> class.
        /// </summary>
        /// <param name="imageRendition">The image rendition.</param>
        public ImageRenditionInfo(ImageRendition imageRendition)
        {
            this.Name = imageRendition.Name;
            this.Width = imageRendition.Width;
            this.Height = imageRendition.Height;
        }

        /// <summary>
        /// The name of the image rendition
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The width of the image in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the image in pixels
        /// </summary>
        public int Height { get; set; }
    }
}
