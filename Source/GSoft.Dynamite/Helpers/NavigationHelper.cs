using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Navigation;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Navigation;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Helpers
{
    using GSoft.Dynamite.Utils;

    /// <summary>
    /// Navigation configuration helper.
    /// </summary>
    public class NavigationHelper : INavigationHelper
    {
        private readonly ITaxonomyHelper taxonomyHelper;
        private readonly ITaxonomyService taxonomyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationHelper" /> class.
        /// </summary>
        /// <param name="taxonomyHelper">The taxonomy helper.</param>
        /// <param name="taxonomyService">The taxonomy service</param>
        public NavigationHelper(ITaxonomyHelper taxonomyHelper, ITaxonomyService taxonomyService)
        {
            this.taxonomyHelper = taxonomyHelper;
            this.taxonomyService = taxonomyService;
        }

        /// <summary>
        /// Sets the web navigation settings.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="settings">The settings.</param>
        public void SetWebNavigationSettings(SPWeb web, ManagedNavigationSettings settings)
        {
            var taxonomySession = new TaxonomySession(web.Site);
            if (taxonomySession.TermStores.Count > 0)
            {
                var termStore = taxonomySession.TermStores[settings.TermStoreName];
                var group = this.taxonomyHelper.GetTermGroupByName(termStore, settings.TermGroupName);
                var termSet = this.taxonomyHelper.GetTermSetByName(termStore, group, settings.TermSetName);
                var navigationSettings = new WebNavigationSettings(web);
                
                navigationSettings.GlobalNavigation.TermStoreId = termStore.Id;
                navigationSettings.GlobalNavigation.TermSetId = termSet.Id;
                navigationSettings.Update(taxonomySession);

                if (settings.PreserveTaggingOnTermSet)
                {
                    termSet.IsAvailableForTagging = true;
                    termStore.CommitAll(); 
                }
            }
        }

        /// <summary>
        /// Gets the navigation term by identifier.
        /// </summary>
        /// <param name="navigationTerms">The navigation terms.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>The navigation term.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public NavigationTerm GetNavigationTermById(IEnumerable<NavigationTerm> navigationTerms, Guid id)
        {
            var terms = navigationTerms == null ? new NavigationTerm[] { } : navigationTerms.ToArray();
            if (terms.Length <= 0)
            {
                return null;
            }

            var term = terms.FirstOrDefault(x => x.Id == id);
            if (term != null)
            {
                return term;
            }

            var childTerms = terms.SelectMany(x => x.Terms);
            return this.GetNavigationTermById(childTerms, id);
        }

        /// <summary>
        /// Gets the navigation parent terms.
        /// </summary>
        /// <param name="navigationTerm">The navigation term.</param>
        /// <returns>A collection of parent terms, traversing upwards.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public IEnumerable<NavigationTerm> GetNavigationParentTerms(NavigationTerm navigationTerm)
        {
            if (navigationTerm != null)
            {
                var currentTerm = navigationTerm;
                var navigationTerms = new List<NavigationTerm> { currentTerm };
                while (currentTerm.Parent != null)
                {
                    currentTerm = currentTerm.Parent;
                    navigationTerms.Add(currentTerm);
                }

                return navigationTerms; 
            }

            return new List<NavigationTerm>();
        }

        /// <summary>
        /// Generates the friendly URL slug.
        /// </summary>
        /// <param name="phrase">The phrase.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>A friendly URL slug containing human readable characters.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public string GenerateFriendlyUrlSlug(string phrase, int maxLength = 75)
        {
            // Remove diacritics (accented characters)
            var slug = RemoveDiacritics(phrase.ToLower());

            // invalid chars, make into spaces
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);

            // convert multiple spaces/hyphens into one space       
            slug = Regex.Replace(slug, @"[\s-]+", " ").Trim();

            // cut and trim it
            slug = slug.Substring(0, slug.Length <= maxLength ? slug.Length : maxLength).Trim();

            // hyphens
            slug = Regex.Replace(slug, @"\s", "-");

            return slug;
        }

        /// <summary>
        /// Set term driven page settings in the term store
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="termDrivenPageInfo">The term driven page setting info</param>
        public void SetTermDrivenPageSettings(SPSite site, TermDrivenPageSettingInfo termDrivenPageInfo)
        {
            var taxonomySession = new TaxonomySession(site);
            if (taxonomySession.TermStores.Count > 0)
            {
                var defaultTermStore = taxonomySession.DefaultSiteCollectionTermStore;

                // Term Set setting
                if (termDrivenPageInfo.IsTermSet)
                {
                    // Get the term set group by name
                    // Note, when you build the term store hierachy by XML using Gary Lapointe Cmdlet, the term group ID isn't kept
                    var group = this.taxonomyHelper.GetTermGroupByName(defaultTermStore, termDrivenPageInfo.TermSet.Group.Name);

                    if (group != null)
                    {
                        // Get the term set 
                        var termSet = this.taxonomyHelper.GetTermSetById(defaultTermStore, group, termDrivenPageInfo.TermSet.Id);

                        // Set URLs
                        if (!string.IsNullOrEmpty(termDrivenPageInfo.TargetUrlForChildTerms))
                        {
                            termSet.SetCustomProperty("_Sys_Nav_TargetUrlForChildTerms", termDrivenPageInfo.TargetUrlForChildTerms);
                        }

                        if (!string.IsNullOrEmpty(termDrivenPageInfo.CatalogTargetUrlForChildTerms))
                        {
                            termSet.SetCustomProperty("_Sys_Nav_CatalogTargetUrlForChildTerms", termDrivenPageInfo.CatalogTargetUrlForChildTerms);
                        }

                        // Flag the term set as a navigation term set
                        termSet.SetCustomProperty("_Sys_Nav_IsNavigationTermSet", "True");
                        termSet.TermStore.CommitAll();
                    }
                }

                // Term setting
                if (termDrivenPageInfo.IsTerm)
                {
                    // Get the taxonomy term
                    var term = this.taxonomyService.GetTermForId(site, termDrivenPageInfo.Term.Id);

                    if (term != null)
                    {
                        // Check in the term term set is flagged as navigation term set
                        var isNavigationTermSet = term.TermSet.CustomProperties["_Sys_Nav_IsNavigationTermSet"];
                        if (string.CompareOrdinal(isNavigationTermSet, "True") != 0)
                        {
                            term.TermSet.SetCustomProperty("_Sys_Nav_IsNavigationTermSet", "True");
                            term.TermSet.TermStore.CommitAll();
                        }

                        // Get the associated navigation term set 
                        var navigationTermSet = NavigationTermSet.GetAsResolvedByWeb(term.TermSet, site.RootWeb, StandardNavigationProviderNames.CurrentNavigationTaxonomyProvider);

                        // Get the navigation term
                        var navigationTerm = navigationTermSet.Terms.FirstOrDefault(t => t.Id.Equals(term.Id));
                        if (navigationTerm != null)
                        {
                            navigationTerm.ExcludeFromCurrentNavigation = termDrivenPageInfo.ExcludeFromCurrentNavigation;
                            navigationTerm.ExcludeFromGlobalNavigation = termDrivenPageInfo.ExcludeFromGlobalNavigation;
                        }

                        if (termDrivenPageInfo.IsSimpleLinkOrHeader)
                        {
                            if (!string.IsNullOrEmpty(termDrivenPageInfo.SimpleLinkOrHeader))
                            {
                                term.SetLocalCustomProperty("_Sys_Nav_SimpleLinkUrl", termDrivenPageInfo.SimpleLinkOrHeader);
                            }
                        }
                        else
                        {
                            // Set URLs properties
                            if (!string.IsNullOrEmpty(termDrivenPageInfo.TargetUrl))
                            {
                                term.SetLocalCustomProperty("_Sys_Nav_TargetUrl", termDrivenPageInfo.TargetUrl);
                            }

                            if (!string.IsNullOrEmpty(termDrivenPageInfo.TargetUrlForChildTerms))
                            {
                                term.SetLocalCustomProperty("_Sys_Nav_TargetUrlForChildTerms", termDrivenPageInfo.TargetUrlForChildTerms);
                            }

                            if (!string.IsNullOrEmpty(termDrivenPageInfo.CatalogTargetUrl))
                            {
                                term.SetLocalCustomProperty("_Sys_Nav_CatalogTargetUrl", termDrivenPageInfo.CatalogTargetUrl);
                            }

                            if (!string.IsNullOrEmpty(termDrivenPageInfo.CatalogTargetUrlForChildTerms))
                            {
                                term.SetLocalCustomProperty("_Sys_Nav_CatalogTargetUrlForChildTerms", termDrivenPageInfo.CatalogTargetUrlForChildTerms);
                            }
                        }

                        // Commit all updates
                        term.TermStore.CommitAll();
                    }
                }             
            }
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var character in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(character);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
