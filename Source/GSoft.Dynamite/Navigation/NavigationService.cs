using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GSoft.Dynamite.Caching;
using GSoft.Dynamite.Helpers;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Utils;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Publishing.Navigation;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Navigation
{
      /// <summary>
    /// Service for main menu navigation nodes.
    /// </summary>
    public class NavigationService
    {
        private readonly ILogger logger;
        private readonly NavigationHelper navigationHelper;
        private readonly SearchHelper searchHelper;
        private readonly ICatalogNavigation catalogNavigation;

          /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="navigationHelper">The navigation helper.</param>
        /// <param name="searchHelper">The search helper.</param>
        /// <param name="catalogNavigation">The catalog navigation.</param>
        public NavigationService(ILogger logger, NavigationHelper navigationHelper, SearchHelper searchHelper, ICatalogNavigation catalogNavigation)
        {
            this.logger = logger;
            this.navigationHelper = navigationHelper;
            this.searchHelper = searchHelper;
            this.catalogNavigation = catalogNavigation;
        }
        
        /// <summary>
        /// Get the pages tagged with terms across the search service
        /// </summary>
        /// <param name="properties">The Managed Properties</param>
        /// <returns>Navigation node</returns>
        public IEnumerable<INavigationNode> GetNavigationNodeItems(NavigationManagedProperties properties)
        {
            // Use 'all menu items' result source for search query
            var searchResultSource = this.searchHelper.GetResultSourceByName(properties.ResultSourceName, SPContext.Current.Site, SearchObjectLevel.Ssa);

            // Build query to return items in current variation label language
            var currentLabel = PublishingWeb.GetPublishingWeb(SPContext.Current.Web).Label;
            var labelLocalAgnosticLanguage = currentLabel.Language.Split('-').First();
            var query = new KeywordQuery(SPContext.Current.Web)
            {
                SourceId = searchResultSource.Id,
                QueryText = string.Format("{0}:{1}", properties.ItemLanguage, labelLocalAgnosticLanguage),
                TrimDuplicates = false
            };

            query.SelectProperties.AddRange(new List<string>(properties.FriendlyUrlRequiredProperties) { properties.Title }.ToArray());

            var tables = new SearchExecutor().ExecuteQuery(query);
            if (tables.Exists(KnownTableTypes.RelevantResults))
            {
                // Build navigation nodes for search results
                var results = tables.Filter("TableType", KnownTableTypes.RelevantResults).Single(relevantTable => relevantTable.QueryRuleId == Guid.Empty);
                var nodes = results.Table.Rows.Cast<DataRow>().Select(x => new NavigationNode(x, properties.Navigation));
                this.logger.Info(
                    "GetNavigationNodeItems: Found {0} items with search query '{1}' from source '{2}'.",
                    results.Table.Rows.Count, 
                    query.QueryText,
                    properties.ResultSourceName);

                return nodes;
            }

            this.logger.Error(
                "GetNavigationNodeItems: No relevant results table found with search query '{0}' from source '{1}'.", 
                query.QueryText,
                properties.ResultSourceName);

            return new List<INavigationNode>();
        }

        /// <summary>
        /// Get all navigation node terms
        /// </summary>
        /// <param name="navigationTerms">Navigation terms</param>
        /// <returns>navigation node terms</returns>
        public IEnumerable<INavigationNode> GetNavigationNodeTerms(IEnumerable<NavigationTerm> navigationTerms)
        {
            var terms = navigationTerms as NavigationTerm[] ?? navigationTerms.Where(x => !x.ExcludeFromGlobalNavigation).ToArray();
            var nodes = terms.Select(x => new NavigationNode(x)).ToArray();

            for (var i = 0; i < terms.Length; i++)
            {
                var term = terms[i];
                var node = nodes[i];

                // If term contains children, recurvise call
                if (term.Terms.Count > 0)
                {
                    node.ChildNodes = this.GetNavigationNodeTerms(term.Terms);
                }
            }

            return nodes;
        }

        /// <summary>
        /// Map nodes with items
        /// </summary>
        /// <param name="navigationTerms">Navigation terms</param>
        /// <param name="navigationItems">Navigation Items</param>
        /// <returns>Navigation nodes</returns>
        public IEnumerable<INavigationNode> MapNavigationNodeTree(IEnumerable<INavigationNode> navigationTerms, IEnumerable<INavigationNode> navigationItems)
        {
            // Initialize current navigation term, current navigation branch terms, navigation items and navigation terms
            var currentTerm = TaxonomyNavigationContext.Current.NavigationTerm;
            var currentBranchTerms = this.navigationHelper.GetNavigationParentTerms(currentTerm).ToArray();
            var items = navigationItems == null ? new INavigationNode[] { } : navigationItems.ToArray();
            
            // Set branch properties for current navigation context
            var terms = navigationTerms.ToList();
            terms.ForEach(x => x.SetCurrentBranchProperties(currentTerm, currentBranchTerms));

            // For each term, map their child terms with recursive call
            for (var i = 0; i < terms.Count; i++)
            {
                var term = terms[i];
                var childNodes = new List<INavigationNode>();

                // If search item found, add it to child items
                var matchingItems = items.Where(x => x.ParentNodeId.Equals(term.Id));
                foreach (var matchingItem in matchingItems)
                {
                    // Item is only in current branch it's the current item
                    if (this.catalogNavigation.IsCurrentItem(matchingItem.Url))
                    {
                        matchingItem.IsNodeInCurrentBranch = currentBranchTerms.Any(y => y.Id.Equals(term.Id));
                    }

                    childNodes.Add(matchingItem);
                } 

                // If term contains children, recurvise call
                if (term.ChildNodes != null && term.ChildNodes.Any())
                {
                    childNodes.AddRange(this.MapNavigationNodeTree(term.ChildNodes, items));
                }

                term.ChildNodes = childNodes;
            }

            return terms;
        }
    }
}