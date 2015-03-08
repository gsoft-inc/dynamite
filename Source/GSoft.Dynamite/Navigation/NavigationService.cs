using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Search;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Navigation;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Service for main menu navigation nodes.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly ILogger logger;
        private readonly INavigationHelper navigationHelper;
        private readonly ISearchHelper searchHelper;
        private readonly IVariationNavigationHelper catalogNavigation;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="navigationHelper">The navigation helper.</param>
        /// <param name="searchHelper">The search helper.</param>
        /// <param name="catalogNavigation">The catalog navigation.</param>
        public NavigationService(ILogger logger, INavigationHelper navigationHelper, ISearchHelper searchHelper, IVariationNavigationHelper catalogNavigation)
        {
            this.logger = logger;
            this.navigationHelper = navigationHelper;
            this.searchHelper = searchHelper;
            this.catalogNavigation = catalogNavigation;
        }

        /// <summary>
        /// Gets all the navigation terms.
        /// </summary>
        /// <param name="web">The Current web</param>
        /// <param name="properties">The navigation properties</param>
        /// <returns>List of navigation node</returns>
        public IEnumerable<NavigationNode> GetAllNavigationNodes(SPWeb web, NavigationManagedProperties properties)
        {
            try
            {
                // Use the SPMonitored scope to 
                using (new SPMonitoredScope("GSoft.Dynamite.NavigationService::GetAllNavigationNodes"))
                {
                    // Create view to return all navigation terms
                    var view = new NavigationTermSetView(web, StandardNavigationProviderNames.GlobalNavigationTaxonomyProvider)
                    {
                        ExcludeTermsByProvider = false
                    };

                    IEnumerable<NavigationNode> items, terms, nodes;
                    var navigationTermSet = TaxonomyNavigation.GetTermSetForWeb(web, StandardNavigationProviderNames.GlobalNavigationTaxonomyProvider, true);

                    // Navigation termset might be null when crawling
                    if (navigationTermSet == null)
                    {
                        return new NavigationNode[] { };
                    }

                    navigationTermSet = navigationTermSet.GetWithNewView(view);

                    using (new SPMonitoredScope("GetNavigationNodeItems"))
                    {
                        // Get navigation items from search
                        items = this.GetNavigationNodeItems(properties, properties.CatalogItemContentTypeId, null).ToArray();

                        // If the cache contains corrupted data,
                        // clear it and fetch the data again
                        // If no items are returned, we do not make the query again since the items are not cached.
                        if (items == null)
                        {
                            items = this.GetNavigationNodeItems(properties);
                        }
                    }

                    using (new SPMonitoredScope("GetNavigationNodeTerms"))
                    {
                        // Get navigation terms from taxonomy
                        terms = this.GetNavigationNodeTerms(web, properties, navigationTermSet.Terms);

                        // If the cache contains corrupted data,
                        // clear it and fetch the data again
                        if ((terms == null) || !terms.Any())
                        {
                            terms = this.GetNavigationNodeTerms(web, properties, navigationTermSet.Terms);
                        }
                    }

                    using (new SPMonitoredScope("MapNavigationNodeTree"))
                    {
                        // Map navigation terms to node object, including search items
                        nodes = this.MapNavigationNodeTree(terms, items);
                    }

                    var nodesArray = nodes as NavigationNode[] ?? nodes.ToArray();
                    this.logger.Info("GetAllNavigationNodes: Found {0} navigation nodes in result source '{1}'.", nodesArray.Length, properties.ResultSourceName);
                    return nodesArray;
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("GetAllNavigationNodes: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the navigation node terms.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="properties">The Managed Properties</param>
        /// <param name="navigationTerms">The navigation terms.</param>
        /// <returns>A navigation node tree.</returns>
        private IEnumerable<NavigationNode> GetNavigationNodeTerms(SPWeb web, NavigationManagedProperties properties, IEnumerable<NavigationTerm> navigationTerms)
        {
            return this.GetNavigationNodeTerms(web, properties, navigationTerms, int.MaxValue);
        }

        /// <summary>
        /// Get the pages tagged with terms across the search service
        /// </summary>
        /// <param name="properties">The Managed Properties</param>
        /// <returns>Navigation node</returns>
        private IEnumerable<NavigationNode> GetNavigationNodeItems(NavigationManagedProperties properties)
        {
            return this.GetNavigationNodeItems(properties, SPContentTypeId.Empty, null);
        }
        
        /// <summary>
        /// Get the pages tagged with terms across the search service
        /// </summary>
        /// <param name="properties">The Managed Properties</param>
        /// <param name="filteredContentTypeId">The content type id</param>
        /// <param name="term">The current term</param>
        /// <returns>Navigation node</returns>
        private IEnumerable<NavigationNode> GetNavigationNodeItems(NavigationManagedProperties properties, SPContentTypeId filteredContentTypeId, string term)
        {
            // Use 'all menu items' result source for search query
            var searchResultSource = this.searchHelper.GetResultSourceByName(SPContext.Current.Site, properties.ResultSourceName, SearchObjectLevel.Ssa);
            
            // Check if find result source
            if (searchResultSource == null)
            {
                this.logger.Error("searchResultSource is null in GSoft.Dynamite.Navigation.NavigationService.GetNavigationNodeItems");
                return new List<NavigationNode>();
            }

            // Build query to return items in current variation label language
            var labelLocalAgnosticLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var query = new KeywordQuery(SPContext.Current.Web)
            {
                SourceId = searchResultSource.Id,
                QueryText = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", properties.ItemLanguage, labelLocalAgnosticLanguage),
                TrimDuplicates = false,
                RowLimit = 500
            };

            // Adds the filter on content type if the parameter is not empty.
            if (filteredContentTypeId != SPContentTypeId.Empty)
            {
                query.QueryText += string.Format(CultureInfo.InvariantCulture, " {0}:{1}", BuiltInManagedProperties.ContentTypeId, filteredContentTypeId + "*");
            }

            // Adds the filter on managed property OccurenceLinkLocation if the parameter is not null.
            if (!string.IsNullOrEmpty(properties.FilterManagedPropertyName) && !string.IsNullOrEmpty(properties.FilterManagedPropertyValue))
            {
                query.QueryText += string.Format(CultureInfo.InvariantCulture, " {0}:{1}", properties.FilterManagedPropertyName, properties.FilterManagedPropertyValue);
            }

            // Adds the filter current navigation Term if the parameter is not null.
            if (!string.IsNullOrEmpty(properties.Navigation) && !string.IsNullOrEmpty(term))
            {
                query.QueryText += string.Format(CultureInfo.InvariantCulture, " {0}:{1}", properties.Navigation, term);
            }

            query.SelectProperties.AddRange(new List<string>(properties.FriendlyUrlRequiredProperties) { properties.Title }.ToArray());

            if (properties.QueryProperties != null && properties.QueryProperties.Any())
            {
                query.SelectProperties.AddRange(properties.QueryProperties.ToArray());
            }

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

            return new List<NavigationNode>();
        }

        /// <summary>
        /// Gets all navigation node terms
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="properties">The navigation managed properties</param>
        /// <param name="navigationTerms">The navigation terms</param>
        /// <param name="maxLevel">the max level</param>
        /// <returns>The node terms</returns>
        private IEnumerable<NavigationNode> GetNavigationNodeTerms(SPWeb web, NavigationManagedProperties properties, IEnumerable<NavigationTerm> navigationTerms, int maxLevel)
        {
            // Navigation terms needs to be editable to get the taxonomy term
            var session = new TaxonomySession(web.Site);

            // Gets terms which are not excluded from global navigation
            var filteredTerms = navigationTerms.Where(
                x => !x.ExcludeFromGlobalNavigation && this.GetNavigationNodeItems(properties, properties.TargetItemContentTypeId, x.Title.ToString()).Any()).Select(x => x.GetAsEditable(session)).ToList();

            var terms = filteredTerms.Where(x => !x.ExcludeFromGlobalNavigation).ToArray();

            var nodes = filteredTerms.Select(x => new NavigationNode(x)).ToArray();

            if (maxLevel > 0)
            {
                for (var i = 0; i < terms.Length; i++)
                {
                    var term = terms[i];
                    var node = nodes[i];

                    // If term contains children, recurvise call
                    if (term.Terms.Count > 0)
                    {
                        node.ChildNodes = this.GetNavigationNodeTerms(web, properties, term.Terms, maxLevel - 1);
                    }
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
        private IEnumerable<NavigationNode> MapNavigationNodeTree(IEnumerable<NavigationNode> navigationTerms, IEnumerable<NavigationNode> navigationItems)
        {
            // Initialize current navigation term, current navigation branch terms, navigation items and navigation terms
            var currentTerm = TaxonomyNavigationContext.Current.NavigationTerm;
            var currentBranchTerms = this.navigationHelper.GetNavigationParentTerms(currentTerm).ToArray();
            var items = navigationItems == null ? new NavigationNode[] { } : navigationItems.ToArray();

            // Set branch properties for current navigation context
            var terms = navigationTerms.ToList();
            terms.ForEach(x => x.SetCurrentBranchProperties(currentTerm, currentBranchTerms));

            // For each term, map their child terms with recursive call
            for (var i = 0; i < terms.Count; i++)
            {
                var term = terms[i];
                var childNodes = new List<NavigationNode>();

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