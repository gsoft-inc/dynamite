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
        /// <param name="width">The  Width</param>
        /// <param name="height">The Height</param>
        public ImageRenditionInfo(string name, int width, int height)
        {
            this.Name = name;
            this.Width = width;
            this.Height = height;
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
        /// Gets or Sets the name
        /// </summary>
        /// <value>
        /// The name of the image rendition
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the width
        /// </summary>
        /// <value>
        /// The width of the image
        /// </value>
        public int Width { get; set; }

        /// <summary>
        /// Gets or Sets the height
        /// </summary>
        /// <value>
        /// The height of the image
        /// </value>
        public int Height { get; set; }
    }
}
