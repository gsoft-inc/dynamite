using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Globalization;

namespace GSoft.Dynamite.PowerShell.ServiceLocator
{
    /// <summary>
    /// PowerShell Resource location configuration file
    /// </summary>
    public class PowerShellResourceLocationConfig : IResourceLocatorConfig
    {
        /// <summary>
        /// The resource file keys for PowerShell
        /// </summary>
        public string[] ResourceFileKeys
        {
            get 
            {
                return new string[] { "GSoft.Dynamite" };
            }
        }
    }
}
