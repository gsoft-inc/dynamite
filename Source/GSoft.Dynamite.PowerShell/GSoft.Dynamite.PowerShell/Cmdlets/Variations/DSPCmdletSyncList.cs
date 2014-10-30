using System;
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
    /// Cmdlet for variations list sync
    /// </summary>
    [Cmdlet("Sync", "DSPList")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class DSPCmdletSyncList : SPCmdlet
    {
        /// <summary>
        /// Gets or sets the source web.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The variation source web", Position = 1)]
        public SPWeb SourceWeb { get; set; }

        /// <summary>
        /// Gets or sets the source list unique identifier.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The source list guid", Position = 1)]
        public Guid SourceListGuid { get; set; }

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
            // Get the list
            var list = this.SourceWeb.Lists[this.SourceListGuid];

            if (list != null)
            {
                using (var childScope = PowerShellContainer.BeginLifetimeScope(list.ParentWeb))
                {
                    var variationHelper = childScope.Resolve<VariationSyncHelper>();
                    variationHelper.SyncList(list, this.LabelToSync);
                } 
            }

            base.InternalEndProcessing();
        }
    }
}
