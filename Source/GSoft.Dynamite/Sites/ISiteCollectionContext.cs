using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
