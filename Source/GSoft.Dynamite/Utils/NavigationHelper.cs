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

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Navigation configuration helper.
    /// </summary>
    public class NavigationHelper : INavigationHelper
    {
        private readonly ITaxonomyHelper taxonomyHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationHelper" /> class.
        /// </summary>
        /// <param name="taxonomyHelper">The taxonomy helper.</param>
        public NavigationHelper(ITaxonomyHelper taxonomyHelper)
        {
            this.taxonomyHelper = taxonomyHelper;
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
