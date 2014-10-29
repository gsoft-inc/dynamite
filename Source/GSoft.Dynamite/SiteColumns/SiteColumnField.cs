using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.SiteColumns
{
    /// <summary>
    /// Implementation of a Site Column Base
    /// </summary>
    [Obsolete]
    public class SiteColumnField : SiteColumnBase
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SiteColumnField()
        {
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public IList<string> DefaultValues { get; set; }
    }
}
