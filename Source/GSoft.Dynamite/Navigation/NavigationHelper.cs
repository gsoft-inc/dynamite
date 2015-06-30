using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GSoft.Dynamite.Extensions;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Navigation;
using GSoft.Dynamite.Pages;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Publishing.Navigation;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Navigation configuration helper.
    /// </summary>
    public class NavigationHelper : INavigationHelper
    {
        /// <summary>
        /// System custom property for the Target Url
        /// </summary>
        public const string SystemTargetUrl = "_Sys_Nav_TargetUrl";

        /// <summary>
        /// System custom property for the Catalog Target Url
        /// </summary>
        public const string SystemCatalogTargetUrl = "_Sys_Nav_CatalogTargetUrl";

        /// <summary>
        /// System custom property for the Simple Link Url
        /// </summary>
        public const string SystemSimpleLinkUrl = "_Sys_Nav_SimpleLinkUrl";

        /// <summary>
        /// System custom property Fo the Navigation Term Set
        /// </summary>
        public const string SystemIsNavigationTermSet = "_Sys_Nav_IsNavigationTermSet";

        /// <summary>
        /// System custom property for the Target Url for Child terms
        /// </summary>
        public const string SystemTargetUrlForChildTerms = "_Sys_Nav_TargetUrlForChildTerms";

        /// <summary>
        /// System custom property for the catalog target url for child terms
        /// </summary>
        public const string SystemCatalogTargetUrlForChildTerms = "_Sys_Nav_CatalogTargetUrlForChildTerms";

        private ITaxonomyService taxonomyService;
        private ITaxonomyHelper taxonomyHelper;
        private ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationHelper" /> class.
        /// </summary>
        /// <param name="taxonomyService">The taxonomy service.</param>
        /// <param name="taxonomyHelper">The taxonomy helper.</param>
        /// <param name="logger">Logging utility</param>
        public NavigationHelper(ITaxonomyService taxonomyService, ITaxonomyHelper taxonomyHelper, ILogger logger)
        {
            this.taxonomyService = taxonomyService;
            this.taxonomyHelper = taxonomyHelper;
            this.logger = logger;
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
                var termStore = this.taxonomyHelper.GetDefaultSiteCollectionTermStore(taxonomySession);
                var group = this.taxonomyService.GetTermGroupFromStore(termStore, settings.TermGroup.Name);
                var termSet = this.taxonomyService.GetTermSetFromGroup(termStore, group, settings.TermSet.Label);

                // Flag the term set as a navigation term set
                termSet.SetCustomProperty(SystemIsNavigationTermSet, "True");
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
                    var termStore = this.taxonomyHelper.GetDefaultSiteCollectionTermStore(taxonomySession);
                    var group = this.taxonomyService.GetTermGroupFromStore(termStore, settings.TermGroup.Name);
                    var termSet = this.taxonomyService.GetTermSetFromGroup(termStore, group, settings.TermSet.Label);

                    string propertyValue;
                    if (termSet.CustomProperties.TryGetValue(SystemIsNavigationTermSet, out propertyValue))
                    {
                        termSet.DeleteCustomProperty(SystemIsNavigationTermSet);
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
        public NavigationTerm FindNavigationTermById(IEnumerable<NavigationTerm> navigationTerms, Guid id)
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
            return this.FindNavigationTermById(childTerms, id);
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
        /// <param name="web">The web for which we want to change a term's target URL in its taxonomy navigation term set</param>
        /// <param name="termDrivenPageInfo">The term driven page setting info</param>
        public void SetTermDrivenPageSettings(SPWeb web, TermDrivenPageSettingInfo termDrivenPageInfo)
        {
            // Force the taxonomy session to update the cache due to previous changes
            // Note: this is necessary in the context of the components installation script
            var taxonomySession = new TaxonomySession(web.Site, true);

            if (taxonomySession.TermStores.Count > 0)
            {
                var defaultTermStore = this.taxonomyHelper.GetDefaultSiteCollectionTermStore(taxonomySession);

                // Term Set setting
                if (termDrivenPageInfo.IsTermSet)
                {
                    this.SetTermDrivenPageSettingsOnTermSet(termDrivenPageInfo, defaultTermStore);
                }

                // Term setting
                if (termDrivenPageInfo.IsTerm)
                {
                    this.SetTermDrivenPageSettingsOnTerm(web, termDrivenPageInfo);
                }
            }
        }
        
        /// <summary>
        /// Method to take a term configured as a term driven page to a simple link url.
        /// </summary>
        /// <param name="web">The web for which we want to change a term's target URL in its taxonomy navigation term set</param>
        /// <param name="termInfo">The metadata term to reset</param>
        public void ResetTermDrivenPageToSimpleLinkUrl(SPWeb web, TermInfo termInfo)
        {
            // Get the web-specific navigation settings
            var webNavigationSettings = new WebNavigationSettings(web);
            
            var taxonomySession = new TaxonomySession(web.Site);
            var defaultStore = taxonomySession.TermStores[webNavigationSettings.GlobalNavigation.TermStoreId];
            var termSet = defaultStore.GetTermSet(webNavigationSettings.GlobalNavigation.TermSetId);

            // Get the taxonomy term
            var term = termSet.GetTerm(termInfo.Id);

            if (term != null)
            {
                term.SetLocalCustomProperty(SystemTargetUrl, string.Empty);
                term.TermSet.TermStore.CommitAll();
            }
        }

        private void SetTermDrivenPageSettingsOnTermSet(TermDrivenPageSettingInfo termDrivenPageInfo, TermStore defaultTermStore)
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
                    termSet.SetCustomProperty(SystemTargetUrlForChildTerms, termDrivenPageInfo.TargetUrlForChildTerms);
                }

                if (!string.IsNullOrEmpty(termDrivenPageInfo.CatalogTargetUrlForChildTerms))
                {
                    termSet.SetCustomProperty(SystemCatalogTargetUrlForChildTerms, termDrivenPageInfo.CatalogTargetUrlForChildTerms);
                }

                termSet.TermStore.CommitAll();
            }
        }

        private void SetTermDrivenPageSettingsOnTerm(SPWeb currentWeb, TermDrivenPageSettingInfo termDrivenPageInfo)
        {
            SPWeb webWithNavSettings = null;
            var webNavigationSettings = FindTaxonomyWebNavigationSettingsInWebOrInParents(currentWeb, out webWithNavSettings);

            if (webNavigationSettings != null
                && webNavigationSettings.GlobalNavigation.Source == StandardNavigationSource.TaxonomyProvider)
            {
                var taxonomySession = new TaxonomySession(webWithNavSettings, true);
                var defaultStore = taxonomySession.DefaultSiteCollectionTermStore; //taxonomySession.TermStores[webNavigationSettings.GlobalNavigation.TermStoreId];

                if (defaultStore.Id != webNavigationSettings.GlobalNavigation.TermStoreId)
                {
                    defaultStore = taxonomySession.TermStores[webNavigationSettings.GlobalNavigation.TermStoreId];
                }

                var previousThreadCulture = CultureInfo.CurrentCulture;
                var previousThreadUiCulture = CultureInfo.CurrentUICulture;
                var previousWorkingLanguage = defaultStore.WorkingLanguage;

                try
                {
                    CultureInfo currentWebCulture = webWithNavSettings.Locale;
                    CultureInfo currentWebUiCulture = new CultureInfo((int)webWithNavSettings.Language);

                    // Force thread culture/uiculture and term store working language
                    Thread.CurrentThread.CurrentCulture = currentWebCulture;
                    Thread.CurrentThread.CurrentUICulture = currentWebUiCulture;
                    defaultStore.WorkingLanguage = currentWebCulture.LCID;   // force the working language to fit with the current web language

                    var termSet = defaultStore.GetTermSet(webNavigationSettings.GlobalNavigation.TermSetId);

                    // Get the taxonomy term
                    var term = termSet.GetTerm(termDrivenPageInfo.Term.Id);

                    if (term != null)
                    {
                        string isNavigationTermSet;

                        // Check if the term term set is flagged as navigation term set
                        // By default a TermSet doesn't have the custom property "_Sys_Nav_IsNavigationTermSet" so we can't acces it directly in the collection
                        term.TermSet.CustomProperties.TryGetValue(SystemIsNavigationTermSet, out isNavigationTermSet);

                        // If the term set allow navigation
                        if (!string.IsNullOrEmpty(isNavigationTermSet))
                        {
                            // Get the associated navigation term set 
                            var navigationTermSet = NavigationTermSet.GetAsResolvedByWeb(
                                term.TermSet,
                                webWithNavSettings,
                                StandardNavigationProviderNames.GlobalNavigationTaxonomyProvider);

                            navigationTermSet = navigationTermSet.GetAsEditable(taxonomySession);

                            // Get the navigation term
                            var navigationTerm = FindTermInNavigationTermsCollection(navigationTermSet.Terms, term.Id);
                            if (navigationTerm != null)
                            {
                                // Gotta re-fetch the navigation term as an "editable" instance in order to avoid UnauthorizedAccessExceptions
                                navigationTerm = navigationTerm.GetAsEditable(taxonomySession);

                                navigationTerm.ExcludeFromCurrentNavigation = termDrivenPageInfo.ExcludeFromCurrentNavigation;
                                navigationTerm.ExcludeFromGlobalNavigation = termDrivenPageInfo.ExcludeFromGlobalNavigation;

                                if (termDrivenPageInfo.IsSimpleLinkOrHeader)
                                {
                                    if (!string.IsNullOrEmpty(termDrivenPageInfo.SimpleLinkOrHeader))
                                    {
                                        if (navigationTerm.LinkType == NavigationLinkType.FriendlyUrl)
                                        {
                                            // clear any existing target URL on the term
                                            navigationTerm.TargetUrl.Value = string.Empty;
                                            navigationTerm.TargetUrlForChildTerms.Value = string.Empty;
                                            navigationTerm.CatalogTargetUrl.Value = string.Empty;
                                            navigationTerm.CatalogTargetUrlForChildTerms.Value = string.Empty;
                                        }

                                        if (navigationTerm.LinkType != NavigationLinkType.SimpleLink)
                                        {
                                            navigationTerm.LinkType = NavigationLinkType.SimpleLink;
                                        }

                                        navigationTerm.SimpleLinkUrl = termDrivenPageInfo.SimpleLinkOrHeader;
                                    }
                                }
                                else
                                {
                                    // Set URLs properties
                                    if (navigationTerm.LinkType != NavigationLinkType.FriendlyUrl)
                                    {
                                        navigationTerm.LinkType = NavigationLinkType.FriendlyUrl;
                                    }

                                    if (!string.IsNullOrEmpty(termDrivenPageInfo.TargetUrl))
                                    {
                                        navigationTerm.TargetUrl.Value = termDrivenPageInfo.TargetUrl;
                                    }

                                    if (!string.IsNullOrEmpty(termDrivenPageInfo.TargetUrlForChildTerms))
                                    {
                                        navigationTerm.TargetUrlForChildTerms.Value = termDrivenPageInfo.TargetUrlForChildTerms;
                                    }

                                    if (!string.IsNullOrEmpty(termDrivenPageInfo.CatalogTargetUrl))
                                    {
                                        navigationTerm.CatalogTargetUrl.Value = termDrivenPageInfo.CatalogTargetUrl;
                                    }

                                    if (!string.IsNullOrEmpty(termDrivenPageInfo.CatalogTargetUrlForChildTerms))
                                    {
                                        navigationTerm.CatalogTargetUrlForChildTerms.Value = termDrivenPageInfo.CatalogTargetUrlForChildTerms;
                                    }
                                }

                                // Commit all updates
                                defaultStore.CommitAll();
                            }
                            else
                            {
                                this.logger.Warn(
                                    "TaxonomyHelper.SetTermDrivenPageSettingsOnTerm: Failed to find corresponding NavigationTerm for term ID={0} in term set ID={1} Name={2}",
                                    term.Id,
                                    navigationTermSet.Id,
                                    navigationTermSet.TaxonomyName);
                            }
                        }
                    }
                }
                finally
                {
                    // Restore previous thread cultures and term store working language
                    Thread.CurrentThread.CurrentCulture = previousThreadCulture;
                    Thread.CurrentThread.CurrentUICulture = previousThreadUiCulture;
                    defaultStore.WorkingLanguage = previousWorkingLanguage;
                }
            }
            else
            {
                this.logger.Warn(
                    "TaxonomyHelper.SetTermDrivenPageSettingsOnTerm: Failed to find taxonomy-type WebNavigationSettings in web ID={0} Url={1} or in any of its parent webs. At least one SPWeb in the hierarchy should have a GlobalNavigation setting of source type Taxonomy.",
                    currentWeb.ID,
                    currentWeb.Url);
            }
        }

        /// <summary>
        /// Finds a term by ID recursively in a navigation term collection
        /// </summary>
        /// <param name="navigationTerms">
        /// The current level of terms to look through (each term's children will be looped through recursively 
        /// if none match in the current level)
        /// </param>
        /// <param name="termId">The term ID to look for</param>
        /// <returns>The navigation term or null if not found</returns>
        private static NavigationTerm FindTermInNavigationTermsCollection(ICollection<NavigationTerm> navigationTerms, Guid termId)
        {
            var foundTerm = navigationTerms.FirstOrDefault(termAtThisLevel => termAtThisLevel.Id == termId);

            if (foundTerm == null)
            {
                foreach (NavigationTerm termAtThisLevel in navigationTerms)
                {
                    foundTerm = FindTermInNavigationTermsCollection(termAtThisLevel.Terms, termId);

                    if (foundTerm != null)
                    {
                        // stop looping and recursing as soon as we hit a match
                        break;
                    }
                }
            }

            return foundTerm;
        }

        private static WebNavigationSettings FindTaxonomyWebNavigationSettingsInWebOrInParents(SPWeb web, out SPWeb webWithNavigationSettings)
        {
            var currentWebNavSettings = new WebNavigationSettings(web);
            webWithNavigationSettings = web;

            if (currentWebNavSettings.GlobalNavigation.Source == StandardNavigationSource.InheritFromParentWeb
                && web.ParentWeb != null)
            {
                // current web inherits its settings from its parent, so we gotta look upwards to the parent webs
                // recursively until we find a match
                return FindTaxonomyWebNavigationSettingsInWebOrInParents(web.ParentWeb, out webWithNavigationSettings);
            }

            return currentWebNavSettings;
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