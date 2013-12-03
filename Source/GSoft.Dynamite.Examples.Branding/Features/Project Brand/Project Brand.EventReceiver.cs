using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Examples.Branding.Features.Project_Brand
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("464f2971-bf2a-409f-8b2f-d1da79dbd9bc")]
    public class Project_BrandEventReceiver : SPFeatureReceiver
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
