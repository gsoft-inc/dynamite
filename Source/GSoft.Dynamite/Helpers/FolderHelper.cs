using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Logging;
using Microsoft.Office.Server.Search.Internal.UI;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Portal.WebControls;

namespace GSoft.Dynamite.Helpers
{
    /// <summary>
    /// Helper class for SharePoint folders
    /// </summary>
    public class FolderHelper
    {
        private readonly ILogger _logger;
        private readonly PageHelper _pageHelper;

        /// <summary>
        /// Constructor for FolderHelper
        /// </summary>
        /// <param name="logger">The logger helper instance</param>
        /// <param name="pageHelper">The page helper instance</param>
        public FolderHelper(ILogger logger, PageHelper pageHelper)
        {
            this._logger = logger;
            this._pageHelper = pageHelper;
        }

        /// <summary>
        /// Ensure a folder hierarchy in a library
        /// </summary>
        /// <param name="library">The library</param>
        /// <param name="folderInfo">The root folder of the library</param>
        public void EnsureFolderHierarchy(SPList library, FolderInfo folderInfo)
        {
            if (folderInfo.IsRootFolder)
            {
                this.EnsureFolder(library, null, folderInfo);
            }
        }

        /// <summary>
        /// Ensure a folder a its sub folders
        /// </summary>
        /// <param name="library">The library</param>
        /// <param name="parentFolder">The parent folder</param>
        /// <param name="folderInfo">The current folder information</param>
        private void EnsureFolder(SPList library, SPFolder parentFolder, FolderInfo folderInfo)
        {
            SPFolder folder = null;

            if (parentFolder == null)
            {
                // We are on at the root folder of the library (i.e. no parent folder).
                // Initialize defaults and pages in here, then move on to subfolders (instead of trying to create the folder)
                folder = library.RootFolder;
                
                // Create pages
                this._pageHelper.EnsurePage(library, folder, folderInfo.Pages);

                // Create sub folders
                foreach (var childFolder in folderInfo.SubFolders)
                {
                    this.EnsureFolder(library, folder, childFolder);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(folderInfo.Name))
                {
                    try
                    {
                        folder = parentFolder.SubFolders[folderInfo.Name];
                        this._logger.Info("Skipping folder creation for " + folderInfo.Name +
                                         " because it already exists (will still apply values and default metadata)");
                    }
                    catch (ArgumentException)
                    {
                        this._logger.Info("Creating folder " + folderInfo.Name);
                    }

                    if (folder == null)
                    {
                        // Add the folder (if it doesn't already exist)
                        folder = parentFolder.SubFolders.Add(folderInfo.Name);
                    }

                    // Create pages
                    this._pageHelper.EnsurePage(library, folder, folderInfo.Pages);
                }

                // Create sub folders
                foreach (var childFolder in folderInfo.SubFolders)
                {
                    this.EnsureFolder(library, folder, childFolder);
                }
            }
        }
    }
}
