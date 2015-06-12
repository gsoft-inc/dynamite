using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.ReusableContent
{
    /// <summary>
    /// Information on a Reusable Content
    /// </summary>
    /// <remarks>
    /// The pattern here is to fill the Filename and FolderInLayouts so the Helper can read a HTML file and fill the Content property with it.
    /// </remarks>
    public class ReusableContentInfo
    {
        public ReusableContentInfo(string title)
        {
            this.Title = title;
        }

        /// <summary>
        /// The Title of the Reusable Content. This is used as the key in the list.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The Category of the Reusable Content. In SharePoint it is stored as a choice.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Does the Reusable Content update itself when the source is updated (true) or is it a copy of the html content (false).
        /// </summary>
        public bool IsAutomaticUpdate { get; set; }

        /// <summary>
        /// Do we show the Reusable Content in the Ribbon Dropdown as available
        /// </summary>
        public bool IsShowInRibbon { get; set; }
        
        /// <summary>
        /// The HTML content of the Reusable Content
        /// </summary>
        public string Content { get; set; }

        #region Content HTML File information

        /// <summary>
        /// The filename of the reusable content HTML file. Eg: "footer.html"
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The folder inside the Layouts hive. Eg: "GSoft.Dynamite"
        /// </summary>
        public string FolderInLayouts { get; set; }

        /// <summary>
        /// The generated Path for the HTML file.
        /// </summary>
        public string HTMLFilePath
        {
            get
            {
                return SPUtility.GetVersionedGenericSetupPath(string.Format(CultureInfo.InvariantCulture, @"TEMPLATE\LAYOUTS\{0}\{1}", this.FolderInLayouts, this.Filename), 15);
            }
        }

        #endregion Content HTML File information
    }
}
