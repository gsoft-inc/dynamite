using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using GSoft.Dynamite.ServiceLocator;
using GSoft.Dynamite.WebConfig;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.Features.WebConfig_Modifications
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("fecdfc2c-bb05-43fa-9357-a25df41584ed")]
    public class WebConfig_ModificationsEventReceiver : SPFeatureReceiver
    {
        private const string RequestLifetimeWebConfigModificationOwner = "GSoftDynamite-RequestLifetimeHttpModule";

        private SPWebConfigModification AutofacRequestHttpModuleWebConfigModification
        {
            get
            {
                return new SPWebConfigModification()
                {
                    // The owner of the web.config modification, useful for removing a
                    // group of modifications
                    Owner = RequestLifetimeWebConfigModificationOwner,

                    // Make sure that the name is a unique XPath selector for the element
                    // we are adding. This name is used for removing the element
                    Name = "add[@name='RequestLifetimeHttpModule']",

                    // We are going to add a new XML node to web.config
                    Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,

                    // The XPath to the location of the parent node in web.config
                    Path = "configuration/system.webServer/modules",

                    // Sequence is important if there are multiple equal nodes that
                    // can't be identified with an XPath expression
                    Sequence = 0,

                    // The XML to insert as child node, make sure that used names match the Name selector
                    Value = "<add name=\"RequestLifetimeHttpModule\" type=\"" + typeof(RequestLifetimeHttpModule).AssemblyQualifiedName + "\" />"
                };
            }
        }

        /// <summary>
        /// The feature activated.
        /// </summary>
        /// <param name="properties">
        /// The properties.
        /// </param>
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            var webConfigModificationHelper = new WebConfigModificationHelper();
            var parent = properties.Feature.Parent as SPWebApplication;
            if (parent != null)
            {
                // Apply Web.config modifications
                webConfigModificationHelper.AddAndCleanWebConfigModification(
                    parent, 
                    new Collection<SPWebConfigModification>() 
                    { 
                        this.AutofacRequestHttpModuleWebConfigModification 
                    });
            }
        }

        /// <summary>
        /// The feature deactivating.
        /// </summary>
        /// <param name="properties">
        /// The properties.
        /// </param>
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            var webConfigModificationHelper = new WebConfigModificationHelper();
            var parent = properties.Feature.Parent as SPWebApplication;
            if (parent != null)
            {
                // Remove any changes by owner
                webConfigModificationHelper.RemoveExistingModificationsFromOwner(
                    parent, 
                    RequestLifetimeWebConfigModificationOwner);
            }
        }
    }
}
