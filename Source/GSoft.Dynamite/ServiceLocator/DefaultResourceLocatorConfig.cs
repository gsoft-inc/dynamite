using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Globalization;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Resource Locator
    /// </summary>
    public class DefaultResourceLocatorConfig : IResourceLocatorConfig
    {
        /// <summary>
        /// The keys for the resource files
        /// </summary>
        public string[] ResourceFileKeys
        {
            get 
            { 
                return new string[] { }; 
            }
        }
    }
}
