namespace GSoft.Dynamite.Definitions
{
    public class DisplayTemplateInfo
    {
        private readonly DisplayTemplateCategory _category;

        public DisplayTemplateInfo(string displayTemplateName, DisplayTemplateCategory displayTemplateCategory)
        {
            this.Name = displayTemplateName;
            this.HtmlFileName = displayTemplateName + ".html";
            this.JavascriptFileName = displayTemplateName + ".js";
            this._category = displayTemplateCategory;
            this.ItemTemplateIdUrl = "~sitecollection/_catalogs/masterpage/Display Templates/" + Category + "/" + JavascriptFileName;
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
        /// The Javascript file name
        /// </summary>
        public string JavascriptFileName { get; private set; }

        /// <summary>
        /// The folder category of the display template
        /// </summary>
        public string Category {
            get
            {
                switch (_category)
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

        public string ItemTemplateIdUrl { get; private set; }
    }

    public enum DisplayTemplateCategory
    {
        Search =1,
        ContentSearch = 2,
        Filter =3
    }
}
