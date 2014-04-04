using System;
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography;
using System.Xml.Linq;

using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;

using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;
using Microsoft.Office.Server.Search.Query.Rules;
using Microsoft.Practices.Unity;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Search
{
    /// <summary>
    /// Creates result sources in the search service application
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DSPQueryRules")]

    // ReSharper disable once InconsistentNaming
    public class DSPCmdletremoveQueryRules : Cmdlet
    {
        /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private SearchHelper _searchHelper;

        private TaxonomyService _taxonomyService;

        private XDocument _configurationFile;

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        /// <value>
        /// The input file.
        /// </value>
        [Parameter(Mandatory = true, ValueFromPipeline = true, 
            HelpMessage =
                "The path to the file containing the result sources configuration or an XmlDocument object or XML string.", 
            Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        /// <summary>
        /// Ends the processing.
        /// </summary>
        protected override void EndProcessing()
        {
            this.ResolveDependencies();

            var xml = this.InputFile.Read();
            this._configurationFile = xml.ToXDocument();

            var rootNode = this._configurationFile.Root;
            if (rootNode != null)
            {
                var serviceApplicationName = rootNode.Attribute("SearchServiceApplication").Value;

                // Get the default search service application
                var searchServiceApp = this._searchHelper.GetDefaultSearchServiceApplication(serviceApplicationName);

                var queryRuleNodes = from sourceNode in this._configurationFile.Descendants("QueryRule") select sourceNode;

                foreach (var queryRuleNode in queryRuleNodes)
                {
                    var displayName = queryRuleNode.Attribute("DisplayName").Value;
                    var objectLevelAsString = queryRuleNode.Attribute("SearchObjectLevel").Value;

                    var searchObjectLevel = (SearchObjectLevel)Enum.Parse(typeof(SearchObjectLevel), objectLevelAsString);
                    var contextWeb = queryRuleNode.Attribute("ContextWeb").Value;

                    var site = new SPSite(contextWeb);
                    var web = site.OpenWeb(contextWeb);

                    var queryRules = this._searchHelper.GetQueryRulesByName(searchServiceApp, searchObjectLevel, web, displayName);

                    if (queryRules.Count > 0)
                    {
                            this.WriteWarning("Deleting query rule:" + displayName);
                            this._searchHelper.DeleteQueryRule(searchServiceApp, searchObjectLevel, web, displayName);                      
                    }

                    web.Dispose();
                    site.Dispose();
                }
            }

            base.EndProcessing();
        }

        /// <summary>
        /// Resolve Dependencies for helpers
        /// </summary>
        private void ResolveDependencies()
        {
            this._searchHelper = PowerShellContainer.Current.Resolve<SearchHelper>();
            this._taxonomyService = PowerShellContainer.Current.Resolve<TaxonomyService>();
        }
    }
}
