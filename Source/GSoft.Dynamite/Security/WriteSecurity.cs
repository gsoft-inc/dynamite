using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Security
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The write security.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Justification = "Not sure if this is a true Flags enum; depends on internals of SharePoint.")]
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "No other values allowed.")]
    public enum WriteSecurity
    {
        /// <summary>
        /// The user can edit all items in the list
        /// </summary>
        AllItems = 1,

        /// <summary>
        /// The user can only edit its own items in the list
        /// </summary>
        OnlyTheirOwn = 2,

        /// <summary>
        /// The user cannot edit any item in the list
        /// </summary>
        None = 4
    }
}
