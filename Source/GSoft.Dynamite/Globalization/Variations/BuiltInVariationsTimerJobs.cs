using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// Constants to help document the different Variations timer jobs.
    /// </summary>
    public static class BuiltInVariationsTimerJobs
    {
        /// <summary>
        /// Creates a complete variations hierarchy by provisioning all sites and pages from 
        /// a source label to a newly created target label language.
        /// By the time this timer job is complete, your target label item should have
        /// the value HierarchyIsCreated = TRUE.
        /// </summary>
        public const string VariationsCreateHierarchies = "VariationsCreateHierarchies";

        /// <summary>
        /// A slight misnomer: this timer job takes care of creating variated sites AND newly
        /// published pages in the target label sites.
        /// </summary>
        public const string VariationsSpawnSites = "VariationsSpawnSites";

        /// <summary>
        /// This timer job updates target pages when new versions are published (or force-updated)
        /// at the source.
        /// </summary>
        public const string VariationsPropagatePage = "VariationsPropagatePage";

        /// <summary>
        /// This job takes care of propagating list item updates.
        /// </summary>
        public const string VariationsPropagateListItem = "VariationsPropagateListItem";
    }
}
