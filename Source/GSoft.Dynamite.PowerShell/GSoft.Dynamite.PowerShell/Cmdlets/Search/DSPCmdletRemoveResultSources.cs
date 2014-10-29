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
    /// Removes Search Result Sources configuration
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DSPResultSources")]
    public class DSPCmdletRemoveResultSources : SPCmdlet
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

            var serviceApplicationName = this.configurationFile.Root.Attribute("SearchServiceApplication").Value;
            var sourceNodes = from sourceNode in this.configurationFile.Descendants("Source") select sourceNode;

            foreach (var sourceNode in sourceNodes)
            {
                var sourceName = sourceNode.Attribute("Name").Value;
                var objectLevelAsString = sourceNode.Attribute("SearchObjectLevel").Value;

                var sortObjectLevel = (SearchObjectLevel)Enum.Parse(typeof(SearchObjectLevel), objectLevelAsString);
                var contextWeb = sourceNode.Attribute("ContextWeb").Value;

                using (var site = new SPSite(contextWeb))
                {
                    using (var web = site.OpenWeb(contextWeb))
                    {
                        using (var childScope = PowerShellContainer.BeginLifetimeScope(web))
                        {
                            var searchHelper = childScope.Resolve<ISearchHelper>();

                            var doProcess = ShouldContinue("Are you sure?", "Delete all result sources");
                            if (doProcess)
                            {
                                this.WriteWarning("Deleting result source:" + sourceName);
                                var searchServiceApp = searchHelper.GetDefaultSearchServiceApplication(serviceApplicationName);
                                searchHelper.DeleteResultSource(searchServiceApp, sourceName, sortObjectLevel, web);
                            }
                        }
                    }
                }
            }

            base.InternalEndProcessing();
        }
    }
}
