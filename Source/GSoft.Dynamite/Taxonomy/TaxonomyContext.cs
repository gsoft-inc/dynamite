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
        TermStoreInfo TermStore { get; set; }

        TermGroupInfo Group { get; set; }

        TermSetInfo TermSet { get; set; }

        TermInfo TermSubset { get; set; }
    }
}
