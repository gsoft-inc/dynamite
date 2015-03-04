using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GSoft.Dynamite.Navigation;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Navigation;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Navigation
{
    using GSoft.Dynamite.Pages;
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
        public void SetWebNavigationSettings(SPWeb web, ManagedNavigationInfo settings)
        {
            var taxonomySession = new TaxonomySession(web.Site);
            if (taxonomySession.TermStores.Count > 0)
            {
                // we assume we're always dealing with the site coll's default term store
                var termStore = taxonomySession.DefaultSiteCollectionTermStore;
                var group = this.taxonomyService.GetTermGroupFromStore(termStore, settings.TermGroup.Name);
                var termSet = this.taxonomyService.GetTermSetFromGroup(termStore, group, settings.TermSet.Label);

                // Flag the term set as a navigation term set
                termSet.SetCustomProperty("_Sys_Nav_IsNavigationTermSet", "True");
                termSet.TermStore.CommitAll();

                var navigationSettings = new WebNavigationSettings(web);

                navigationSettings.GlobalNavigation.Source = StandardNavigationSource.TaxonomyProvider;
                navigationSettings.GlobalNavigation.TermStoreId = termStore.Id;
                navigationSettings.GlobalNavigation.TermSetId = termSet.Id;

                navigationSettings.CurrentNavigation.Source = StandardNavigationSource.TaxonomyProvider;
                navigationSettings.CurrentNavigation.TermStoreId = termStore.Id;
                navigationSettings.CurrentNavigation.TermSetId = termSet.Id;

                navigationSettings.AddNewPagesToNavigation = settings.AddNewPagesToNavigation;
                navigationSettings.CreateFriendlyUrlsForNewPages = settings.CreateFriendlyUrlsForNewsPages;
                navigationSettings.Update(taxonomySession);

                if (settings.PreserveTaggingOnTermSet)
                {
                    termSet.IsAvailableForTagging = true;
                    termSet.TermStore.CommitAll();
                }
            }
        }

        /// <summary>
        /// Reset web navigation to its default configuration. Disabled the term set as navigation term set.
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="settings">The managed navigation settings. Set null if you want to keep the associated termset unchanged</param>
        public void ResetWebNavigationToDefault(SPWeb web, ManagedNavigationInfo settings)
        {
            var taxonomySession = new TaxonomySession(web.Site);
            if (taxonomySession.TermStores.Count > 0)
            {
                if (settings != null)
                {
                    // Disable the navigation flag on the the term set
                    var termStore = taxonomySession.DefaultSiteCollectionTermStore;
                    var group = this.taxonomyService.GetTermGroupFromStore(termStore, settings.TermGroup.Name);
                    var termSet = this.taxonomyService.GetTermSetFromGroup(termStore, group, settings.TermSet.Label);

                    const string PropertyName = "_Sys_Nav_IsNavigationTermSet";
                    string propertyValue;
                    if (termSet.CustomProperties.TryGetValue(PropertyName, out propertyValue))
                    {
                        termSet.DeleteCustomProperty(PropertyName);
                        termSet.TermStore.CommitAll();
                    }
                }

                var navigationSettings = new WebNavigationSettings(web);
                navigationSettings.ResetToDefaults();
                navigationSettings.Update(taxonomySession);
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
        /// Generates the friendly URL slug with a default maximum length of 75 characters.
        /// </summary>
        /// <param name="phrase">The phrase.</param>
        /// <returns>A friendly URL slug containing human readable characters.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public string GenerateFriendlyUrlSlug(string phrase)
        {
            return this.GenerateFriendlyUrlSlug(phrase, 75);
        }

        /// <summary>
        /// Generates the friendly URL slug.
        /// </summary>
        /// <param name="phrase">The phrase.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>A friendly URL slug containing human readable characters.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Slugs should be normalized to lowercase.")]
        public string GenerateFriendlyUrlSlug(string phrase, int maxLength)
        {
            // Remove diacritics (accented characters)
            var slug = RemoveDiacritics(phrase.ToLower(CultureInfo.InvariantCulture));

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
            // Force the taxonomy session to update the cache due to previous changes
            // Note: this is necessary in the context of the components installation script
            var taxonomySession = new TaxonomySession(site, true);
            if (taxonomySession.TermStores.Count > 0)
            {
                var defaultTermStore = taxonomySession.DefaultSiteCollectionTermStore;

                // Term Set setting
                if (termDrivenPageInfo.IsTermSet)
                {
                    // Get the term set group by name
                    // Note, when you build the term store hierachy by XML using Gary Lapointe Cmdlet, the term group ID isn't kept
                    var group = this.taxonomyService.GetTermGroupFromStore(defaultTermStore, termDrivenPageInfo.TermSet.Group.Name);

                    if (group != null)
                    {
                        // Get the term set 
                        var termSet = group.TermSets[termDrivenPageInfo.TermSet.Id];

                        // Set URLs
                        if (!string.IsNullOrEmpty(termDrivenPageInfo.TargetUrlForChildTerms))
                        {
                            termSet.SetCustomProperty("_Sys_Nav_TargetUrlForChildTerms", termDrivenPageInfo.TargetUrlForChildTerms);
                        }

                        if (!string.IsNullOrEmpty(termDrivenPageInfo.CatalogTargetUrlForChildTerms))
                        {
                            termSet.SetCustomProperty("_Sys_Nav_CatalogTargetUrlForChildTerms", termDrivenPageInfo.CatalogTargetUrlForChildTerms);
                        }

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
                        var terms = new List<Term> { term };
                        terms.AddRange(term.ReusedTerms);

                        // For the orginal term and its reuses
                        foreach (var currentTerm in terms)
                        {
                            string isNavigationTermSet;

                            // Check if the term term set is flagged as navigation term set
                            // By default a TermSet doesn't have the custom property "_Sys_Nav_IsNavigationTermSet" so we can't acces it directly in the collection
                            currentTerm.TermSet.CustomProperties.TryGetValue("_Sys_Nav_IsNavigationTermSet", out isNavigationTermSet);

                            // If the term set allow navigation
                            if (!string.IsNullOrEmpty(isNavigationTermSet))
                            {
                                // Get the associated navigation term set 
                                var navigationTermSet = NavigationTermSet.GetAsResolvedByWeb(
                                    currentTerm.TermSet, 
                                    site.RootWeb,
                                    StandardNavigationProviderNames.CurrentNavigationTaxonomyProvider);

                                // Get the navigation term
                                var navigationTerm = navigationTermSet.Terms.FirstOrDefault(t => t.Id.Equals(currentTerm.Id));
                                if (navigationTerm != null)
                                {
                                    navigationTerm.ExcludeFromCurrentNavigation =
                                        termDrivenPageInfo.ExcludeFromCurrentNavigation;
                                    navigationTerm.ExcludeFromGlobalNavigation =
                                        termDrivenPageInfo.ExcludeFromGlobalNavigation;
                                }

                                if (termDrivenPageInfo.IsSimpleLinkOrHeader)
                                {
                                    if (!string.IsNullOrEmpty(termDrivenPageInfo.SimpleLinkOrHeader))
                                    {
                                        currentTerm.SetLocalCustomProperty(
                                            "_Sys_Nav_SimpleLinkUrl",
                                            termDrivenPageInfo.SimpleLinkOrHeader);
                                    }
                                }
                                else
                                {
                                    // Set URLs properties
                                    if (!string.IsNullOrEmpty(termDrivenPageInfo.TargetUrl))
                                    {
                                        currentTerm.SetLocalCustomProperty("_Sys_Nav_TargetUrl", termDrivenPageInfo.TargetUrl);
                                    }

                                    if (!string.IsNullOrEmpty(termDrivenPageInfo.TargetUrlForChildTerms))
                                    {
                                        currentTerm.SetLocalCustomProperty(
                                            "_Sys_Nav_TargetUrlForChildTerms",
                                            termDrivenPageInfo.TargetUrlForChildTerms);
                                    }

                                    if (!string.IsNullOrEmpty(termDrivenPageInfo.CatalogTargetUrl))
                                    {
                                        currentTerm.SetLocalCustomProperty(
                                            "_Sys_Nav_CatalogTargetUrl",
                                            termDrivenPageInfo.CatalogTargetUrl);
                                    }

                                    if (!string.IsNullOrEmpty(termDrivenPageInfo.CatalogTargetUrlForChildTerms))
                                    {
                                        currentTerm.SetLocalCustomProperty(
                                            "_Sys_Nav_CatalogTargetUrlForChildTerms",
                                            termDrivenPageInfo.CatalogTargetUrlForChildTerms);
                                    }
                                }

                                // Commit all updates
                                currentTerm.TermStore.CommitAll();
                            }
                        }
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
