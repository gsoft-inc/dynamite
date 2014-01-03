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
using Microsoft.Office.Server.Search.Administration;
using Microsoft.SharePoint;
using GSoft.Dynamite.PowerShell.Unity;
using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Search
{
    /// <summary>
    /// Removes Search Result Sources confguration
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DSPResultSources")]
    public class DSPCmdletRemoveResultSources: Cmdlet
    {
        /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private SearchHelper _searchHelper;

        private XDocument _configurationFile;

        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The path to the file containing the result sources configuration or an XmlDocument object or XML string.",
            Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        protected override void EndProcessing()
        {
            this.ResolveDependencies();

            var xml = InputFile.Read();
            _configurationFile = xml.ToXDocument();

            var serviceApplicationName = _configurationFile.Root.Attribute("SearchServiceApplication").Value;

            var searchServiceApp = this._searchHelper.GetDefaultSearchServiceApplication(serviceApplicationName);

            var sourceNodes = from sourceNode in _configurationFile.Descendants("Source")
                              select (sourceNode);

            foreach (var sourceNode in sourceNodes)
            {
                var sourceName = sourceNode.Attribute("Name").Value;
                var objectLevelAsString = sourceNode.Attribute("SearchObjectLevel").Value;

                var sortObjectLevel = (SearchObjectLevel)Enum.Parse(typeof(SearchObjectLevel), objectLevelAsString);
                var contextWeb = sourceNode.Attribute("ContextWeb").Value;

                var spSite = new SPSite(contextWeb);
                var spWeb = spSite.OpenWeb(contextWeb);

                var doProcess = ShouldContinue("Are you sure?", "Delete all result sources");
                if(doProcess)
                {
                    WriteWarning("Deleting result source:" + sourceName);
                    this._searchHelper.DeleteResultSource(searchServiceApp, sourceName, sortObjectLevel, spWeb);
                }

                spWeb.Dispose();
                spSite.Dispose();
            }

            base.EndProcessing();
        }

        /// <summary>
        /// Resolve Dependencies for helpers
        /// </summary>
        private void ResolveDependencies()
        {
            this._searchHelper = PowerShellContainer.Current.Resolve<SearchHelper>();
        }
    }
}
