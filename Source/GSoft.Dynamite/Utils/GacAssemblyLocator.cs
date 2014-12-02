using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSoft.Dynamite.Utils
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using GSoft.Dynamite.Logging;

    /// <summary>
    /// The Gac assembly locator.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static class GacAssemblyLocator
    {
        private const string FolderPath = @"C:\Windows\Microsoft.NET\assembly\";

        /// <summary>
        /// Returns assemblies found in the c:\windows\assembly directory
        /// </summary>
        /// <remarks>
        /// This method will match Assemblies from any version
        /// </remarks>
        /// <param name="gacFolders">
        /// The gac Folders.
        /// </param>
        /// <param name="assemblyNameCondition">
        /// A function to filter assembly names with (as string comparison)
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static IList<Assembly> GetAssemblies(IList<string> gacFolders, Func<string, bool> assemblyNameCondition)
        {
            return GetAssemblies(gacFolders, assemblyNameCondition, null);
        }

        /// <summary>
        /// Returns assemblies found in the c:\windows\assembly directory
        /// </summary>
        /// <param name="gacFolders">
        /// The gac Folders.
        /// </param>
        /// <param name="assemblyNameCondition">
        /// A function to filter assembly names with (as string comparison)
        /// </param>
        /// <param name="assemblyVersionCondition">
        /// A function to filter assembly version with (as string comparison)
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We really wish to ignore assembly scanning failures.")]
        public static IList<Assembly> GetAssemblies(IList<string> gacFolders, Func<string, bool> assemblyNameCondition, Func<string, bool> assemblyVersionCondition)
        {
            var assemblyList = new List<Assembly>();
            var logger = new TraceLogger("GSoft.Dynamite", "GSoft.Dynamite", false);

            try
            {
                foreach (string folder in gacFolders)
                {
                    var path = Path.Combine(FolderPath, folder);
                    if (Directory.Exists(path))
                    {
                        string[] assemblyFolders = Directory.GetDirectories(path);

                        foreach (string assemblyFolder in assemblyFolders)
                        {
                            ProcessFolder(assemblyFolder, assemblyNameCondition, assemblyVersionCondition, assemblyList, logger);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                logger.Fatal(string.Format(CultureInfo.InvariantCulture, "GACAssemblyLocator failed to scan GAC. Error: {0}", error.ToString()));
            }

            return assemblyList;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We really wish to ignore assembly scanning failures.")]
        private static void ProcessFile(string file, Func<string, bool> assemblyVersionCondition, IList<Assembly> assemblyList, ILogger logger)
        {
            try
            {
                logger.Info(string.Format(CultureInfo.InvariantCulture, "GACAssemblyLocator filename match! Loading assembly {0}.", file));
                Assembly a = Assembly.LoadFile(file);

                if (assemblyVersionCondition != null)
                {
                    if (assemblyVersionCondition(a.FullName))
                    {
                        assemblyList.Add(a);
                    }
                }
                else
                {
                    // If no condition is specified, accept DLLs from all versions
                    assemblyList.Add(a);
                }
            }
            catch (Exception error)
            {
                logger.Fatal(string.Format(CultureInfo.InvariantCulture, "GACAssemblyLocator failed to load assembly (file {0}). Error: {1}", file, error.ToString()));
            }
        }

        private static void ProcessFolder(string folder, Func<string, bool> assemblyNameCondition, Func<string, bool> assemblyVersionCondition, IList<Assembly> assemblyList, ILogger logger)
        {
            var filesPathsMatchingAssemblyNameCondition = Directory.GetFiles(folder).Where(assemblyNameCondition);

            foreach (string file in filesPathsMatchingAssemblyNameCondition)
            {
                ProcessFile(file, assemblyVersionCondition, assemblyList, logger);
            }

            foreach (string subFolder in Directory.GetDirectories(folder))
            {
                ProcessFolder(subFolder, assemblyNameCondition, assemblyVersionCondition, assemblyList, logger);
            }
        }
    }
}
