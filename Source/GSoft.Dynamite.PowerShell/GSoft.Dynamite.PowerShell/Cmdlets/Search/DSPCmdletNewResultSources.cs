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
using Microsoft.Office.Server.Search.Query;
using Microsoft.SharePoint;
using Microsoft.SharePoint.PowerShell;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Search
{
    /// <summary>
    /// Creates result sources in the search service application
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DSPResultSources")]

    // ReSharper disable once InconsistentNaming
    public class DSPCmdletNewResultSources : SPCmdlet
    {
        private XDocument configurationFile;
        private bool overwrite;

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
                return this.overwrite;
            }

            set
            {
                this.overwrite = value;
            }
        }

        /// <summary>
        /// Ends the processing.
        /// </summary>
        protected override void InternalEndProcessing()
        {
            var xml = this.InputFile.Read();
            this.configurationFile = xml.ToXDocument();

            var serviceApplicationName = this.configurationFile.Root.Attribute("SearchServiceApplication").Value;
            var sourceNodes = from sourceNode in this.configurationFile.Descendants("Source") select sourceNode;

            foreach (var sourceNode in sourceNodes)
            {
                var contextWeb = sourceNode.Attribute("ContextWeb").Value;

                using (var site = new SPSite(contextWeb))
                {
                    using (var childScope = PowerShellContainer.BeginWebLifetimeScope(site.RootWeb))
                    {
                        var searchHelper = childScope.Resolve<SearchHelper>();

                        var searchServiceApp = searchHelper.GetDefaultSearchServiceApplication(serviceApplicationName);

                        var sourceName = sourceNode.Attribute("Name").Value;
                        var objectLevelAsString = sourceNode.Attribute("SearchObjectLevel").Value;

                        var sortObjectLevel = (SearchObjectLevel)Enum.Parse(typeof(SearchObjectLevel), objectLevelAsString);

                        if (this.Overwrite)
                        {
                            this.WriteWarning("Overwrite specified. Deleting and recreating result source:" + sourceName);
                            searchHelper.DeleteResultSource(searchServiceApp, sourceName, sortObjectLevel, site.RootWeb);
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
                            searchHelper.EnsureResultSource(
                                searchServiceApp,
                                sourceName,
                                sortObjectLevel,
                                site.RootWeb,
                                query,
                                sortField,
                                sortDirection,
                                this.Overwrite);
                        }
                        else
                        {
                            searchHelper.EnsureResultSource(
                                searchServiceApp,
                                sourceName,
                                sortObjectLevel,
                                site.RootWeb,
                                query,
                                null,
                                this.Overwrite);
                        }
                    }
                }
            }

            base.InternalEndProcessing();
        }
    }
}
