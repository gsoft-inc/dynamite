using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Xml.Serialization;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using ImageRendition = GSoft.Dynamite.PowerShell.Cmdlets.Renditions.Entities.ImageRenditionDefinition;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Renditions
{
    /// <summary>
    /// Cmdlet for managed metadata navigation configuration
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DSPImageRenditions")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once InconsistentNaming
    public class DSPCmdletRemoveImageRenditions : Cmdlet
    {
        private XmlSerializer _serializer;
        
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
        protected override void EndProcessing()
        {
            // Initialize XML serializer
            this._serializer = new XmlSerializer(typeof(ImageRendition));

            var xml = this.InputFile.Read();
            var configurationXml = xml.ToXDocument();

            // Get all site nodes
            var siteNodes = from siteNode in configurationXml.Descendants("Site") select siteNode;
            foreach (var siteNode in siteNodes)
            {
                var siteUrl = siteNode.Attribute("Url").Value;

                // Write verbose information
                this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Removing image renditions on site '{0}'", siteUrl));
                using (var site = new SPSite(siteUrl))
                {
                    // Get all image rendition definitions
                    var renditionDefinitions = from rendition in siteNode.Descendants("ImageRendition")
                                               select (ImageRendition)this._serializer.Deserialize(rendition.CreateReader());

                    var renditionCollection = SiteImageRenditions.GetRenditions(site);
                    foreach (var renditionDefinition in renditionDefinitions)
                    {
                        var definition = renditionDefinition;
                        var rendition = renditionCollection.FirstOrDefault(
                            x => x.Name.Equals(definition.Name, StringComparison.OrdinalIgnoreCase) &&
                                        (x.Width == definition.Width) &&
                                        (x.Height == definition.Height));

                        if (rendition != null)
                        {
                            // Write verbose information
                            this.WriteVerbose(
                                string.Format(
                                CultureInfo.InvariantCulture,
                                "Removing image rendition '{0}' with width '{1}' and height '{2}'",
                                renditionDefinition.Name,
                                renditionDefinition.Width,
                                renditionDefinition.Height));

                            renditionCollection.Remove(rendition);
                        }
                        else
                        {
                            // Write warning information
                            this.WriteWarning(
                                string.Format(
                                CultureInfo.InvariantCulture,
                                "Could not find image rendition '{0}' with width '{1}' and height '{2}'",
                                renditionDefinition.Name,
                                renditionDefinition.Width,
                                renditionDefinition.Height));
                        }
                    }

                    // Write verbose information
                    this.WriteVerbose("Updating image rendition collection");
                    renditionCollection.Update();
                }
            }

            base.EndProcessing();
        }
    }
}
