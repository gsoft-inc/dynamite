using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Lists;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// Forces the synchronization (i.e. copy from source label to destination language) 
    /// of lists and webs in the variations process
    /// </summary>
    public interface IVariationSyncHelper
    {
        /// <summary>
        /// Sync a SPList for multiple target labels
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="listInfo">The source list metadata</param>
        /// <param name="labels">The destination labels</param>
        void SyncList(SPWeb web, ListInfo listInfo, IList<VariationLabelInfo> labels);

        /// <summary>
        /// Sync a SPList for a target label
        /// </summary>
        /// <param name="listToSync">The source SPList instance to sync.</param>
        /// <param name="labelToSync">The label name to Sync. example: <c>"en"</c> or <c>"fr"</c>.</param>
        void SyncList(SPList listToSync, string labelToSync);

        /// <summary>
        /// Sync a SPList for multiple target labels
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="labels">Variations labels</param>
        void SyncWeb(SPWeb web, IList<VariationLabelInfo> labels);

        /// <summary>
        /// Sync a SPWeb with variations
        /// </summary>
        /// <param name="web">The source web instance to sync.</param>
        /// <param name="labelToSync">Source label to sync</param>
        void SyncWeb(SPWeb web, string labelToSync);
    }
}
