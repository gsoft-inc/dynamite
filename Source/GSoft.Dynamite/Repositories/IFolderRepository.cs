using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Repositories
{
    /// <summary>
    /// The Folder Repository Interface
    /// </summary>
    public interface IFolderRepository
    {
        /// <summary>
        /// Method to get a folder by his Id
        /// </summary>
        /// <param name="folderId">the Id of the folder</param>
        /// <returns>The folder or throw an exception if not found</returns>
        SPFolder GetFolderById(int folderId);

        /// <summary>
        /// Method to get a folder by his Id
        /// </summary>
        /// <param name="web">Explicitly specify the SPWeb</param>
        /// <param name="folderId">the Id of the folder</param>
        /// <returns>The folder or throw an exception if not found</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        SPFolder GetFolderByIdForWeb(SPWeb web, int folderId);
    }
}