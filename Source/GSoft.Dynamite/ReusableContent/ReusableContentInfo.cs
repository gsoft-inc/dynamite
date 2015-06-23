using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Fields.Constants;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.ReusableContent
{
    /// <summary>
    /// Information on a Reusable Content
    /// </summary>
    /// <remarks>
    /// The pattern here is to fill the Filename and FolderInLayouts so the Helper can read a HTML file and fill the Content property with it.
    /// </remarks>
    public class ReusableContentInfo : BaseEntity
    {
        /// <summary>
        /// Simple constructor.
        /// </summary>
        public ReusableContentInfo()
        {
        }

        /// <summary>
        /// Constructor with needed properties
        /// </summary>
        /// <param name="title">The title of the reusable content</param>
        /// <param name="category">The category (choice)</param>
        /// <param name="isAutomaticUpdate">If the reusable content update automatically, 
        /// the content of every inserted reusable content will be changed if the item in the list is changed.
        /// Otherwise, if false, when inserting a reusable content, it will duplicate the html and will be independant to the item in the reusable content list.</param>
        /// <param name="isShowInRibbon">Do we show the reusable content in the ribbon</param>
        /// <param name="fileName">The filename of the html content</param>
        /// <param name="folderInLayouts">The folder inside the Layouts hive</param>
        public ReusableContentInfo(string title, string category, bool isAutomaticUpdate, bool isShowInRibbon, string fileName, string folderInLayouts)
            : this()
        {
            this.Title = title;
            this.Category = category;
            this.IsAutomaticUpdate = isAutomaticUpdate;
            this.IsShowInRibbon = isShowInRibbon;
            this.FileName = fileName;
            this.FolderInLayouts = folderInLayouts;
        }

        /// <summary>
        /// The Category of the Reusable Content. In SharePoint it is stored as a choice.
        /// </summary>
        [Property(PublishingFields.ContentCategoryName)]
        public string Category { get; private set; }

        /// <summary>
        /// Does the Reusable Content update itself when the source is updated (true) or is it a copy of the html content (false).
        /// </summary>
        [Property(PublishingFields.AutomaticUpdateName)]
        public bool IsAutomaticUpdate { get; private set; }

        /// <summary>
        /// Do we show the Reusable Content in the Ribbon Dropdown as available
        /// </summary>
        [Property(PublishingFields.ShowInRibbonName)]
        public bool IsShowInRibbon { get; private set; }

        /// <summary>
        /// The HTML content of the Reusable Content
        /// </summary>
        [Property(PublishingFields.ReusableHtmlName)]
        public string Content { get; set; }

        #region Content HTML File information

        /// <summary>
        /// The filename of the reusable content HTML file. Eg: "footer.html"
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// The folder inside the Layouts hive. Eg: "GSoft.Dynamite"
        /// </summary>
        public string FolderInLayouts { get; private set; }

        /// <summary>
        /// The generated Path for the HTML file.
        /// </summary>
        public string HTMLFilePath
        {
            get
            {
                return SPUtility.GetVersionedGenericSetupPath(string.Format(CultureInfo.InvariantCulture, @"TEMPLATE\LAYOUTS\{0}\{1}", this.FolderInLayouts, this.FileName), 15);
            }
        }

        #endregion Content HTML File information
    }
}
