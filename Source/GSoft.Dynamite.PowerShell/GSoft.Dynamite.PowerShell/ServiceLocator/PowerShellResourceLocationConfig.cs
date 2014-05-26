using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Globalization;

namespace GSoft.Dynamite.PowerShell.ServiceLocator
{
    public class PowerShellResourceLocationConfig : IResourceLocatorConfig
    {
        public string[] ResourceFileKeys
        {
            get 
            {
                return new string[] { "GSoft.Dynamite" };
            }
        }
    }
}
