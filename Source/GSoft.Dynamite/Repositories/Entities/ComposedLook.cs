using GSoft.Dynamite.Binding;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Repositories.Entities
{
    /// <summary>
    /// A composed look.
    /// </summary>
    public class ComposedLook : BaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComposedLook"/> class.
        /// </summary>
        public ComposedLook()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComposedLook" /> class and appends the web's server relative url.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="name">The name.</param>
        /// <param name="masterPagePath">The master page path.</param>
        /// <param name="themePath">The theme path.</param>
        /// <param name="imagePath">The image path.</param>
        /// <param name="fontSchemePath">The font scheme path.</param>
        /// <param name="displayOrder">The display order.</param>
        public ComposedLook(SPWeb web, string name, string masterPagePath, string themePath, string imagePath = "", string fontSchemePath = "", int displayOrder = 100)
        {
            // Init fields
            this.FontSchemePath = new UrlValue();
            this.ThemePath = new UrlValue();
            this.ImagePath = new UrlValue();
            this.MasterPagePath = new UrlValue();
            this.Name = name;
            this.DisplayOrder = displayOrder;
            this.MasterPagePath.Url = !string.IsNullOrEmpty(masterPagePath) ? SPUtility.ConcatUrls(web.ServerRelativeUrl, masterPagePath) : masterPagePath;
            this.ImagePath.Url = imagePath;
            this.ThemePath.Url = !string.IsNullOrEmpty(themePath) ? SPUtility.ConcatUrls(web.Site.ServerRelativeUrl, themePath) : themePath;
            this.FontSchemePath.Url = !string.IsNullOrEmpty(fontSchemePath) ? SPUtility.ConcatUrls(web.Site.ServerRelativeUrl, fontSchemePath) : fontSchemePath;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Property]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order.
        /// </summary>
        /// <value>
        /// The display order.
        /// </value>
        [Property]
        public double DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the master page path.
        /// </summary>
        /// <value>
        /// The master page path.
        /// </value>
        [Property(BuiltInFields.MasterPageUrlName)]
        public UrlValue MasterPagePath { get; set; }

        /// <summary>
        /// Gets or sets the image path.
        /// </summary>
        /// <value>
        /// The image path.
        /// </value>
        [Property(BuiltInFields.ImageUrlName)]
        public UrlValue ImagePath { get; set; }

        /// <summary>
        /// Gets or sets the theme path.
        /// </summary>
        /// <value>
        /// The theme path.
        /// </value>
        [Property(BuiltInFields.ThemeUrlName)]
        public UrlValue ThemePath { get; set; }

        /// <summary>
        /// Gets or sets the font scheme path.
        /// </summary>
        /// <value>
        /// The font scheme path.
        /// </value>
        [Property(BuiltInFields.FontSchemeUrlName)]
        public UrlValue FontSchemePath { get; set; }
    }
}
