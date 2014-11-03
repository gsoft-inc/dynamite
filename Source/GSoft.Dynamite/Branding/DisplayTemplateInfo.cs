using System.Globalization;

namespace GSoft.Dynamite.Branding
{
    /// <summary>
    /// The various Display Template categories
    /// </summary>
    public enum DisplayTemplateCategory
    {
        /// <summary>
        /// Search display templates
        /// </summary>
        Search = 1,

        /// <summary>
        /// Content Search Web Part display templates
        /// </summary>
        ContentSearch = 2,

        /// <summary>
        /// Filters display templates
        /// </summary>
        Filter = 3
    }

    /// <summary>
    /// Easily serializable representation of a display template's metadata
    /// </summary>
    public class DisplayTemplateInfo
    {
        private readonly DisplayTemplateCategory category;

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
            this.category = displayTemplateCategory;
        }

        /// <summary>
        /// Internal name of the display template
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The HTML file name
        /// </summary>
        public string HtmlFileName { get; private set; }

        /// <summary>
        /// The JavaScript file name
        /// </summary>
        public string JavaScriptFileName { get; private set; }

        /// <summary>
        /// The folder category of the display template
        /// </summary>
        public string Category 
        {
            get
            {
                switch (this.category)
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
        /// Path to the Display Template
        /// </summary>
        public string ItemTemplateUrl 
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
