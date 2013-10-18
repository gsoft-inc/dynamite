using Microsoft.SharePoint;

namespace GSoft.Dynamite.Examples.Branding.ProjectBrand.WebEventReceiver
{
    /// <summary>
    /// Web-level Branding event receiver that changes the site's master page
    /// </summary>
    public class WebEventReceiver : SPWebEventReceiver
    {
        /// <summary>
        /// Fires when a new web is created and changes the site's main and custom master page
        /// </summary>
        /// <param name="properties">The context</param>
        public override void WebProvisioned(SPWebEventProperties properties)
        {
            SPWeb site = properties.Web;
            SPWeb rootSite = site.Site.RootWeb;
            site.MasterUrl = rootSite.MasterUrl;
            site.CustomMasterUrl = rootSite.CustomMasterUrl;
            site.Update();
        }
    }
}
