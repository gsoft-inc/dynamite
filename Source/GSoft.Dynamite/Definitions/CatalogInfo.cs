using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Lists;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Definitions
{
    public class CatalogInfo : ListInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CatalogInfo()
        {    
        }

        public TaxonomyFieldInfo TaxonomyFieldMap { get; set; }

        public IList<ManagedPropertyInfo> ManagedProperties { get; set; }
    }
}
