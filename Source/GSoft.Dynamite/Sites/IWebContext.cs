using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Sites
{
    /// <summary>
    /// Minimal information about the current web
    /// </summary>
    public interface IWebContext
    {
        /// <summary>
        /// Unique ID of the site
        /// </summary>
        Guid WebId { get; }

        /// <summary>
        /// Absolute URL of the site
        /// </summary>
        Uri WebAbsoluteUrl { get; }
    }
}
