using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Practices.Unity;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.SharePoint;
using Microsoft.Office.Server.Search.Query;
using Microsoft.Office.Server.Search.Query.Rules;
using Microsoft.Office.Server.Search.Administration.Query;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Search
{
    /// <summary>
    /// Creates result sources in the search serviec application
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DSPResultSources")]
    public class DSPCmdletNewResultSources: Cmdlet
    {
         /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private SearchHelper _searchHelper;

        private XDocument _configurationFile;
        private bool _delete;
        private bool _overwrite;

        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The path to the file containing the result sources configuration or an XmlDocument object or XML string.",
            Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        [Parameter(HelpMessage = "Delete result sources configuration",
        Position = 2)]
        public SwitchParameter Delete
        {
            get { return _delete; }
            set { _delete = value; }
        }

        [Parameter(HelpMessage = "Specifies if result sources should be overwritten",
        Position = 3)]
        public SwitchParameter Overwrite
        {
            get { return _overwrite; }
            set { _overwrite = value; }
        }

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

                var sortObjectLevel = (SearchObjectLevel)Enum.Parse(typeof(SearchObjectLevel),objectLevelAsString);
                var contextWeb = sourceNode.Attribute("ContextWeb").Value;

                var spSite = new SPSite(contextWeb);
                var spWeb = spSite.OpenWeb(contextWeb);

                if(Delete)
                {
                    WriteWarning("Deleting result source:" + sourceName);
                    this._searchHelper.DeleteResultSource(searchServiceApp, sourceName, sortObjectLevel, spWeb);
                }
                else
                {
                    if(Overwrite)
                    {
                        WriteWarning("Overwrite specified. Deleting and recreating result source:" + sourceName);
                    }
                    else
                    {
                        WriteWarning("Creating result source:" + sourceName);
                    }
                    
                    var sortDirectionAsString = sourceNode.Attribute("SortDirection").Value;
                    var sortField = sourceNode.Attribute("SortField").Value;

                    var query = sourceNode.Descendants("Query").Single().Value;

                    if (!String.IsNullOrEmpty(sortDirectionAsString) && !String.IsNullOrEmpty(sortField))
                    {
                        var sortDirection = (SortDirection)Enum.Parse(typeof(SortDirection), sortDirectionAsString);
                        this._searchHelper.EnsureResultSource(searchServiceApp, sourceName, sortObjectLevel, spWeb, query, sortField, sortDirection, Overwrite);
                    }
                    else
                    {
                        this._searchHelper.EnsureResultSource(searchServiceApp, sourceName, sortObjectLevel, spWeb, query, null, Overwrite);
                    }
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
