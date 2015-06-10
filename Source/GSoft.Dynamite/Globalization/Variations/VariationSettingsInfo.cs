using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
            this.SourceVariationTopLevelWebTemplate = "CMSPUBLISHING#0";
            this.IsAutomaticTargetPageCreation = true;
            this.IsUpdateTargetPageWebParts = true;
        }

        /// <summary>
        /// If true, when a new page is published at the source, then a new page 
        /// will be automatically created in all target webs. This is the "Create
        /// Everywhere" option under _layouts/15/VariationSettings.aspx.
        /// If false, then the contributor will have to choose specifically which
        /// target languages should get a copy of the content. This is the "Create
        /// Selectively" option.
        /// This will be mapped to the "EnableAutoSpawnPropertyName" property bag
        /// value on the root folder of the variations relationships list.
        /// This value is true by default (i.e. the first major version being
        /// published will always lead to a draft appearing in all target webs).
        /// </summary>
        public bool IsAutomaticTargetPageCreation { get; set; }

        /// <summary>
        /// If true, when a source page is re-published after its target page was 
        /// deleted, then the target page will be re-created.
        /// If false, then re-publishing a source page will not cause previously
        /// deleted target pages to get created.
        /// The OPPOSITE of this value will be mapped to the "AutoSpawnStopAfterDeletePropertyName" 
        /// property bag value on the root folder of the variations relationships list.
        /// This value is false by default (i.e. target page deletions become
        /// permanent, regardless of subsequent source major version publish events).
        /// </summary>
        public bool IsRecreateDeletedTargetPage { get; set; }

        /// <summary>
        /// If true, changes to web parts on the source page will be propagated to their
        /// corresponding "sister" web parts on the target page along with content updates.
        /// If false, web parts on the target pages will be left alone during update
        /// propagation.
        /// This will be mapped to the "UpdateWebPartsPropertyName" property bag
        /// value on the root folder of the variations relationships list.
        /// This value is true by default (i.e. updates to web part properties will
        /// be propagated whenever the source page updates get propagated).
        /// </summary>
        public bool IsUpdateTargetPageWebParts { get; set; }

        /// <summary>
        /// If true, new page variations will come with a copy of the resources used
        /// on the original page.
        /// If false, the target pages will refer to the original locations of their
        /// source's resources.
        /// This will be mapped to the "CopyResourcesPropertyName" property bag
        /// value on the root folder of the variations relationships list.
        /// This value is false by default and doesn't even appear in the
        /// VariationsSettings page anymore in SharePoint 2013 (it used to
        /// be available in SP2010).
        /// </summary>
        public bool IsCopyResourcesToTarget { get; set; }

        /// <summary>
        /// If true, email notifications will be sent to site and page contacts when a new site
        /// or page is created or when a page is updated by the variations system.
        /// We initialize this value as false by default (through click programming, SharePoint usually
        /// turns this on).
        /// </summary>
        public bool IsSendNotificationEmail { get; set; }

        /// <summary>
        /// The web template to use when creating the top level source web. 
        /// Default is "CMSPUBLISHING#0".
        /// </summary>
        public string SourceVariationTopLevelWebTemplate { get; set; }

        /// <summary>
        /// Supported language labels. At least (and at most) one of these should be the 
        /// source label (IsSource == true).
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable more flexile object initialization.")]
        public ICollection<VariationLabelInfo> Labels { get; set; }
    }
}
