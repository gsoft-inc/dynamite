using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using GSoft.Dynamite.Navigation;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using GSoft.Dynamite.Utils;
using Microsoft.Practices.Unity;
using Microsoft.SharePoint;
using System;
using Microsoft.SharePoint.Publishing;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Variations
{
    /// <summary>
    /// Cmdlet for variations web sync
    /// </summary>
    [Cmdlet("Sync", "DSPWeb")]
    public class DSPCmdletSyncWeb : Cmdlet
    {
        /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private VariationsHelper _variationHelper;

        [Parameter(Mandatory = true,
        ValueFromPipeline = true,
        HelpMessage = "The variation source web",
        Position = 1)]
        public SPWeb SourceWeb { get; set; }

        [Parameter(Mandatory = true,
        ValueFromPipeline = true,
        HelpMessage = "The label to Sync",
        Position = 1)]
        public string LabelToSync { get; set; }

        protected override void EndProcessing()
        {
            this.ResolveDependencies();

            WriteWarning("Sync SPWeb " + SourceWeb.Url + " to the " + LabelToSync.ToUpper() + " variation label...");

            _variationHelper.SyncWeb(SourceWeb, LabelToSync);

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
