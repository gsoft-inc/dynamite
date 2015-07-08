using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Taxonomy;

namespace GSoft.Dynamite.Sites
{
    /// <summary>
    /// Minimal information about the current site collection
    /// </summary>
    public interface ISiteCollectionContext
    {
        /// <summary>
        /// Unique ID of the site collecton
        /// </summary>
        Guid SiteId { get; }

        /// <summary>
        /// Default zone absolute URL of the site collection
        /// </summary>
        Uri SiteAbsoluteUrl { get; }

        /// <summary>
        /// The metadata of the default term store connected to the
        /// site collection.
        /// </summary>
        TermStoreInfo ContextTermStore { get; }
    }
}
