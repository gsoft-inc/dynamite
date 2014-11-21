using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Lists;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Files
{
    /// <summary>
    /// FileHelper is a Helper class to handle file creation on document library
    /// </summary>
    public class FileHelper : IFileHelper
    {
        private readonly IListLocator listLocator;

        /// <summary>
        /// Default constructor 
        /// </summary>
        /// <param name="listLocator">The expert to locate a List object</param>
        public FileHelper(IListLocator listLocator)
        {
            this.listLocator = listLocator;
        }

        /// <summary>
        /// Method to ensure a file in a document library. Will create if not exist.
        /// </summary>
        /// <param name="web">The web where the list or library is</param>
        /// <param name="listTitle">The list title where to ensure the file</param>
        /// <param name="file">The fileinfo containing all the information needed to create the file</param>
        /// <returns>The file</returns>
        public SPFile EnsureFile(SPWeb web, string listTitle, FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            SPFile sharePointFile = null;

            // Locate the list/library
            var list = this.listLocator.TryGetList(web, listTitle);

            // Go get the file if its url is not null
            if (!string.IsNullOrEmpty(file.Url.ToString()))
            {
                sharePointFile = web.GetFile(file.Url.ToString());
            }

            // If the file is not found, create it.
            if (sharePointFile == null || file.Overwrite)
            {
                sharePointFile = list.RootFolder.Files.Add(file.FileName, file.Data, file.Overwrite);
            }

            return sharePointFile;
        }
    }
}
