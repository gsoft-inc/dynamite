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
        /// Ensure a folder hierarchy in a list or library
        /// </summary>
        /// <param name="list">The SharePoint list or library</param>
        /// <param name="rootFolderInfo">The metadata that should define the root folder of the list</param>
        /// <returns>The list's root folder instance</returns>
        SPFolder EnsureFolderHierarchy(SPList list, FolderInfo rootFolderInfo);

        /// <summary>
        /// Method to revert to home page to the default web page
        /// </summary>
        /// <param name="web">The web</param>
        void ResetWelcomePageToDefault(SPWeb web);
    }
}
