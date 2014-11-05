using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Taxonomy;

namespace GSoft.Dynamite.Pages
{
    /// <summary>
    /// Definition for Term/TermSet navigation settings
    /// </summary>
    public class TermDrivenPageSettingInfo
    {
        /// <summary>
        /// Initializes a new <see cref="TermDrivenPageSettingInfo"/> instance
        /// </summary>
        /// <param name="termSet">The term set</param>
        /// <param name="targetUrlForChildTerms">The target page for child terms</param>
        /// <param name="catalogTargetUrlForChildTerms">The target catalog page for child terms</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "Cross-site publishing term target URLs should be stored as strings because they may include magic SharePoint tokens such as ~site or ~sitecollection.")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "Cross-site publishing term target URLs should be stored as strings because they may include magic SharePoint tokens such as ~site or ~sitecollection.")]
        public TermDrivenPageSettingInfo(TermSetInfo termSet, string targetUrlForChildTerms, string catalogTargetUrlForChildTerms)
        {
            this.TermSet = termSet;
            this.TargetUrlForChildTerms = targetUrlForChildTerms;
            this.CatalogTargetUrlForChildTerms = catalogTargetUrlForChildTerms;
            this.IsTermSet = true;
            this.IsTerm = false;
        }

        /// <summary>
        /// Initializes a new <see cref="TermDrivenPageSettingInfo"/> instance
        /// </summary>
        /// <param name="term">The target term</param>
        /// <param name="targetUrl">The destination URL</param>
        /// <param name="catalogTargetUrl">The catalog destination URL</param>
        /// <param name="targetUrlForChildTerms">The target URL for child terms</param>
        /// <param name="catalogTargetUrlForChildTerms">The catalog target URL for child terms</param>
        /// <param name="excludeFromGlobalNav">Whether the term should be excluded from global navigation</param>
        /// <param name="excludeFromCurrentNav">Whether the term should be excluded from current navigation</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "Cross-site publishing term target URLs should be stored as strings because they may include magic SharePoint tokens such as ~site or ~sitecollection.")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "Cross-site publishing term target URLs should be stored as strings because they may include magic SharePoint tokens such as ~site or ~sitecollection.")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "3#", Justification = "Cross-site publishing term target URLs should be stored as strings because they may include magic SharePoint tokens such as ~site or ~sitecollection.")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "4#", Justification = "Cross-site publishing term target URLs should be stored as strings because they may include magic SharePoint tokens such as ~site or ~sitecollection.")]
        public TermDrivenPageSettingInfo(
            TermInfo term, 
            string targetUrl, 
            string catalogTargetUrl,
            string targetUrlForChildTerms, 
            string catalogTargetUrlForChildTerms, 
            bool excludeFromGlobalNav, 
            bool excludeFromCurrentNav)
        {
            this.TargetUrlForChildTerms = targetUrlForChildTerms;
            this.CatalogTargetUrlForChildTerms = catalogTargetUrlForChildTerms;
            this.Term = term;
            this.TargetUrl = targetUrl;
            this.CatalogTargetUrl = catalogTargetUrl;
            this.IsTermSet = false;
            this.IsTerm = true;
            this.ExcludeFromGlobalNavigation = excludeFromGlobalNav;
            this.ExcludeFromCurrentNavigation = excludeFromCurrentNav;
            this.IsSimpleLinkOrHeader = false;
        }

        /// <summary>
        /// Initializes a new <see cref="TermDrivenPageSettingInfo"/> instance
        /// </summary>
        /// <param name="term">The target term</param>
        /// <param name="simpleLinkOrHeader">The simple link or header metadata</param>
        public TermDrivenPageSettingInfo(TermInfo term, string simpleLinkOrHeader)
        {
            this.Term = term;
            this.IsTerm = true;
            this.IsTermSet = false;
            this.IsSimpleLinkOrHeader = true;
            this.SimpleLinkOrHeader = simpleLinkOrHeader;
        }

        /// <summary>
        /// Term set of the page's term
        /// </summary>
        public TermSetInfo TermSet { get; private set; }

        /// <summary>
        /// The page's associated term
        /// </summary>
        public TermInfo Term { get; private set; }

        /// <summary>
        /// Was defined with a term set
        /// </summary>
        public bool IsTermSet { get; private set; }

        /// <summary>
        /// Was defined with a term
        /// </summary>
        public bool IsTerm { get; private set; }

        /// <summary>
        /// Target navigation URL for items tagged with the current term
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Cross-site publishing term target URLs should be stored as strings because they may include magic SharePoint tokens such as ~site or ~sitecollection.")]
        public string TargetUrl { get; private set; }

        /// <summary>
        /// Target navigation URL for all items tagged with child terms of the current term
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Cross-site publishing term target URLs should be stored as strings because they may include magic SharePoint tokens such as ~site or ~sitecollection.")]
        public string TargetUrlForChildTerms { get; private set; }

        /// <summary>
        /// The catalog target URL
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Cross-site publishing term target URLs should be stored as strings because they may include magic SharePoint tokens such as ~site or ~sitecollection.")]
        public string CatalogTargetUrl { get; private set; }

        /// <summary>
        /// The catalog child terms target URL
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Cross-site publishing term target URLs should be stored as strings because they may include magic SharePoint tokens such as ~site or ~sitecollection.")]
        public string CatalogTargetUrlForChildTerms { get; private set; }

        /// <summary>
        /// Whether term should be excluded from global navigation
        /// </summary>
        public bool ExcludeFromGlobalNavigation { get; private set; }

        /// <summary>
        /// Whether term should be excluded from current navigation
        /// </summary>
        public bool ExcludeFromCurrentNavigation { get; private set; }

        /// <summary>
        /// Simple link or header metadata
        /// </summary>
        public string SimpleLinkOrHeader { get; private set; }

        /// <summary>
        /// Whether simple link or header metadata is provided
        /// </summary>
        public bool IsSimpleLinkOrHeader { get; private set; }
    }
}
