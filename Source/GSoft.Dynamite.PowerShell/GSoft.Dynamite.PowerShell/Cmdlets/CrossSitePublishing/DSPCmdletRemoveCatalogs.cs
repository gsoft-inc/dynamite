using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Autofac;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using Microsoft.SharePoint;
using Microsoft.SharePoint.PowerShell;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing
{
    /// <summary>
    /// Removes catalogs configuration
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DSPCatalogs")]

    // ReSharper disable once InconsistentNaming
    public class DSPCmdletRemoveCatalogs : SPCmdlet
    {
        private XDocument configurationFile;

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The path to the file containing the terms to import or an XmlDocument object or XML string.", Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        /// <summary>
        /// Ends the processing.
        /// </summary>
        protected override void InternalEndProcessing()
        {
            var xml = this.InputFile.Read();
            this.configurationFile = xml.ToXDocument();

            // Get all webs nodes
            var webNodes = from webNode in this.configurationFile.Descendants("Web") select webNode;

            foreach (var webNode in webNodes)
            {
                var webUrl = webNode.Attribute("Url").Value;

                using (var site = new SPSite(webUrl))
                {
                    var web = site.OpenWeb();

                    // Get all catalogs nodes
                    var catalogNodes = from catalogNode in webNode.Descendants("Catalog") select catalogNode;

                    foreach (var catalogNode in catalogNodes)
                    {
                        var catalogUrl = catalogNode.Attribute("RootFolderUrl").Value;

                        var isContinue = ShouldContinue("Are you sure?", "Delete Catalogs configuration");

                        if (isContinue)
                        {
                            using (var childScope = PowerShellContainer.BeginLifetimeScope(web))
                            {
                                var listHelper = childScope.Resolve<IListHelper>();

                                // Create the list if doesn't exists
                                var list = listHelper.GetListByRootFolderUrl(web, catalogUrl);

                                if (list != null)
                                {
                                    this.WriteWarning("Delete the list " + catalogUrl);

                                    // Delete the list
                                    list.Delete();
                                }
                                else
                                {
                                    this.WriteWarning("No list with the name " + catalogUrl);
                                }
                            }
                        }
                    }
                }
            }

            base.InternalEndProcessing();
        }
    }
}
