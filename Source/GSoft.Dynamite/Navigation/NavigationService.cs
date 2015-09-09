using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using GSoft.Dynamite.Extensions;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Search;
using GSoft.Dynamite.Taxonomy;
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
        private readonly ITaxonomyHelper taxonomyHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="navigationHelper">The navigation helper.</param>
        /// <param name="searchHelper">The search helper.</param>
        /// <param name="catalogNavigation">The catalog navigation.</param>
        /// <param name="taxonomyHelper">The taxonomy helper.</param>
        public NavigationService(
            ILogger logger, 
            INavigationHelper navigationHelper, 
            ISearchHelper searchHelper, 
            IVariationNavigationHelper catalogNavigation,
            ITaxonomyHelper taxonomyHelper)
        {
            this.logger = logger;
            this.navigationHelper = navigationHelper;
            this.searchHelper = searchHelper;
            this.catalogNavigation = catalogNavigation;
            this.taxonomyHelper = taxonomyHelper;
        }

        /// <summary>
        /// Gets all the navigation terms.
        /// </summary>
        /// <param name="web">The Current web</param>
        /// <param name="queryParameters">The navigation query parameters.</param>
        /// <returns>
        /// List of navigation node.
        /// </returns>
        public IEnumerable<NavigationNode> GetAllNavigationNodes(SPWeb web, NavigationQueryParameters queryParameters)
        {
            try
            {
                // Use the monitored scope to trace the execution time
                using (new SPMonitoredScope("GSoft.Dynamite.NavigationService::GetAllNavigationNodes"))
                {
                    // Get navigation terms from taxonomy
                    var navigationNodes = this.GetGlobalNavigationTaxonomyNodes(web, queryParameters);

                    // Make sure the nodes are not null
                    if (navigationNodes.Any(node => node != null))
                    {
                        // If specified, filter to the restricted term set
                        if (!queryParameters.RestrictedTermSetId.Equals(Guid.Empty))
                        {
                            navigationNodes = this.FilterNavigationNodesToRestrictedTermSet(web, queryParameters, navigationNodes);
                        }

                        // If match settings are defined
                        if (queryParameters.NodeMatchingSettings != null)
                        {
                            // If specified, filter the navigation nodes to only the ones who are reacheable
                            // (i.e.) The nodes that have a target item search result
                            if (queryParameters.NodeMatchingSettings.RestrictToReachableTargetItems)
                            {
                                var targetItemNodes = this.GetTargetItemNavigationNodes(web, queryParameters, navigationNodes);
                                navigationNodes = this.FilterNavigationNodesToReacheableTargetItems(navigationNodes, targetItemNodes);
                            }

                            // If specified, include the catalog items from the search
                            if (queryParameters.NodeMatchingSettings.IncludeCatalogItems)
                            {
                                // Get catalog items from search
                                var catalogItemNavigationNodes = this.GetCatalogItemNavigationNodes(web, queryParameters);

                                // Map navigation terms to node object, including search items
                                navigationNodes = this.MapNavigationNodeTree(navigationNodes, catalogItemNavigationNodes);
                            }
                        }

                        this.logger.Info("GetAllNavigationNodes: Found {0} navigation nodes (including children).", navigationNodes.Flatten(n => n.ChildNodes).Count()); 
                    }

                    return navigationNodes;
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("GetAllNavigationNodes: {0}", ex.Message);
                throw;
            }
        }

        private IEnumerable<NavigationNode> GetGlobalNavigationTaxonomyNodes(SPWeb web, NavigationQueryParameters queryParameters, IEnumerable<NavigationTerm> navigationTerms = null)
        {
            // If navigation terms is null, fetch this initial terms from the taxonomy navigation term set
            if (navigationTerms == null)
            {
                var nodeMatchingSettings = queryParameters.NodeMatchingSettings;
                if ((nodeMatchingSettings != null) && nodeMatchingSettings.RestrictToCurrentNavigationLevel)
                {
                    navigationTerms = TaxonomyNavigationContext.Current.NavigationTerm.Parent.Terms;
                }
                else
                {
                    // Create view to return all navigation terms
                    var view = new NavigationTermSetView(web, StandardNavigationProviderNames.GlobalNavigationTaxonomyProvider)
                    {
                        ExcludeTermsByProvider = false
                    };

                    var navigationTermSet = TaxonomyNavigation.GetTermSetForWeb(web, StandardNavigationProviderNames.GlobalNavigationTaxonomyProvider, true);

                    // Navigation termset might be null when crawling
                    if (navigationTermSet == null)
                    {
                        return new NavigationNode[] { };
                    }

                    navigationTerms = navigationTermSet.GetWithNewView(view).Terms;
                }
            }

            // Gets terms which are not excluded from global navigation
            // Note: Navigation terms needs to be editable to get the taxonomy term
            var session = new TaxonomySession(web.Site);
            var terms = navigationTerms.Where(x => !x.ExcludeFromGlobalNavigation).Select(x => x.GetAsEditable(session)).ToArray();
            var nodes = terms.Select(x => new NavigationNode(x)).ToArray();
            for (var i = 0; i < terms.Length; i++)
            {
                var term = terms[i];
                var node = nodes[i];

                // If term contains children, recurvise call
                if (term.Terms.Count > 0)
                {
                    node.ChildNodes = this.GetGlobalNavigationTaxonomyNodes(web, queryParameters, term.Terms);
                }
            }

            return nodes;
        }

        private IEnumerable<NavigationNode> FilterNavigationNodesToRestrictedTermSet(SPWeb web, NavigationQueryParameters queryParameters, IEnumerable<NavigationNode> nodes, Term[] restrictedTerms = null)
        {
            // If first pass, initialize the restricted nodes with first level terms
            if (restrictedTerms == null)
            {
                // Get restricted term set
                var session = new TaxonomySession(web.Site);
                TermStore termStore = null;

                if (queryParameters.TermStoreId == Guid.Empty)
                {
                    termStore = this.taxonomyHelper.GetDefaultSiteCollectionTermStore(session);
                }
                else
                {
                    termStore = session.TermStores[queryParameters.TermStoreId];
                }

                var termSet = termStore.GetTermSet(queryParameters.RestrictedTermSetId);

                var nodeMatchingSettings = queryParameters.NodeMatchingSettings;
                if ((nodeMatchingSettings != null) && nodeMatchingSettings.RestrictToCurrentNavigationLevel)
                {
                    var currentTermId = TaxonomyNavigationContext.Current.NavigationTerm.Id;
                    restrictedTerms = termSet.GetTerm(currentTermId).Parent.Terms.ToArray();
                }
                else
                {
                    restrictedTerms = termSet.Terms.ToArray();
                }
            }

            // Flattened navigation nodes for easier search
            var flattenedNodes = nodes.Flatten(node => node.ChildNodes);
            var restrictedNodes = restrictedTerms.Select(term => flattenedNodes.SingleOrDefault(node => node.Id.Equals(term.Id))).ToArray();
            for (var i = 0; i < restrictedTerms.Length; i++)
            {
                var restrictedTerm = restrictedTerms[i];
                var restrictedNode = restrictedNodes[i];

                // If term contains children, recurvise call
                if (restrictedTerm.Terms.Count > 0)
                {
                    restrictedNode.ChildNodes = this.FilterNavigationNodesToRestrictedTermSet(web, queryParameters, nodes, restrictedTerm.Terms.ToArray());
                }
                else
                {
                    restrictedNode.ChildNodes = new List<NavigationNode>();
                }
            }

            return restrictedNodes;
        }

        private IEnumerable<NavigationNode> FilterNavigationNodesToReacheableTargetItems(IEnumerable<NavigationNode> nodes, IEnumerable<NavigationNode> targetItems)
        {
            // Only keep navigation terms that are included in the target item nodes
            // Note: Target item id's are tagged with the parent node id
            var filteredNodes = nodes.Where(node => targetItems.Any(target => target.ParentNodeId.Equals(node.Id))).ToArray();
            foreach (var filteredNode in filteredNodes)
            {
                var filteredNodeChildren = filteredNode.ChildNodes.ToArray();

                // If term contains children, recurvise call
                if (filteredNodeChildren.Length > 0)
                {
                    filteredNode.ChildNodes = this.FilterNavigationNodesToReacheableTargetItems(filteredNodeChildren, targetItems);
                }
            }

            return filteredNodes;
        }

        private IEnumerable<NavigationNode> GetCatalogItemNavigationNodes(SPWeb web, NavigationQueryParameters queryParameters)
        {
            return this.GetNavigationNodesBySearch(web, queryParameters.SearchSettings, queryParameters.SearchSettings.CatalogItemFilters);
        }

        private IEnumerable<NavigationNode> GetTargetItemNavigationNodes(SPWeb web, NavigationQueryParameters queryParameters, IEnumerable<NavigationNode> nodes)
        {
            // Adds the filter for each first level navigation term id
            var targetItemFilters = new List<string>();
            var additionalFilters = new List<string>(queryParameters.SearchSettings.TargetItemFilters ?? new string[] { });
            foreach (var node in nodes)
            {
                targetItemFilters.Add(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}:#{1:D}",
                        queryParameters.SearchSettings.NavigationManagedPropertyName, 
                        node.Id));
            }

            if (targetItemFilters.Count > 0)
            {
                var filterAsOrStatement = string.Format(
                    CultureInfo.InvariantCulture,
                    "({0})",
                    string.Join(" OR ", targetItemFilters));

                additionalFilters.Add(filterAsOrStatement); 
            }

            return this.GetNavigationNodesBySearch(web, queryParameters.SearchSettings, additionalFilters);
        }

        private IEnumerable<NavigationNode> GetNavigationNodesBySearch(SPWeb web, NavigationSearchSettings settings, IEnumerable<string> additionalFilters = null)
        {
            var filters = new List<string>(settings.GlobalFilters);

            // Check if find result source
            var searchResultSource = this.searchHelper.GetResultSourceByName(web.Site, settings.ResultSourceName, SearchObjectLevel.Ssa);
            if (searchResultSource == null)
            {
                this.logger.Error("searchResultSource is null in GSoft.Dynamite.Navigation.NavigationService.GetNavigationNodeItems");
                return new List<NavigationNode>();
            }

            var query = new KeywordQuery(web)
            {
                SourceId = searchResultSource.Id,
                TrimDuplicates = false,
                RowLimit = 500
            };

            // Add defined filters
            if (additionalFilters != null)
            {
                filters.AddRange(additionalFilters);
            }

            if (settings.SelectedProperties != null && settings.SelectedProperties.Any())
            {
                query.SelectProperties.AddRange(settings.SelectedProperties.ToArray());
            }

            // TODO: For now, the filters are applied seperated by whitespaces which means "AND" in KQL land.
            // TODO: We should figure out a way to make this more flexible to use "OR" if necessary.
            query.QueryText = string.Join(" ", filters.Where(filter => !string.IsNullOrEmpty(filter)));

            // Execute search query
            var tables = new SearchExecutor().ExecuteQuery(query);
            if (tables.Exists(KnownTableTypes.RelevantResults))
            {
                // Build navigation nodes for search results
                var results = tables.Filter("TableType", KnownTableTypes.RelevantResults).Single(relevantTable => relevantTable.QueryRuleId == Guid.Empty);
                var nodes = results.Table.Rows.Cast<DataRow>().Select(dataRow => new NavigationNode(dataRow, settings.NavigationManagedPropertyName));
                this.logger.Info(
                    "GetNavigationNodeItems: Found {0} items with search query '{1}' from source '{2}'.",
                    results.Table.Rows.Count,
                    query.QueryText,
                    settings.ResultSourceName);

                return nodes;
            }

            this.logger.Error(
                "GetNavigationNodeItems: No relevant results table found with search query '{0}' from source '{1}'.",
                query.QueryText,
                settings.ResultSourceName);

            return new List<NavigationNode>();
        }

        private IEnumerable<NavigationNode> MapNavigationNodeTree(IEnumerable<NavigationNode> navigationTerms, IEnumerable<NavigationNode> navigationItems)
        {
            var terms = navigationTerms.ToList();
            var items = navigationItems == null ? new NavigationNode[] { } : navigationItems.ToArray();
            NavigationTerm[] currentBranchTerms = null;

            if (TaxonomyNavigationContext.Current != null)
            {
                // Set branch properties for current navigation context
                var currentTerm = TaxonomyNavigationContext.Current.NavigationTerm;
                currentBranchTerms = this.navigationHelper.GetNavigationParentTerms(currentTerm).ToArray();
                terms.ForEach(x => x.SetCurrentBranchProperties(currentTerm, currentBranchTerms)); 
            }

            // For each term, map their child terms with recursive call
            for (var i = 0; i < terms.Count; i++)
            {
                var term = terms[i];
                var childNodes = new List<NavigationNode>();

                // If search item found, add it to child items
                var matchingItems = items.Where(x => x.ParentNodeId.Equals(term.Id));
                foreach (var matchingItem in matchingItems)
                {
                    if (currentBranchTerms != null)
                    {
                        // Item is only in current branch it's the current item
                        if (this.catalogNavigation.IsCurrentItem(matchingItem.Url))
                        {
                            matchingItem.IsNodeInCurrentBranch = currentBranchTerms.Any(y => y.Id.Equals(term.Id));
                        } 
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