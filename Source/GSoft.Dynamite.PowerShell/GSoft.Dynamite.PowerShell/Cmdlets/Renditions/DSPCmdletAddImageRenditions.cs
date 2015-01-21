using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Xml.Serialization;
using Autofac;
using GSoft.Dynamite.Branding;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using Microsoft.SharePoint;
using Microsoft.SharePoint.PowerShell;
using Microsoft.SharePoint.Publishing;
using PSImageRendition = GSoft.Dynamite.PowerShell.Cmdlets.Renditions.Entities.ImageRenditionDefinition;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Renditions
{
    /// <summary>
    /// Cmdlet for managed metadata navigation configuration
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "DSPImageRenditions")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once InconsistentNaming
    public class DSPCmdletAddImageRenditions : SPCmdlet
    {
        private XmlSerializer serializer;

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The path to the file containing the image rendition configuration or an XmlDocument object or XML string.",
            Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        /// <summary>
        /// The end processing.
        /// </summary>
        protected override void InternalEndProcessing()
        {
            // Initialize XML serializer
            this.serializer = new XmlSerializer(typeof(PSImageRendition));

            var xml = this.InputFile.Read();
            var configurationXml = xml.ToXDocument();

            // Get all site nodes
            var siteNodes = from siteNode in configurationXml.Descendants("Site") select siteNode;
            foreach (var siteNode in siteNodes)
            {
                var siteUrl = siteNode.Attribute("Url").Value;

                // Write verbose information
                this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Adding image renditions to site '{0}'", siteUrl));
                using (var site = new SPSite(siteUrl))
                {
                    using (var childScope = PowerShellContainer.BeginLifetimeScope(site))
                    {
                        var imageRenditionHelper = childScope.Resolve<IImageRenditionHelper>();

                        // Get all image existingImageRendition definitions
                        var renditionDefinitions = from rendition in siteNode.Descendants("ImageRendition")
                                                   select (PSImageRendition)this.serializer.Deserialize(rendition.CreateReader());

                        // var renditionCollection = SiteImageRenditions.GetRenditions(site);
                        foreach (var renditionDefinition in renditionDefinitions)
                        {
                            imageRenditionHelper.EnsureImageRendition(
                                site,
                                new ImageRenditionInfo()
                                {
                                    Name = renditionDefinition.Name,
                                    Height = renditionDefinition.Height,
                                    Width = renditionDefinition.Width
                                });

                            // Write verbose information
                            this.WriteVerbose(
                                string.Format(
                                CultureInfo.InvariantCulture,
                                "Updating image rendition '{0}' with width '{1}' and height '{2}'",
                                renditionDefinition.Name,
                                renditionDefinition.Width,
                                renditionDefinition.Height));
                        }
                    }
                }
            }

            base.InternalEndProcessing();
        }
    }
}
