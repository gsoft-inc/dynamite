using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using Autofac;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Helpers;
using GSoft.Dynamite.PowerShell.Unity;
using Microsoft.SharePoint;
using Microsoft.SharePoint.PowerShell;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Variations
{
    /// <summary>
    /// Cmdlet for variations web sync
    /// </summary>
    [Cmdlet("Sync", "DSPWeb")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class DSPCmdletSyncWeb : SPCmdlet
    {
        /// <summary>
        /// Gets or sets the source web.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The variation source web", Position = 1)]
        public SPWeb SourceWeb { get; set; }

        /// <summary>
        /// Gets or sets the label to sync.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The label to Sync", Position = 1)]
        public string LabelToSync { get; set; }

        /// <summary>
        /// The end processing.
        /// </summary>
        protected override void InternalEndProcessing()
        {
            this.WriteWarning("Sync SPWeb " + this.SourceWeb.Url + " to the " + this.LabelToSync.ToUpper() + " variation label...");

            using (var childScope = PowerShellContainer.BeginLifetimeScope(this.SourceWeb))
            {
                var variationHelper = childScope.Resolve<IVariationHelper>();
                variationHelper.SyncWeb(this.SourceWeb, this.LabelToSync);
            }

            base.InternalEndProcessing();
        }
    }
}
