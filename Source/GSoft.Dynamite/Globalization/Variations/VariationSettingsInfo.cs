using System.Collections.Generic;

namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// Variations settings definition
    /// </summary>
    public class VariationSettingsInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public VariationSettingsInfo()
        {
            this.Labels = new List<VariationLabelInfo>();
        }

        /// <summary>
        /// Gets or sets the create hierarchies flag
        /// </summary>
        public string CreateHierarchies { get; set; }

        /// <summary>
        /// Enable ("true") or disable ("false") auto spawn
        /// </summary>
        public string EnableAutoSpawn { get; set; }

        /// <summary>
        /// Enable ("true") or disable ("false") auto spawn stop after delete
        /// </summary>
        public string AutoSpawnStopAfterDelete { get; set; }

        /// <summary>
        /// Enable ("true") or disable ("false") the web parts' update process
        /// </summary>
        public string UpdateWebParts { get; set; }

        /// <summary>
        /// Enable ("true") or disable ("false") the resources copy
        /// </summary>
        public string CopyResources { get; set; }

        /// <summary>
        /// Enable ("true") or disable ("false") the email notification
        /// </summary>
        public string SendNotificationEmail { get; set; }

        /// <summary>
        /// The source web template ("CMSPUBLISHING#0")
        /// </summary>
        public string SourceVarRootWebTemplate { get; set; }

        /// <summary>
        /// Supported language labels
        /// </summary>
        public IList<VariationLabelInfo> Labels { get; set; }
    }
}
