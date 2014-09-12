using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Globalization;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Base definition for a SharePoint structural entity (list, field, content type, web and site)
    /// </summary>
    public class BaseTypeInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseTypeInfo()
        {       
        }

        /// <summary>
        /// The display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The description 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Title by languages (LCID) for the list
        /// </summary>
        public IDictionary<CultureInfo, string> TitleResources { get; set; }

        /// <summary>
        /// Description by languages (LCID) for the list
        /// </summary>
        public IDictionary<CultureInfo, string> DescriptionResources { get; set; }
    }
}
