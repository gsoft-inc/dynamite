using System;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Autofac;
using GSoft.Dynamite.Helpers;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Search;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.SharePoint;
using Microsoft.SharePoint.PowerShell;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Search
{
    /// <summary>
    /// Creates result sources in the search service application
    /// </summary>
    [Cmdlet(VerbsCommon.New, "DSPQueryRules")]
    public class DSPCmdletNewQueryRules : SPCmdlet
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
                    var contextWeb = queryRuleNode.Attribute("ContextWeb").Value;
                    using (var site = new SPSite(contextWeb))
                    {
                        using (var web = site.OpenWeb())
                        {
                            using (var childScope = PowerShellContainer.BeginLifetimeScope(web))
                            {
                                var searchHelper = childScope.Resolve<ISearchHelper>();
                                var taxonomyService = childScope.Resolve<ITaxonomyService>();

                                var searchServiceApp = searchHelper.GetDefaultSearchServiceApplication(serviceApplicationName);

                                var displayName = queryRuleNode.Attribute("DisplayName").Value;
                                var objectLevelAsString = queryRuleNode.Attribute("SearchObjectLevel").Value;

                                var startDateAsString = queryRuleNode.Attribute("StartDate") != null ? queryRuleNode.Attribute("StartDate").Value : null;
                                var endDateAsString = queryRuleNode.Attribute("EndDate") != null ? queryRuleNode.Attribute("EndDate").Value : null;
                                var isActive = queryRuleNode.Attribute("IsActive") != null ? queryRuleNode.Attribute("IsActive").Value : "true";
                                DateTime? startDate = null;
                                DateTime? endDate = null;

                                if (!string.IsNullOrEmpty(startDateAsString))
                                {
                                    startDate = DateTime.Parse(startDateAsString).ToUniversalTime();
                                }

                                if (!string.IsNullOrEmpty(endDateAsString))
                                {
                                    endDate = DateTime.Parse(endDateAsString).ToUniversalTime();
                                }

                                var searchObjectLevel = (SearchObjectLevel)Enum.Parse(typeof(SearchObjectLevel), objectLevelAsString);

                                var queryRules = searchHelper.GetQueryRulesByName(searchServiceApp, searchObjectLevel, web, displayName);

                                if (queryRules.Any())
                                {
                                    this.WriteWarning("Query rule already exists! Deleting and recreating query rule:" + displayName);
                                    searchHelper.DeleteQueryRule(searchServiceApp, searchObjectLevel, web, displayName);
                                }

                                this.WriteWarning("Creating query rule:" + displayName);
                                var queryRule = searchHelper.CreateQueryRule(searchServiceApp, searchObjectLevel, web, displayName, bool.Parse(isActive), startDate, endDate);

                                // Process Category Conditions
                                var terms = queryRuleNode.Descendants("ContextConditions").Single().Descendants("Categories").Descendants("Term");
                                if (terms != null)
                                {
                                    var termConditions = from termNode in terms select termNode;
                                    foreach (var condition in termConditions)
                                    {
                                        Term term = null;
                                        var termLabel = condition.Value;
                                        var termSet = condition.Attribute("TermSet").Value;
                                        var termGroup = condition.Attribute("TermGroup").Value;
                                        var termId = condition.Attribute("TermId") != null ? condition.Attribute("TermId").Value : null;

                                        if (string.IsNullOrEmpty(termId))
                                        {
                                            // Retrieve term by label
                                            term = taxonomyService.GetTermForLabel(site, termGroup, termSet, termLabel);
                                        }
                                        else
                                        {
                                            // Retrieve term by Id
                                            var guid = Guid.Parse(termId);
                                            term = taxonomyService.GetTermForIdInTermSet(site, termGroup, termSet, guid);
                                        }

                                        if (term != null)
                                        {
                                            // Add the category condition 
                                            queryRule.CreateCategoryContextCondition(term);
                                            queryRule.Update();
                                        }
                                        else
                                        {
                                            this.WriteWarning("Term '" + termLabel + "' not found!");
                                        }
                                    }
                                }

                                // Process Result Sources
                                var sources = queryRuleNode.Descendants("ContextConditions").Single().Descendants("ResultSources").Descendants("SourceName");
                                if (sources != null)
                                {
                                    var sourceConditions = from sourceNode in sources select sourceNode;
                                    foreach (var condition in sourceConditions)
                                    {
                                        var sourceName = condition.Value;
                                        var source = searchHelper.GetResultSourceByName(searchServiceApp, sourceName, searchObjectLevel, web);

                                        if (source != null)
                                        {
                                            // Add the result source condition 
                                            queryRule.CreateSourceContextCondition(source);
                                            queryRule.Update();
                                        }
                                        else
                                        {
                                            this.WriteWarning("The specified source '" + sourceName + "' for the query rule source condition doesn't exists");
                                        }
                                    }
                                }

                                // Process Change Query Actions
                                var changeQueryActions = queryRuleNode.Descendants("QueryActions").Single().Descendants("ChangeQueryActions").Descendants("Action");
                                if (changeQueryActions != null)
                                {
                                    var actions = from changeQueryAction in changeQueryActions select changeQueryAction;
                                    foreach (var action in actions)
                                    {
                                        var sourceName = action.Descendants("SourceName").Single().Value;
                                        var source = searchHelper.GetResultSourceByName(searchServiceApp, sourceName, searchObjectLevel, web);

                                        var queryTemplate = action.Descendants("QueryTemplate").Single().Value;

                                        if (source != null)
                                        {
                                            // Add the action
                                            searchHelper.CreateChangeQueryAction(queryRule, queryTemplate, source.Id);
                                        }
                                        else
                                        {
                                            this.WriteWarning("The specified source' " + sourceName + "' for the change query action doesn't exists");
                                        }
                                    }
                                }

                                // Process Result Block Actions
                                var resultBLockActions = queryRuleNode.Descendants("QueryActions").Single().Descendants("ResultBlockActions").Descendants("Action");
                                if (resultBLockActions != null)
                                {
                                    var actions = from resultBlockQueryAction in resultBLockActions select resultBlockQueryAction;
                                    foreach (var action in actions)
                                    {
                                        var blockTitle = action.Descendants("BlockTitle").Single().Value;
                                        var sourceName = action.Descendants("SourceName").Single().Value;
                                        var source = searchHelper.GetResultSourceByName(searchServiceApp, sourceName, searchObjectLevel, web);

                                        var routingLabel = action.Descendants("RountingLabel").Single() != null ? action.Descendants("RountingLabel").Single().Value : null;
                                        var numberOfItems = action.Descendants("NumberOfItems").Single() != null ? action.Descendants("NumberOfItems").Single().Value : null;

                                        var queryTemplate = action.Descendants("QueryTemplate").Single().Value;

                                        if (source != null)
                                        {
                                            // Add the action
                                            searchHelper.CreateResultBlockAction(queryRule, blockTitle, queryTemplate, source.Id, routingLabel, numberOfItems);
                                        }
                                        else
                                        {
                                            this.WriteWarning("The specified source' " + sourceName + "' for the change query action doesn't exists");
                                        }
                                    }
                                }

                                // Prcoess Promoted Link Action
                                var promotedResultActions = queryRuleNode.Descendants("QueryActions").Single().Descendants("PromotedResultActions").Descendants("Action");
                                if (promotedResultActions != null)
                                {
                                    var actions = from promoetedResultQueryAction in promotedResultActions select promoetedResultQueryAction;
                                    foreach (var action in actions)
                                    {
                                        var linkTitle = action.Descendants("LinkTitle").Single() != null ? action.Descendants("LinkTitle").Single().Value : "NoName";
                                        var linkUrl = action.Descendants("LinkUrl").Single().Value;
                                        var linkDescription = action.Descendants("LinkDescription").Single() != null ? action.Descendants("LinkDescription").Single().Value : string.Empty;

                                        var isVisualBestBet = action.Descendants("IsVisualBestBet").Single() != null && bool.Parse(action.Descendants("IsVisualBestBet").Single().Value);
                                        var deleteIfUnused = action.Descendants("DeleteIfUnused").Single() != null && bool.Parse(action.Descendants("DeleteIfUnused").Single().Value);

                                        if (!string.IsNullOrEmpty(linkUrl))
                                        {
                                            var url = new Uri(linkUrl);

                                            // Add the action
                                            var bestBet = searchHelper.EnsureBestBet(
                                                searchServiceApp,
                                                searchObjectLevel,
                                                web,
                                                linkTitle,
                                                url,
                                                linkDescription,
                                                isVisualBestBet,
                                                deleteIfUnused);

                                            searchHelper.CreatePromotedResultAction(queryRule, bestBet.Id);
                                        }
                                    }
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
