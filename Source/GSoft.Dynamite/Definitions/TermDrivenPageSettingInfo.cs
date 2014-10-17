namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition for Term/TermSet navigation settings
    /// </summary>
    public class TermDrivenPageSettingInfo
    {
        public TermSetInfo TermSet { get; private set; }
        public TermInfo Term { get; private set; }

        public bool IsTermSet { get; private set; }
        public bool IsTerm { get; private set; }

        public string TargetUrl { get; private set; }
        public string TargetUrlForChildTerms { get; private set; }
        public string CatalogTargetUrl { get;  private set; }
        public string CatalogTargetUrlForChildTerms { get; private set; }

        public bool ExcludeFromGlobalNavigation { get; private set; }
        public bool ExcludeFromCurrentNavigation { get; private set; }

        public string SimpleLinkOrHeader { get; private set; }
        public bool IsSimpleLinkOrHeader { get; private set; }

        public TermDrivenPageSettingInfo(TermSetInfo termSet, string targetUrlForChildTerms,
            string catalogTargetUrlForChildTerms)
        {
            this.TermSet = termSet;
            this.TargetUrlForChildTerms = targetUrlForChildTerms;
            this.CatalogTargetUrlForChildTerms = catalogTargetUrlForChildTerms;
            this.IsTermSet = true;
            this.IsTerm = false;
        }

        public TermDrivenPageSettingInfo(TermInfo term, string targetUrl, string catalogTargetUrl,
            string targetUrlForChildTerms, string catalogTargetUrlForChildTerms, bool excludeFromGlobalNav, bool excludeFromCurrentNav)
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

        public TermDrivenPageSettingInfo(TermInfo term, string simpleLinkOrHeader)
        {
            this.Term = term;
            this.IsTerm = true;
            this.IsTermSet = false;
            this.IsSimpleLinkOrHeader = true;
            this.SimpleLinkOrHeader = simpleLinkOrHeader;
        }

    }
}
