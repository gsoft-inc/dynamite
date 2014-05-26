using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Globalization;

namespace GSoft.Dynamite.ServiceLocator
{
    public class DefaultResourceLocatorConfig : IResourceLocatorConfig
    {
        public string[] ResourceFileKeys
        {
            get 
            { 
                return new string[] { }; 
            }
        }
    }
}
