using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// Enumeration
    /// </summary>
    public enum WriteSecurityOptions
    {
        /// <summary>
        /// All users can modify all items.
        /// </summary>
        AllUser = 1,

        /// <summary>
        /// Users can modify only items that they create.
        /// </summary>
        OwnerOnly = 2,

        /// <summary>
        /// Users cannot modify any list item.
        /// </summary>
        Nobody = 4
    }
}
