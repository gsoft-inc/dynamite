using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Definitions
{
    public class PageLayoutInfo
    {
        /// <summary>
        /// Name of the Page Layout
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Names of the zones in the page layout
        /// </summary>
        public string[] ZoneNames { get; set; }

        /// <summary>
        /// The associated content type id
        /// </summary>
        public string AssociatedContentTypeId { get; set; }
    
    }
}
