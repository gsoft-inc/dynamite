using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Utils;
using Microsoft.Practices.Unity;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Variations
{
    /// <summary>
    /// Cmdlet for variations web sync
    /// </summary>
    [Cmdlet("Sync", "DSPWeb")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once InconsistentNaming
    public class DSPCmdletSyncWeb : Cmdlet
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

           this.WriteWarning("Sync SPWeb " + this.SourceWeb.Url + " to the " + this.LabelToSync.ToUpper() + " variation label...");

           this._variationHelper.SyncWeb(this.SourceWeb, this.LabelToSync);

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
