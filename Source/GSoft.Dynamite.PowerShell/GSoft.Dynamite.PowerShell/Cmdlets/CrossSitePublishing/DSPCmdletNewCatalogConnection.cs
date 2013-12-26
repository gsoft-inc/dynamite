using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing
{
    /// <summary>
    /// Cmdlet for creating a catalog connection
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DSPCatalogConnection")]
    public class DspCmdletNewCatalogConnection : Cmdlet
    {
        private XDocument _configurationFile;

        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The path to the file containing the catalog connection configuration or an XmlDocument object or XML string.",
            Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        protected override void EndProcessing()
        {
            var xml = InputFile.Read();
            _configurationFile = xml.ToXDocument();

            // Get all webs nodes
            var webNodes = from webNode in _configurationFile.Descendants("Web")
                       select (webNode);

            foreach (var webNode in webNodes)
            {
                var webUrl = webNode.Attribute("Url").Value;
                using (var site = new SPSite(webUrl))
                {
                    using (var web = site.OpenWeb())
                    {
                        var catalogManager = new CatalogConnectionManager(site);
                        
                        // Get catalog connection nodes
                        var catalogConnectionNodes = webNode.Descendants("CatalogConnection");

                        // Add each connection to the catalog manager
                        foreach (var catalogConnection in catalogConnectionNodes.Select(x => GetCatalogConnectionSettingsFromNode(web, x)))
                        {
                            catalogManager.AddCatalogConnection(catalogConnection);
                        }

                        catalogManager.Update();
                    }
                }
            }

            base.EndProcessing();
        }

        private static CatalogConnectionSettings GetCatalogConnectionSettingsFromNode(SPWeb web, XElement node)
        {
            // Fetch catalog settings with utility
            var catalogUrl = SPUtility.ConcatUrls(node.Attribute("CatalogSiteUrl").Value, node.Attribute("SiteRelativeCatalogUrl").Value);
            var settings = PublishingCatalogUtility.GetPublishingCatalog(web.Site, catalogUrl);

            // Configure settings per XML
            settings.ConnectedWebId = web.ID;
            settings.ConnectedWebServerRelativeUrl = web.ServerRelativeUrl;
            settings.RewriteCatalogItemUrls = bool.Parse(node.Attribute("RewriteCatalogItemUrls").Value);
            settings.IsManualCatalogItemUrlRewriteTemplate = bool.Parse(node.Attribute("IsManualCatalogItemUrlRewriteTemplate").Value);
            settings.IsReusedWithPinning = bool.Parse(node.Attribute("IsReusedWithPinning").Value);
            settings.CatalogTaxonomyManagedProperty = node.Attribute("CatalogTaxonomyManagedProperty").Value;
            settings.CatalogItemUrlRewriteTemplate = node.Attribute("CatalogItemUrlRewriteTemplate").Value;

            return settings;
        }
    }
}
