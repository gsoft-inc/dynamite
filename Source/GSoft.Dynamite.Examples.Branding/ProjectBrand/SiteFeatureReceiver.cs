using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Examples.Branding.ProjectBrand
{
    /// <summary>
    /// Branding feature reciever
    /// </summary>
    public class SiteFeatureReceiver : SPFeatureReceiver
    {
        /// <summary>
        /// Branding feature activation handler
        /// </summary>
        /// <param name="properties">The context</param>
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPSite siteCollection = (SPSite)properties.Feature.Parent;
            SPFeatureProperty masterUrlProperty = properties.Feature.Properties["MasterPage"];
            string masterUrl = masterUrlProperty.Value;

            if (string.IsNullOrEmpty(masterUrl) == false)
            {
                masterUrl = SPUrlUtility.CombineUrl(siteCollection.ServerRelativeUrl, "_catalogs/masterpage/" + masterUrl);

                foreach (SPWeb site in siteCollection.AllWebs)
                {
                    site.MasterUrl = masterUrl;
                    site.CustomMasterUrl = masterUrl;
                    site.Update();
                }
            }
        }

        /// <summary>
        /// Branding feature deactivation handler
        /// </summary>
        /// <param name="properties">The context</param>
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPSite siteCollection = (SPSite)properties.Feature.Parent;
            string masterUrl = SPUrlUtility.CombineUrl(siteCollection.ServerRelativeUrl, "_catalogs/masterpage/v4.master");

            foreach (SPWeb site in siteCollection.AllWebs)
            {
                site.MasterUrl = masterUrl;
                site.CustomMasterUrl = masterUrl;
                site.Update();
            }
        }
    }
}
