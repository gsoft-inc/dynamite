using System;

namespace GSoft.Dynamite.Examples.Core.Utilities
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class VersionContext
    {
        /// <summary>
        /// In RELEASE mode: the current assembly's AssemblyFileVersion as a string.
        /// In DEBUG mode: a random Guid as a string.
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
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
                    return info.FileVersion;
#endif
            }
        }
    }
}
