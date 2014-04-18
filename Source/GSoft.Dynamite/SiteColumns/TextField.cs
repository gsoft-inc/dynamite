using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.SiteColumns
{
    /// <summary>
    /// Text field definition.
    /// </summary>
    public class TextField : SiteColumnField
    {
        /// <summary>
        /// Gets or sets a value indicating whether [is multiline].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is multiline]; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultiline { get; set; }
    }
}
