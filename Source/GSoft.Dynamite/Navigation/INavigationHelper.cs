namespace GSoft.Dynamite.Navigation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using GSoft.Dynamite.Pages;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Publishing.Navigation;

    /// <summary>
    /// Navigation configuration helper.
    /// </summary>
    public interface INavigationHelper
    {
        /// <summary>
        /// Sets the web navigation settings.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="settings">The settings.</param>
        [Obsolete("Use NavigationSettingsInfos class instead")]
        void SetWebNavigationSettings(SPWeb web, ManagedNavigationSettings settings);

        /// <summary>
        /// Sets the web navigation settings.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="settings">The settings.</param>
        void SetWebNavigationSettings(SPWeb web, ManagedNavigationInfo settings);

        /// <summary>
        /// Gets the navigation term by identifier.
        /// </summary>
        /// <param name="navigationTerms">The navigation terms.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>The navigation term.</returns>
        NavigationTerm GetNavigationTermById(IEnumerable<NavigationTerm> navigationTerms, Guid id);

        /// <summary>
        /// Gets the navigation parent terms.
        /// </summary>
        /// <param name="navigationTerm">The navigation term.</param>
        /// <returns>A collection of parent terms, traversing upwards.</returns>
        IEnumerable<NavigationTerm> GetNavigationParentTerms(NavigationTerm navigationTerm);

        /// <summary>
        /// Generates the friendly URL slug with a default maximum length.
        /// </summary>
        /// <param name="phrase">The phrase.</param>
        /// <returns>A friendly URL slug containing human readable characters.</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Return value is just an URL fragment (a slug), not a full URL.")]
        string GenerateFriendlyUrlSlug(string phrase);

        /// <summary>
        /// Generates the friendly URL slug.
        /// </summary>
        /// <param name="phrase">The phrase.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>A friendly URL slug containing human readable characters.</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Return value is just an URL fragment (a slug), not a full URL.")]
        string GenerateFriendlyUrlSlug(string phrase, int maxLength);

        /// <summary>
        /// Set term driven page settings in the term store
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="termDrivenPageInfo">The term driven page setting info</param>
        void SetTermDrivenPageSettings(SPSite site, TermDrivenPageSettingInfo termDrivenPageInfo);

        /// <summary>
        /// Reset web navigation to its default configuration. Disabled the term set as mavigation term set.
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="settings">The managed navigation settings. Set null if you want to keep the associated termset unchanged</param>
        void ResetWebNavigationToDefault(SPWeb web, ManagedNavigationInfo settings);
    }
}