namespace GSoft.Dynamite.Setup
{
    using Microsoft.SharePoint;

    /// <summary>
    /// Used to recursively build SPFolders and publishing pages from metadata in the Pages library
    /// </summary>
    public interface IFolderMaker
    {
        /// <summary>
        /// Builds a folder hierarchy in a Pages library
        /// </summary>
        /// <param name="library">The Pages library</param>
        /// <param name="rootFolderInfo">The metadata for initializing the folder at the root of the library</param>
        void Make(SPList library, IFolderInfo rootFolderInfo);
    }
}