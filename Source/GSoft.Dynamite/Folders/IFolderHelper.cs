using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Folders
{
    /// <summary>
    /// Helper for SharePoint folders
    /// </summary>
    public interface IFolderHelper
    {
        /// <summary>
        /// Ensure a folder hierarchy in a library
        /// </summary>
        /// <param name="library">The library</param>
        /// <param name="folderInfo">The root folder of the library</param>
        void EnsureFolderHierarchy(SPList library, FolderInfo folderInfo);

        /// <summary>
        /// Method to revert to home page to the default web page
        /// </summary>
        /// <param name="web">The web</param>
        void ResetWelcomePageToDefault(SPWeb web);
    }
}
