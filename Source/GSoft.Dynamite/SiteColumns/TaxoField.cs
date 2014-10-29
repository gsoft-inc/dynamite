using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.SiteColumns
{
    /// <summary>
    /// Taxonomy field definition.
    /// </summary>
    [Obsolete]
    public class TaxoField : SiteColumnField
    {
        /// <summary>
        /// Gets or sets a value indicating whether [is multiple].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is multiple]; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultiple { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is open].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is open]; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Gets or sets the name of the term set group.
        /// </summary>
        /// <value>
        /// The name of the term set group.
        /// </value>
        public string TermSetGroupName { get; set; }

        /// <summary>
        /// Gets or sets the name of the term set.
        /// </summary>
        /// <value>
        /// The name of the term set.
        /// </value>
        public string TermSetName { get; set; }

        /// <summary>
        /// Gets or sets the name of the term subset.
        /// </summary>
        /// <value>
        /// The name of the term subset.
        /// </value>
        public string TermSubsetName { get; set; }
    }
}
