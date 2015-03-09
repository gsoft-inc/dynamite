using System.Globalization;

namespace GSoft.Dynamite.Branding
{
    /// <summary>
    /// Easily serializable representation of a display template's metadata
    /// </summary>
    public class DisplayTemplateInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public DisplayTemplateInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DisplayTemplateInfo"/>
        /// </summary>
        /// <param name="displayTemplateName">Name of the display template</param>
        /// <param name="displayTemplateCategory">Category of the display template</param>
        public DisplayTemplateInfo(string displayTemplateName, DisplayTemplateCategory displayTemplateCategory)
        {
            this.Name = displayTemplateName;
            this.HtmlFileName = displayTemplateName + ".html";
            this.JavaScriptFileName = displayTemplateName + ".js";
            this.Category = displayTemplateCategory;
        }

        /// <summary>
        /// Internal name of the display template
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The HTML file name
        /// </summary>
        public string HtmlFileName { get; set; }

        /// <summary>
        /// The JavaScript file name
        /// </summary>
        public string JavaScriptFileName { get; set; }

        /// <summary>
        /// The folder category of the display template
        /// </summary>
        public string CategoryFolderName
        {
            get
            {
                switch (this.Category)
                {
                    case DisplayTemplateCategory.Search:
                        return "Search";

                    case DisplayTemplateCategory.ContentSearch:
                        return "Content Web Parts";

                    case DisplayTemplateCategory.Filter:
                        return "Filters";

                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Display template category
        /// </summary>
        public DisplayTemplateCategory Category
        {
            get;
            set;
        }

        /// <summary>
        /// Path to the Display Template
        /// </summary>
        public string ItemTemplateTokenizedPath 
        { 
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "~sitecollection/_catalogs/masterpage/Display Templates/{0}/{1}",
                    this.Category,
                    this.JavaScriptFileName);
            }
        }
    }
}
