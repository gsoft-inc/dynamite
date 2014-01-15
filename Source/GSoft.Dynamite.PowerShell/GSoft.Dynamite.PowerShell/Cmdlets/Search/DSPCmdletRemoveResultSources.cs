using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Utils;

using Microsoft.Office.Server.Search.Administration;
using Microsoft.Practices.Unity;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Search
{
    /// <summary>
    /// Removes Search Result Sources configuration
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DSPResultSources")]

    // ReSharper disable once InconsistentNaming
    public class DSPCmdletRemoveResultSources : Cmdlet
    {
        /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private SearchHelper _searchHelper;

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

            var serviceApplicationName = this._configurationFile.Root.Attribute("SearchServiceApplication").Value;
            var searchServiceApp = this._searchHelper.GetDefaultSearchServiceApplication(serviceApplicationName);
            var sourceNodes = from sourceNode in this._configurationFile.Descendants("Source") select sourceNode;

            foreach (var sourceNode in sourceNodes)
            {
                var sourceName = sourceNode.Attribute("Name").Value;
                var objectLevelAsString = sourceNode.Attribute("SearchObjectLevel").Value;

                var sortObjectLevel = (SearchObjectLevel)Enum.Parse(typeof(SearchObjectLevel), objectLevelAsString);
                var contextWeb = sourceNode.Attribute("ContextWeb").Value;

                var site = new SPSite(contextWeb);
                var web = site.OpenWeb(contextWeb);

                var doProcess = ShouldContinue("Are you sure?", "Delete all result sources");
                if (doProcess)
                {
                    this.WriteWarning("Deleting result source:" + sourceName);
                    this._searchHelper.DeleteResultSource(searchServiceApp, sourceName, sortObjectLevel, web);
                }

                web.Dispose();
                site.Dispose();
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
