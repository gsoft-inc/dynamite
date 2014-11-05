using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Helpers;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Search;
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
    /// Catalog navigation context utility. Depends on HttpContext.
    /// </summary>
    public class CatalogNavigation : ICatalogNavigation
    {
        private const string LocalSharePointResultsSourceName = "Local SharePoint Results";
        private readonly ILogger logger;
        private readonly INavigationHelper navigationHelper;
        private readonly ISearchHelper searchHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogNavigation" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="navigationHelper">The navigation helper.</param>
        /// <param name="searchHelper">The search helper.</param>
        public CatalogNavigation(ILogger logger, INavigationHelper navigationHelper, ISearchHelper searchHelper)
        {
            this.logger = logger;
            this.navigationHelper = navigationHelper;
            this.searchHelper = searchHelper;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public CatalogNavigationType Type
        {
            get
            {
                if (TaxonomyNavigationContext.Current != null)
                {
                    if (TaxonomyNavigationContext.Current.HasCatalogUrl)
                    {
                        return CatalogNavigationType.ItemPage;
                    }

                    if (TaxonomyNavigationContext.Current.NavigationTerm != null)
                    {
                        return CatalogNavigationType.CategoryPage;
                    }
                }

                return CatalogNavigationType.None;
            }
        }

        /// <summary>
        /// Determines whether [is current item] [the specified item URL].
        /// </summary>
        /// <param name="itemUrl">The item URL.</param>
        /// <returns>True if URL is the current catalog item.</returns>
        public bool IsCurrentItem(Uri itemUrl)
        {
            var queryStrings = HttpUtility.ParseQueryString(HttpContext.Current.Request.Url.Query);
            var urlSuffix = string.Format(CultureInfo.InvariantCulture, "/{0}", queryStrings.Get("UrlSuffix"));

            return !string.IsNullOrEmpty(urlSuffix) && itemUrl.AbsolutePath.EndsWith(urlSuffix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the variation peer URL.
        /// </summary>
        /// <param name="label">The variation label.</param>
        /// <returns>
        /// The peer URL.
        /// </returns>
        public Uri GetVariationPeerUrl(VariationLabel label)
        {
            var cacheVariationLabel = new VariationLabelInfo(label);
            return this.GetVariationPeerUrl(cacheVariationLabel);
        }

        /// <summary>
        /// Gets the variation peer URL (for CategoryPage or undefined context).
        /// </summary>
        /// <param name="label">The variation label.</param>
        /// <returns>
        /// The peer URL.
        /// </returns>
        public Uri GetVariationPeerUrl(VariationLabelInfo label)
        {
            var currentUrl = HttpContext.Current.Request.Url;
            switch (this.Type)
            {
                case CatalogNavigationType.CategoryPage:
                    return this.GetPeerCatalogCategoryUrl(currentUrl, label);
                case CatalogNavigationType.ItemPage:
                    throw new InvalidOperationException("Missing information for a Catalog ItemPage context. Use GetVariationPeerUrlForCatalogItem instead.");
                default:
                    return this.GetPeerUrl(label, currentUrl);
            }
        }

        /// <summary>
        /// Gets the variation peer URL.
        /// </summary>
        /// <param name="label">The variation label.</param>
        /// <param name="associationKeyManagedPropertyName">Managed property name for association key between variation peer items</param>
        /// <param name="associationKeyValue">Value of the association key for the current item under variation</param>
        /// <param name="languageManagedPropertyName">Managed property name for the language discriminator column</param>
        /// <param name="catalogNavigationTermManagedPropertyName">Managed property name for the catalog navigation taxonomy column</param>
        /// <returns>
        /// The peer URL.
        /// </returns>
        public Uri GetVariationPeerUrlForCatalogItem(
            VariationLabelInfo label, 
            string associationKeyManagedPropertyName, 
            string associationKeyValue, 
            string languageManagedPropertyName, 
            string catalogNavigationTermManagedPropertyName)
        {
            var currentUrl = HttpContext.Current.Request.Url;
            switch (this.Type)
            {
                case CatalogNavigationType.CategoryPage:
                    // Ignore the extra ItemPage parameters (because we aren't in ItemPage context, after all)
                    return this.GetPeerCatalogCategoryUrl(currentUrl, label);
                case CatalogNavigationType.ItemPage:
                    return this.GetPeerCatalogItemUrl(currentUrl, label, associationKeyManagedPropertyName, associationKeyValue, languageManagedPropertyName, catalogNavigationTermManagedPropertyName);
                default:
                    // Ignore the extra ItemPage parameters (because we aren't in ItemPage context, after all)
                    return this.GetPeerUrl(label, currentUrl);
            }
        }

        private Uri GetPeerUrl(VariationLabelInfo label, Uri currentUrl)
        {
            if (currentUrl.LocalPath.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase))
            {
                return new Uri(SPUtility.ConcatUrls(label.TopWebUrl.ToString(), currentUrl.PathAndQuery));
            }
            else
            {
                try
                {
                    return new Uri(
                        Variations.GetPeerUrl(SPContext.Current.Web, currentUrl.AbsoluteUri, label.Title),
                        UriKind.Relative);
                }
                catch (ArgumentOutOfRangeException)
                {
                    this.logger.Info(@"GetPeerUrl: Cannot find variation peer URL with 'Variations.GetPeerUrl'.  
                                        Using label web URL with path and query strings as navigation URL.");

                    // Keep query string (except source)
                    var queryCollection = HttpUtility.ParseQueryString(currentUrl.Query);
                    queryCollection.Remove("Source");

                    // Construct peer URL with top web URL + path + query.
                    var topWebUrl = new Uri(label.TopWebUrl + "/");
                    var pathAndQuerySegments = new List<string>(topWebUrl.Segments.Concat(currentUrl.Segments.Skip(topWebUrl.Segments.Length)));

                    // If any query string, add to segments
                    if (queryCollection.HasKeys())
                    {
                        pathAndQuerySegments.Add(string.Format(CultureInfo.InvariantCulture, "?{0}", queryCollection));
                    }

                    return new Uri(topWebUrl, string.Join(string.Empty, pathAndQuerySegments));
                }
            }
        }

        private Uri GetPeerCatalogCategoryUrl(Uri currentUrl, VariationLabelInfo label)
        {
            // Get current navigation term ID
            var termId = TaxonomyNavigationContext.Current.NavigationTerm.Id;

            var labelSiteRelativeUrl = label.TopWebUrl.AbsolutePath;
            using (var labelWeb = SPContext.Current.Site.OpenWeb(labelSiteRelativeUrl))
            {
                // Create view to return all navigation terms
                var view = new NavigationTermSetView(labelWeb, StandardNavigationProviderNames.GlobalNavigationTaxonomyProvider)
                {
                    ExcludeTermsByProvider = false
                };

                var navigationTermSet =
                    TaxonomyNavigation.GetTermSetForWeb(labelWeb, StandardNavigationProviderNames.GlobalNavigationTaxonomyProvider, true).GetWithNewView(view);

                // Get the matching label navigation term and return it's friendly URL
                var navigationTerm = this.navigationHelper.GetNavigationTermById(navigationTermSet.Terms, termId);
                if (navigationTerm != null)
                {
                    this.logger.Info(
                        "GetPeerCatalogCategoryUrl: Navigation term found for term id '{0}': '{1}'",
                        termId,
                        navigationTerm.Title);

                    var queryString = string.Empty;

                    // Check if some search keywords are present
                    var searchKeywords = HttpUtility.ParseQueryString(currentUrl.Query).Get("k");

                    if (!string.IsNullOrEmpty(searchKeywords))
                    {
                        queryString = "?k=" + HttpUtility.UrlEncode(searchKeywords);
                    }

                    return new Uri(navigationTerm.GetResolvedDisplayUrl(queryString), UriKind.Relative);
                }
                else
                {
                    this.logger.Error("GetPeerCatalogCategoryUrl: Navigation term not found for term id '{0}'", termId);

                    return new Uri(
                        Variations.GetPeerUrl(SPContext.Current.Web, currentUrl.AbsoluteUri, label.Title),
                        UriKind.Relative);
                }
            }
        }

        private Uri GetPeerCatalogItemUrl(
            Uri currentUrl, 
            VariationLabelInfo label, 
            string associationKeyManagedPropertyName, 
            string associationKeyValue, 
            string languageManagedPropertyName, 
            string catalogNavigationTermManagedPropertyName)
        {
            ValidateProperties("GetPeerCatalogItemUrl", associationKeyManagedPropertyName, associationKeyValue, catalogNavigationTermManagedPropertyName);

            var url = new Uri(Variations.GetPeerUrl(SPContext.Current.Web, currentUrl.AbsoluteUri, label.Title), UriKind.Relative);

            var searchResultSource = this.searchHelper.GetResultSourceByName(LocalSharePointResultsSourceName, SPContext.Current.Site, SearchObjectLevel.Ssa);

            var labelLocaleAgnosticLanguage = label.Language.Split('-').First();
            var queryText = string.Format(
                CultureInfo.InvariantCulture, 
                "{0}:{1} {2}={3}", 
                associationKeyManagedPropertyName, 
                associationKeyValue, 
                languageManagedPropertyName, 
                labelLocaleAgnosticLanguage);

            var query = new KeywordQuery(SPContext.Current.Web)
            {
                SourceId = searchResultSource.Id,
                QueryText = queryText
            };

            // Search query must include the following properties for the friendly URL to work
            query.SelectProperties.AddRange(new[] { catalogNavigationTermManagedPropertyName, "Path", "spSiteUrl", "ListID" });
            var tables = new SearchExecutor().ExecuteQuery(query);
            if (tables.Exists(KnownTableTypes.RelevantResults))
            {
                var table = tables.Filter("TableType", KnownTableTypes.RelevantResults).Single(relevantTable => relevantTable.QueryRuleId == Guid.Empty);
                if (table != null && table.ResultRows.Count == 1 && table.Table.Columns.Contains("Path"))
                {
                    url = new Uri(table.Table.Rows[0]["Path"].ToString());
                }
            }

            return url;
        }

        private static void ValidateProperties(
            string callingMethodName,
            string associationKeyManagedPropertyName,
            string associationKeyValue,
            string catalogNavigationTermManagedPropertyName)
        {
            if (string.IsNullOrEmpty(associationKeyManagedPropertyName))
            {
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "{0}: Property '{1}' is null or empty string.", callingMethodName, "AssociationKeyManagedPropertyName"));
            }

            if (string.IsNullOrEmpty(associationKeyValue))
            {
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "{0}: Property '{1}' is null or empty string.", callingMethodName, "AssociationKeyValue"));
            }

            if (string.IsNullOrEmpty(catalogNavigationTermManagedPropertyName))
            {
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "{0}: Property '{1}' is null or empty string.", callingMethodName, "CatalogNavigationTermManagedPropertyName"));
            }
        }
    }
}
