using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Definitions
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
    }
}
