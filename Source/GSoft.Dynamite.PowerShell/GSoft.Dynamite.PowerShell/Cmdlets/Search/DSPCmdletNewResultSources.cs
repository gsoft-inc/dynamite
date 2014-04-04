using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Utils;

using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;
using Microsoft.Practices.Unity;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Search
{
    /// <summary>
    /// Creates result sources in the search service application
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DSPResultSources")]

    // ReSharper disable once InconsistentNaming
    public class DSPCmdletNewResultSources : Cmdlet
    {
        /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private SearchHelper _searchHelper;

        private XDocument _configurationFile;

        private bool _overwrite;

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
        /// Gets or sets the overwrite.
        /// </summary>
        /// <value>
        /// The overwrite.
        /// </value>
        [Parameter(HelpMessage = "Specifies if result sources should be overwritten", Position = 3)]
        public SwitchParameter Overwrite
        {
            get
            {
                return this._overwrite;
            }

            set
            {
                this._overwrite = value;
            }
        }

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

                if (this.Overwrite)
                {
                    this.WriteWarning("Overwrite specified. Deleting and recreating result source:" + sourceName);
                    this._searchHelper.DeleteResultSource(searchServiceApp, sourceName, sortObjectLevel, web);
                }
                else
                {
                    this.WriteWarning("Creating result source:" + sourceName);
                }

                var sortDirectionAsString = sourceNode.Attribute("SortDirection").Value;
                var sortField = sourceNode.Attribute("SortField").Value;

                var query = sourceNode.Descendants("Query").Single().Value;

                if (!string.IsNullOrEmpty(sortDirectionAsString) && !string.IsNullOrEmpty(sortField))
                {
                    var sortDirection = (SortDirection)Enum.Parse(typeof(SortDirection), sortDirectionAsString);
                    this._searchHelper.EnsureResultSource(
                        searchServiceApp, 
                        sourceName, 
                        sortObjectLevel, 
                        web, 
                        query, 
                        sortField, 
                        sortDirection, 
                        this.Overwrite);
                }
                else
                {
                    this._searchHelper.EnsureResultSource(
                        searchServiceApp, 
                        sourceName, 
                        sortObjectLevel, 
                        web, 
                        query, 
                        null,
                        this.Overwrite);
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
