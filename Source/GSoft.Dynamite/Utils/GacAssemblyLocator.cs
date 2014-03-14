using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSoft.Dynamite.Utils
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// The Gac assembly locator.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class GacAssemblyLocator
    {
        private const string FolderPath = @"c:\windows\assembly";

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
        public IList<Assembly> GetAssemblies(IList<string> gacFolders, Func<string, bool> assemblyNameCondition)
        {
            return this.GetAssemblies(gacFolders, assemblyNameCondition, null);
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
        public IList<Assembly> GetAssemblies(IList<string> gacFolders, Func<string, bool> assemblyNameCondition, Func<string, bool> assemblyVersionCondition)
        {
            var assemblyList = new List<Assembly>();

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
                            this.ProcessFolder(assemblyFolder, assemblyNameCondition, assemblyVersionCondition, assemblyList);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // this.logger.Fatal(string.Format("{0} : {1}", err.Message, err.StackTrace));
            }

            return assemblyList;
        }

        private void ProcessFile(string file, Func<string, bool> assemblyVersionCondition, IList<Assembly> assemblyList)
        {
            try
            {
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
            catch (Exception)
            {
                /* do nothing, just log a warning */

                // this.logger.Warn(string.Format("{0} : {1}", err.Message, err.StackTrace));
            }
        }

        private void ProcessFolder(string folder, Func<string, bool> assemblyNameCondition, Func<string, bool> assemblyVersionCondition, IList<Assembly> assemblyList)
        {
            // apply condition here
            foreach (string file in Directory.GetFiles(folder).Where(assemblyNameCondition))
            {
                this.ProcessFile(file, assemblyVersionCondition, assemblyList);
            }

            foreach (string subFolder in Directory.GetDirectories(folder))
            {
                this.ProcessFolder(subFolder, assemblyNameCondition, assemblyVersionCondition, assemblyList);
            }
        }
    }
}
