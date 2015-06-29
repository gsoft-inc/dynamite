using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Search;
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
    public class VariationNavigationHelper : IVariationNavigationHelper
    {
        private const string LocalSharePointResultsSourceName = "Local SharePoint Results";
        private readonly ILogger logger;
        private readonly INavigationHelper navigationHelper;
        private readonly ISearchHelper searchHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariationNavigationHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="navigationHelper">The navigation helper.</param>
        /// <param name="searchHelper">The search helper.</param>
        public VariationNavigationHelper(ILogger logger, INavigationHelper navigationHelper, ISearchHelper searchHelper)
        {
            this.logger = logger;
            this.navigationHelper = navigationHelper;
            this.searchHelper = searchHelper;
        }

        /// <summary>
        /// Determine the current navigation context type
        /// </summary>
        /// <returns>The current variation navigation context</returns>
        public VariationNavigationType CurrentNavigationContextType
        {
            get
            {
                var navigationType = VariationNavigationType.None;

                if (TaxonomyNavigationContext.Current != null)
                {
                    if (TaxonomyNavigationContext.Current.HasCatalogUrl)
                    {
                        navigationType = VariationNavigationType.ItemPage;
                    }
                    else
                    {
                        if (TaxonomyNavigationContext.Current.NavigationTerm != null)
                        {
                            navigationType = VariationNavigationType.CategoryPage;
                        }
                    }
                }

                return navigationType;
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
        /// Get the peer url for a SharePoint page
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="currentUrl">The current page url</param>
        /// <param name="label">The target label to resolve</param>
        /// <returns>
        /// The url of the peer page
        /// </returns>
        public Uri GetPeerPageUrl(SPWeb web, Uri currentUrl, VariationLabelInfo label)
        {
            // Special case for application pages under /_layouts and site home page
            if (currentUrl.AbsolutePath.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase))
            {
                var relativePart = new Uri(currentUrl.PathAndQuery, UriKind.Relative);
                return new Uri(SPUtility.ConcatUrls(label.TopWebUrl.ToString(), relativePart.ToString()));
            }

            try
            {
                // Important: Use the server relative URL (absolute path) as the current URL parameter.
                // In the case where a load balancer is used, the server URL might be changed.  
                // Omit this problem by using the server relative URL.
                var peerPageUri = new Uri(Variations.GetPeerUrl(web, currentUrl.AbsolutePath, label.Title), UriKind.Relative);
                
                // Special case for home page
                if (SPContext.Current.ListItem != null
                && web.RootFolder.WelcomePage == SPContext.Current.ListItem.Url)
                {
                    var peerHomePageUrl = Regex.Replace(peerPageUri.OriginalString, @"\/Pages\/.*", string.Empty);
                    peerPageUri = new Uri(peerHomePageUrl, UriKind.Relative);
                }

                return peerPageUri;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // TODO: rewrite and unit test the following logic - I do not trust this logic for Managed Path scenarios.
                this.logger.Warn(
                    "GetPeerUrl: Cannot find variation peer URL with web '{0}', url '{1}' and label '{2}'. Exception message: '{3}'.",
                    web.Url,
                    currentUrl.AbsoluteUri,
                    label.Title,
                    ex.Message);

                // Keep query string (except source)
                var queryCollection = HttpUtility.ParseQueryString(currentUrl.Query);
                queryCollection.Remove("Source");

                if (queryCollection["RootFolder"] != null)
                {
                    string rootFolderParam = queryCollection["RootFolder"];
                    string currentServerWebRelativeUrl = web.RootFolder.ServerRelativeUrl;
                    rootFolderParam = rootFolderParam.Replace(currentServerWebRelativeUrl, label.TopWebUrl.AbsolutePath + "/");
                    queryCollection["RootFolder"] = rootFolderParam;
                }

                // Construct peer URL with top web URL + path + query.
                var topWebUrl = new Uri(label.TopWebUrl + "/");
                var pathAndQuerySegments = new List<string>(topWebUrl.Segments.Concat(currentUrl.Segments.Skip(topWebUrl.Segments.Length)));

                // If any query string, add to segments
                if (queryCollection.HasKeys())
                {
                    pathAndQuerySegments.Add(string.Format(CultureInfo.InvariantCulture, "?{0}", queryCollection));
                }

                return new Uri(topWebUrl, new Uri(string.Join(string.Empty, pathAndQuerySegments), UriKind.Relative));
            }
        }

        /// <summary>
        /// Get the peer url for a taxonomy navigation page (generated by a term set)
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="currentUrl">The current page url</param>
        /// <param name="label">The target label to resolve</param>
        /// <returns>
        /// The url of the peer page
        /// </returns>
        public Uri GetPeerCatalogCategoryUrl(SPWeb web, Uri currentUrl, VariationLabelInfo label)
        {
            // Get current navigation term ID
            var termId = TaxonomyNavigationContext.Current.NavigationTerm.Id;

            var labelSiteRelativeUrl = label.TopWebUrl.AbsolutePath;
            using (var labelWeb = web.Site.OpenWeb(labelSiteRelativeUrl))
            {
                // Create view to return all navigation terms
                var view = new NavigationTermSetView(labelWeb, StandardNavigationProviderNames.GlobalNavigationTaxonomyProvider)
                {
                    ExcludeTermsByProvider = false
                };

                var navigationTermSet =
                    TaxonomyNavigation.GetTermSetForWeb(labelWeb, StandardNavigationProviderNames.GlobalNavigationTaxonomyProvider, true).GetWithNewView(view);

                // Get the matching label navigation term and return it's friendly URL
                var navigationTerm = this.navigationHelper.FindNavigationTermById(navigationTermSet.Terms, termId);
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
                        Variations.GetPeerUrl(web, currentUrl.AbsoluteUri, label.Title),
                        UriKind.Relative);
                }
            }
        }

        /// <summary>
        /// Get the peer url for a page represents a cross site publishing catalog item 
        /// </summary>
        /// <param name="currentUrl">The current page url</param>
        /// <param name="label">The target label to resolve</param>
        /// <param name="associationKeyManagedPropertyName">The content association key search managed property name</param>
        /// <param name="associationKeyValue">The value of the content association key for the current item</param>
        /// <param name="languageManagedPropertyName">The language search managed property name</param>
        /// <param name="catalogNavigationTermManagedPropertyName">The navigation search managed property name used for the friendly url generation</param>
        /// <returns>The url of the peer page</returns>
        public Uri GetPeerCatalogItemUrl(
            Uri currentUrl, 
            VariationLabelInfo label, 
            string associationKeyManagedPropertyName, 
            string associationKeyValue, 
            string languageManagedPropertyName, 
            string catalogNavigationTermManagedPropertyName)
        {
            ValidateProperties("GetPeerCatalogItemUrl", associationKeyManagedPropertyName, associationKeyValue, catalogNavigationTermManagedPropertyName);

            var url = new Uri(Variations.GetPeerUrl(SPContext.Current.Web, currentUrl.AbsoluteUri, label.Title), UriKind.Relative);

            var searchResultSource = this.searchHelper.GetResultSourceByName(SPContext.Current.Site, LocalSharePointResultsSourceName, SearchObjectLevel.Ssa);

            var queryText = string.Format(
                CultureInfo.InvariantCulture,
                "{0}:{1} {2}:{3}",
                associationKeyManagedPropertyName,
                associationKeyValue,
                languageManagedPropertyName,
                label.Language);

            var query = new KeywordQuery(SPContext.Current.Web)
            {
                SourceId = searchResultSource.Id,
                QueryText = queryText
            };

            // Search query must include the following properties for the friendly URL to work
            query.SelectProperties.AddRange(new[] 
            { 
                catalogNavigationTermManagedPropertyName,
                BuiltInManagedProperties.Url.Name, 
                BuiltInManagedProperties.SiteUrl.Name, 
                BuiltInManagedProperties.ListId.Name 
            });
            var tables = new SearchExecutor().ExecuteQuery(query);
            if (tables.Exists(KnownTableTypes.RelevantResults))
            {
                var table = tables.Filter("TableType", KnownTableTypes.RelevantResults).Single(relevantTable => relevantTable.QueryRuleId == Guid.Empty);
                if (table != null && table.ResultRows.Count == 1 && table.Table.Columns.Contains(BuiltInManagedProperties.Url.Name))
                {
                    url = new Uri(table.Table.Rows[0][BuiltInManagedProperties.Url.Name].ToString(), UriKind.Absolute);

                    // Convert the absolute Uri into a relative Uri. This is required for environments using a load balancer,
                    // because we need to ignore the load balancer's port found in the absolute Uri.
                    url = new Uri(url.AbsolutePath, UriKind.Relative);
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
