using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Security
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The read security.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "No other values allowed.")]
    public enum ReadSecurity
    {
        /// <summary>
        /// The user can read all items in the list
        /// </summary>
        AllItems = 1,

        /// <summary>
        /// The user can only read its own items in the list
        /// </summary>
        OnlyTheirOwn = 2
    }
}
