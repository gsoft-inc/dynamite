using System;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Taxonomy
{
    public interface ITaxonomySessionManager
    {
        TaxonomySession GetSession(SPSite site);
        TaxonomySession RefreshTaxonomySessionCache(SPSite site);
    }
}
