using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Files
{
    /// <summary>
    /// Interface for the File Helper, an expert responsible for file handling like file creation in list
    /// </summary>
    public interface IFileHelper
    {
        /// <summary>
        /// Method to ensure a file in a document library. Will create if not exist.
        /// </summary>
        /// <param name="web">The web where the list or library is</param>
        /// <param name="listTitle">The list title where to ensure the file</param>
        /// <param name="file">The fileinfo containing all the information needed to create the file</param>
        /// <returns>The file</returns>
        SPFile EnsureFile(SPWeb web, string listTitle, FileInfo file);
    }
}
