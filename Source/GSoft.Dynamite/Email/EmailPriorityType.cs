using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSoft.Dynamite.Email
{
    /// <summary>
    /// Enumeration of the available priority levels when sending emails.
    /// </summary>
    public enum EmailPriorityType
    {
        /// <summary>
        /// Default value
        /// </summary>
        Normal = 0,

        /// <summary>
        /// High importance
        /// </summary>
        High = 1,

        /// <summary>
        /// Low importance
        /// </summary>
        Low = 2
    }
}
