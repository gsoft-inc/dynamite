using System;
using System.Collections.Generic;
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
        public IList<ListInfo> Lists { get; set; }

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
