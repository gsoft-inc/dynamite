using System;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Taxonomy
{
    public interface ISiteTaxonomyCacheManager
    {
        SiteTaxonomyCache GetSiteTaxonomyCache(SPSite site, string termStoreName);
        //TaxonomySession RefreshTaxonomySessionCache(SPSite site);
    }
}
