using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Lists;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Sites
{
    /// <summary>
    /// Definition for a web
    /// </summary>
    public class WebInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public WebInfo()
        {
            this.Lists = new List<ListInfo>();
        }

        /// <summary>
        /// Lists of the web
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable more flexile object initialization.")]
        public ICollection<ListInfo> Lists { get; set; }

        /// <summary>
        /// The web's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The relative-to-parent path to the web
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The web template to be applied
        /// </summary>
        public SPWebTemplate Template { get; set; }
    }
}
