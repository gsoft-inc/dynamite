using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.SharePoint;
using GSoft.Dynamite.PowerShell.Unity;
using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing
{
    /// <summary>
    /// Removes catalogs configuration
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DSPCatalogs")]
    public class DSPCmdletRemoveCatalogs :Cmdlet
    {
        /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private ListHelper _listHelper;

        private XDocument _configurationFile = null;

        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The path to the file containing the terms to import or an XmlDocument object or XML string.",
            Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        protected override void EndProcessing()
        {
            this.ResolveDependencies();

            var xml = InputFile.Read();
            _configurationFile = xml.ToXDocument();

            // Get all webs nodes
            var webNodes = from webNode in _configurationFile.Descendants("Web")
                           select (webNode);

            foreach (var webNode in webNodes)
            {
                var webUrl = webNode.Attribute("Url").Value;

                using (var spSite = new SPSite(webUrl))
                {
                    var spWeb = spSite.OpenWeb();

                    // Get all catalogs nodes
                    var catalogNodes = from catalogNode in webNode.Descendants("Catalog")
                                       select (catalogNode);

                    foreach (var catalogNode in catalogNodes)
                    {
                        var catalogUrl = catalogNode.Attribute("RootFolderUrl").Value;

                        var isContinue = ShouldContinue("Are you sure?", "Delete Catalogs configuration");

                        if(isContinue)
                        {
                            // Create the list if doesn't exists
                            var list = this._listHelper.GetListByRootFolderUrl(spWeb, catalogUrl);

                            if (list != null)
                            {
                                WriteWarning("Delete the list " + catalogUrl);

                                // Delete the list
                                list.Delete();
                            }
                            else
                            {
                                WriteWarning("No list with the name " + catalogUrl);
                            }
                        }                    
                     }
                }                          
            }

            base.EndProcessing();

        }

        /// <summary>
        /// Resolve Dependencies for helpers
        /// </summary>
        private void ResolveDependencies()
        {
            this._listHelper = PowerShellContainer.Current.Resolve<ListHelper>();
        }
    }
}
