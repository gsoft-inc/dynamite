using GSoft.Dynamite.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Taxonomy
{
    public class TaxonomyContext
    {
        /// <summary>
        /// Context's term group. Assume Default Farm Term Store if this property is null.
        /// </summary>
        public TermStoreInfo TermStore { get; set; }

        /// <summary>
        /// Context's term group. Assume Default Site Collection Term Group if this property is null.
        /// </summary>
        public TermGroupInfo Group { get; set; }

        /// <summary>
        /// Context's term set.
        /// </summary>
        public TermSetInfo TermSet { get; set; }

        /// <summary>
        /// Terms that limits the choices of term options further.
        /// Assume that field is bound to term set if this property is null.
        /// </summary>
        public TermInfo TermSubset { get; set; }
    }
}
