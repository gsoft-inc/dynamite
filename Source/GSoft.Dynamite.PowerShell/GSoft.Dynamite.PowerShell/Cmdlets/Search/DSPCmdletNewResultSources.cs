using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Autofac;
using GSoft.Dynamite.Helpers;
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
                    using (var childScope = PowerShellContainer.BeginLifetimeScope(site.RootWeb))
                    {
                        var searchHelper = childScope.Resolve<ISearchHelper>();

                        var searchServiceApp = searchHelper.GetDefaultSearchServiceApplication(serviceApplicationName);

                        var sourceName = sourceNode.Attribute("Name").Value;
                        var objectLevelAsString = sourceNode.Attribute("SearchObjectLevel").Value;

                        // Get the search provider . Default is Local SharePoint Provider
                        var searchProvider = (sourceNode.Attribute("SearchProvider") == null) ? "Local SharePoint Provider" : sourceNode.Attribute("SearchProvider").Value;

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

                        var sortDirectionsAsString = sourceNode.Attribute("SortDirections").Value;
                        var sortFields = sourceNode.Attribute("SortFields").Value;

                        var query = sourceNode.Descendants("Query").Single().Value;

                        if (!string.IsNullOrEmpty(sortDirectionsAsString) && !string.IsNullOrEmpty(sortFields))
                        {
                            var delimiter = new[] { ',', ';', ' ' };
                            var fields = sortFields.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                            var directions = sortDirectionsAsString.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).Select(x => (SortDirection)Enum.Parse(typeof(SortDirection), x));

                            searchHelper.EnsureResultSource(
                                searchServiceApp,
                                sourceName,
                                sortObjectLevel,
                                searchProvider,
                                site.RootWeb,
                                query,
                                fields,
                                directions,
                                this.Overwrite);
                        }
                        else
                        {
                            searchHelper.EnsureResultSource(
                                searchServiceApp,
                                sourceName,
                                sortObjectLevel,
                                searchProvider,
                                site.RootWeb,
                                query,
                                null, 
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
