using System;
using System.Management.Automation;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Utils;
using Microsoft.Practices.Unity;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Variations
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Cmdlet for variations list sync
    /// </summary>
    [Cmdlet("Sync", "DSPList")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once InconsistentNaming
    public class DSPCmdletSyncList : Cmdlet
    {
        /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private VariationsHelper _variationHelper;

        /// <summary>
        /// Gets or sets the source web.
        /// </summary>
        [Parameter(Mandatory = true, 
        ValueFromPipeline = true, 
        HelpMessage = "The variation source web", 
        Position = 1)]
        public SPWeb SourceWeb { get; set; }

        /// <summary>
        /// Gets or sets the source list unique identifier.
        /// </summary>
        [Parameter(Mandatory = true, 
        ValueFromPipeline = true, 
        HelpMessage = "The source list guid", 
        Position = 1)]
        public Guid SourceListGuid { get; set; }

        /// <summary>
        /// Gets or sets the label to sync.
        /// </summary>
        [Parameter(Mandatory = true, 
        ValueFromPipeline = true, 
        HelpMessage = "The label to Sync", 
        Position = 1)]
        public string LabelToSync { get; set; }

        /// <summary>
        /// The end processing.
        /// </summary>
        protected override void EndProcessing()
        {
            this.ResolveDependencies();

            // Get the list
            var list = this.SourceWeb.Lists[this.SourceListGuid];

            this._variationHelper.SyncList(list, this.LabelToSync);

            base.EndProcessing();
        }

        /// <summary>
        /// Resolve Dependencies for helpers
        /// </summary>
        private void ResolveDependencies()
        {
            this._variationHelper = PowerShellContainer.Current.Resolve<VariationsHelper>();
        } 
    }
}
