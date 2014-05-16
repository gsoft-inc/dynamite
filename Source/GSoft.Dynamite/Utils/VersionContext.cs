using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Small utility to return a version string to append to files. In debug, we want a new version every time,
    /// in release, we use the assembly version.
    /// </summary>
    public static class VersionContext
    {
        /// <summary>
        /// In RELEASE mode: the current assembly's AssemblyFileVersion as a string.
        /// In DEBUG mode: a random GUID as a string.
        /// </summary>
        public static string CurrentVersionTag
        {
            get
            {
#if DEBUG
                // To help with debugging, Css or Script files should not be cached by the browser
                return Guid.NewGuid().ToString();
#else
                // Each release should force the clients to download a new version of the Css and Script files
                var assembly = Assembly.GetExecutingAssembly();
                var info = FileVersionInfo.GetVersionInfo(assembly.Location);
                return info.FileVersion;
#endif
            }
        }
    }
}
