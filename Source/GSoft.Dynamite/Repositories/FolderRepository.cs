using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using GSoft.Dynamite.Extensions;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Repositories
{
    /// <summary>
    /// Class to interact with folders
    /// </summary>
    public class FolderRepository : IFolderRepository
    {
        /// <summary>
        /// Constructor for <see cref="FolderRepository"/>
        /// </summary>
        public FolderRepository()
        {
        }

        /// <summary>
        /// Method to get a folder by his Id
        /// </summary>
        /// <param name="folderId">the Id of the folder</param>
        /// <returns>The folder or throw an exception if not found</returns>
        public SPFolder GetFolderById(int folderId)
        {
            return this.GetFolderByIdForWeb(SPContext.Current.Web, folderId);
        }

        /// <summary>
        /// Method to get a folder by his Id
        /// </summary>
        /// <param name="web">Explicitly specify the SPWeb</param>
        /// <param name="folderId">the Id of the folder</param>
        /// <returns>The folder or throw an exception if not found</returns>
        public SPFolder GetFolderByIdForWeb(SPWeb web, int folderId)
        {
            SPListItem item = web.GetPagesLibrary().GetItemById(folderId);

            if (item == null || item.Folder == null)
            {
                throw new KeyNotFoundException(string.Format(CultureInfo.InvariantCulture, "Folder key <{0}> was not found.", folderId));
            }

            return item.Folder;
        }
    }
}
