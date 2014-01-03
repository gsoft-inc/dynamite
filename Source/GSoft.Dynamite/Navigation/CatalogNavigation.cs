using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    /// Catalog navigation.
    /// </summary>
    public class CatalogNavigation : ICatalogNavigation
    {
        private const string SiteMapProviderName = "GlobalNavigationTaxonomyProvider";
        private const string LocalSharePointResultsSourceName = "Local SharePoint Results";
        private readonly ILogger _logger;
        private readonly NavigationHelper _navigationHelper;
        private readonly SearchHelper _searchHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogNavigation" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="navigationHelper">The navigation helper.</param>
        /// <param name="searchHelper">The search helper.</param>
        public CatalogNavigation(ILogger logger, NavigationHelper navigationHelper, SearchHelper searchHelper)
        {
            this._logger = logger;
            this._navigationHelper = navigationHelper;
            this._searchHelper = searchHelper;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public CatalogNavigationType Type
        {
            get
            {
                if (TaxonomyNavigationContext.Current != null)
                {
                    var currentUrl = HttpContext.Current.Request.Url;
                    if (TaxonomyNavigationContext.Current.HasCatalogUrl)
                    {
                        return CatalogNavigationType.ItemPage;
                    }
                    else if(TaxonomyNavigationContext.Current.NavigationTerm != null)
                    {
                        return CatalogNavigationType.CategoryPage;
                    }
                    else
                    {
                        return CatalogNavigationType.None;
                    }

                }
                else
                {
                    return CatalogNavigationType.None;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the catalog navigation term managed property.
        /// </summary>
        /// <value>
        /// The name of the catalog navigation term managed property.
        /// </value>
        public string CatalogNavigationTermManagedPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the association key managed property.
        /// </summary>
        /// <value>
        /// The name of the association key managed property.
        /// </value>
        public string AssociationKeyManagedPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the association key value.
        /// </summary>
        /// <value>
        /// The association key value.
        /// </value>
        public string AssociationKeyValue { get; set; }

        public Uri GetVariationPeerUrl(VariationLabel label)
        {
            var currentUrl = HttpContext.Current.Request.Url;
            switch (this.Type)
            {
                case CatalogNavigationType.CategoryPage:
                    return this.GetPeerCatalogCategoryUrl(currentUrl, label);
                case CatalogNavigationType.ItemPage:
                    return this.GetPeerCatalogItemUrl(currentUrl, label);
                default:
                    return this.GetPeerUrl(label, currentUrl);
            }
        }

        private Uri GetPeerUrl(VariationLabel label, Uri currentUrl)
        {
            if (currentUrl.LocalPath.StartsWith("/_layouts"))
            {
                return new Uri(SPUtility.ConcatUrls(label.TopWebUrl, currentUrl.PathAndQuery));
            }
            else
            {
                try
                {
                    return new Uri(Variations.GetPeerUrl(SPContext.Current.Web, currentUrl.AbsoluteUri, label.Title),
                        UriKind.Relative);
                }
                catch (ArgumentOutOfRangeException)
                {
                    this._logger.Info(@"GetPeerUrl: Cannot find variation peer URL with 'Variations.GetPeerUrl'.  
                                        Using label web URL with path and query strings as navigation URL.");

                    // Construct peer URL with top web URL + path + query.
                    var topWebUrl = new Uri(label.TopWebUrl + "/");
                    var pathAndQuerySegments = topWebUrl.Segments.Concat(currentUrl.Segments.Skip(topWebUrl.Segments.Length));
                    return new Uri(topWebUrl, string.Join(string.Empty, pathAndQuerySegments));
                }
            }
        }

        private Uri GetPeerCatalogCategoryUrl(Uri currentUrl, VariationLabel label)
        {
            // Get current navigation term ID
            var termId = TaxonomyNavigationContext.Current.NavigationTerm.Id;

            var labelSiteRelativeUrl = new Uri(label.TopWebUrl).AbsolutePath;
            using (var labelWeb = SPContext.Current.Site.OpenWeb(labelSiteRelativeUrl))
            {
                // Create view to return all navigation terms
                var view = new NavigationTermSetView(labelWeb, SiteMapProviderName)
                {
                    ExcludeTermsByProvider = false
                };

                var navigationTermSet =
                    TaxonomyNavigation.GetTermSetForWeb(labelWeb, SiteMapProviderName, true).GetWithNewView(view);

                // Get the matching label navigation term and return it's friendly URL
                var navigationTerm = this._navigationHelper.GetNavigationTermById(navigationTermSet.Terms,
                    termId);
                if (navigationTerm != null)
                {
                    this._logger.Info("GetPeerCatalogCategoryUrl: Navigation term found for term id '{0}': '{1}'",
                        termId, navigationTerm.Title);
                    return new Uri(navigationTerm.GetResolvedDisplayUrl(String.Empty), UriKind.Relative);
                }
                else
                {
                    this._logger.Error("GetPeerCatalogCategoryUrl: Navigation term not found for term id '{0}'", termId);
                    return new Uri(Variations.GetPeerUrl(SPContext.Current.Web, currentUrl.AbsoluteUri, label.Title),
                        UriKind.Relative);
                }
            }
        }

        private Uri GetPeerCatalogItemUrl(Uri currentUrl, VariationLabel label)
        {
            this.ValidateProperties("GetPeerCatalogItemUrl");

            var url = new Uri(Variations.GetPeerUrl(SPContext.Current.Web, currentUrl.AbsoluteUri, label.Title), UriKind.Relative);

            // TODO: Change result source
            var searchResultSource = this._searchHelper.GetResultSourceByName(LocalSharePointResultsSourceName, SearchObjectLevel.Ssa);

            var labelLocalAgnosticLanguage = label.Language.Split('-').First();
            var query = new KeywordQuery(SPContext.Current.Web)
            {
                SourceId = searchResultSource.Id,
                QueryText = string.Format("{0}:{1} {2}={3}", this.AssociationKeyManagedPropertyName, this.AssociationKeyValue, "DetectedLanguage", labelLocalAgnosticLanguage),
            };

            // 
            query.SelectProperties.AddRange(new[] { this.CatalogNavigationTermManagedPropertyName, "Path", "spSiteUrl", "ListID" });
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

        private void ValidateProperties(string callingMethodName)
        {
            if (string.IsNullOrEmpty(this.CatalogNavigationTermManagedPropertyName))
            {
                throw new NullReferenceException(string.Format(
                    "{0}: Property '{1}' is null or empty string.", callingMethodName, "CatalogNavigationTermManagedPropertyName"));
            }

            if (string.IsNullOrEmpty(this.AssociationKeyManagedPropertyName))
            {
                throw new NullReferenceException(string.Format(
                    "{0}: Property '{1}' is null or empty string.", callingMethodName, "AssociationKeyManagedPropertyName"));
            }

            if (string.IsNullOrEmpty(this.AssociationKeyValue))
            {
                throw new NullReferenceException(string.Format(
                    "{0}: Property '{1}' is null or empty string.", callingMethodName, "AssociationKeyValue"));
            }
        }

        private static string[] ReplaceFirstSegments(IList<string> sourceSegments, IEnumerable<string> destinationSegments)
        {
            var replacedSegments = new List<string>(destinationSegments);
            for (var i = 0; i < sourceSegments.Count; i++)
            {
                replacedSegments[i] = (i == (sourceSegments.Count - 1)) ? sourceSegments[i] + "/" : sourceSegments[i];
            }

            return replacedSegments.ToArray();
        }
    }
}
