using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Formats supported by <see cref="DateTimeFieldInfo"/>
    /// </summary>
    public enum DateTimeFieldFormat
    {
        /// <summary>
        /// Default setting, keeps only track of date (not time)
        /// </summary>
        DateOnly,

        /// <summary>
        /// Includes time in field value
        /// </summary>
        DateTime        
    }
}
