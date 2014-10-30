using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Branding.Entities
{
    /// <summary>
    /// A composed look.
    /// </summary>
    public class ComposedLook : BaseEntity
    {
        private const int DefaultDisplayOrder = 100;

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
        /// <param name="masterPagePath">The web relative master page URL path (ex: /_catalogs/masterpage/custom.master).</param>
        /// <param name="themePath">The web relative theme URL path (ex: /_catalogs/theme/15/custom.spcolor).</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "SharePoint specific terminology accepted.")]
        public ComposedLook(SPWeb web, string name, string masterPagePath, string themePath)
        {
            // Init fields
            this.Name = name;
            this.MasterPagePath = GetServerRelativeUrlValue(web, masterPagePath);
            this.ThemePath = GetServerRelativeUrlValue(web, themePath);
            this.FontSchemePath = new UrlValue() { Url = string.Empty };
            this.ImagePath = new UrlValue() { Url = string.Empty };
            this.DisplayOrder = DefaultDisplayOrder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComposedLook" /> class and appends the web's server relative url.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="name">The name.</param>
        /// <param name="masterPagePath">The web relative master page URL path (ex: /_catalogs/masterpage/custom.master).</param>
        /// <param name="themePath">The web relative theme URL path (ex: /_catalogs/theme/15/custom.spcolor).</param>
        /// <param name="fontSchemePath">The web relative font scheme URL path (ex: /_catalogs/theme/15/custom.spfont).</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "SharePoint specific terminology accepted.")]
        public ComposedLook(SPWeb web, string name, string masterPagePath, string themePath, string fontSchemePath)
        {
            // Init fields
            this.Name = name;
            this.MasterPagePath = GetServerRelativeUrlValue(web, masterPagePath);
            this.ThemePath = GetServerRelativeUrlValue(web, themePath);
            this.FontSchemePath = GetServerRelativeUrlValue(web, fontSchemePath);
            this.ImagePath = new UrlValue() { Url = string.Empty };
            this.DisplayOrder = DefaultDisplayOrder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComposedLook" /> class and appends the web's server relative url.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="name">The name.</param>
        /// <param name="masterPagePath">The web relative master page URL path (ex: /_catalogs/masterpage/custom.master).</param>
        /// <param name="themePath">The web relative theme URL path (ex: /_catalogs/theme/15/custom.spcolor).</param>
        /// <param name="imagePath">The web relative image URL path (ex: /_layouts/15/images/custom.jpg).</param>
        /// <param name="fontSchemePath">The web relative font scheme URL path (ex: /_catalogs/theme/15/custom.spfont).</param>
        /// <param name="displayOrder">The display order in the list.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "SharePoint specific terminology accepted.")]
        public ComposedLook(SPWeb web, string name, string masterPagePath, string themePath, string imagePath = "", string fontSchemePath = "", int displayOrder = DefaultDisplayOrder)
        {
            // Init fields
            this.Name = name;
            this.MasterPagePath = GetServerRelativeUrlValue(web, masterPagePath);
            this.ThemePath = GetServerRelativeUrlValue(web, themePath);
            this.ImagePath = new UrlValue() { Url = imagePath };
            this.FontSchemePath = GetServerRelativeUrlValue(web, fontSchemePath);
            this.DisplayOrder = displayOrder;
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

        private static UrlValue GetServerRelativeUrlValue(SPWeb web, string webRelativeUrl)
        {
            var url = !string.IsNullOrEmpty(webRelativeUrl) ? SPUtility.ConcatUrls(web.ServerRelativeUrl, webRelativeUrl) : string.Empty;
            return new UrlValue() { Url = url };
        }
    }
}
