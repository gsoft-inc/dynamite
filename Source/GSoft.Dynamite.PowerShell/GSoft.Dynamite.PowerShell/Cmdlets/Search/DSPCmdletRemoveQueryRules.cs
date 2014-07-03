using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Autofac;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Utils;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.SharePoint;
using Microsoft.SharePoint.PowerShell;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Search
{
    /// <summary>
    /// Creates result sources in the search service application
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DSPQueryRules")]
    public class DSPCmdletremoveQueryRules : SPCmdlet
    {
        private XDocument configurationFile;

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        /// <value>
        /// The input file.
        /// </value>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The path to the file containing the result sources configuration or an XmlDocument object or XML string.", Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        /// <summary>
        /// Ends the processing.
        /// </summary>
        protected override void InternalEndProcessing()
        {
            var xml = this.InputFile.Read();
            this.configurationFile = xml.ToXDocument();

            var rootNode = this.configurationFile.Root;
            if (rootNode != null)
            {
                var serviceApplicationName = rootNode.Attribute("SearchServiceApplication").Value;
                var queryRuleNodes = from sourceNode in this.configurationFile.Descendants("QueryRule") select sourceNode;

                foreach (var queryRuleNode in queryRuleNodes)
                {
                    var displayName = queryRuleNode.Attribute("DisplayName").Value;
                    var objectLevelAsString = queryRuleNode.Attribute("SearchObjectLevel").Value;
                    var searchObjectLevel = (SearchObjectLevel)Enum.Parse(typeof(SearchObjectLevel), objectLevelAsString);
                    var contextWeb = queryRuleNode.Attribute("ContextWeb").Value;

                    using (var site = new SPSite(contextWeb))
                    {
                        using (var web = site.OpenWeb(contextWeb))
                        {
                            using (var childScope = PowerShellContainer.BeginWebLifetimeScope(web))
                            {
                                var searchHelper = childScope.Resolve<SearchHelper>();
                                var searchServiceApp = searchHelper.GetDefaultSearchServiceApplication(serviceApplicationName);
                                var queryRules = searchHelper.GetQueryRulesByName(searchServiceApp, searchObjectLevel, web, displayName);

                                if (queryRules.Any())
                                {
                                    this.WriteWarning("Deleting query rule:" + displayName);
                                    searchHelper.DeleteQueryRule(searchServiceApp, searchObjectLevel, web, displayName);
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
