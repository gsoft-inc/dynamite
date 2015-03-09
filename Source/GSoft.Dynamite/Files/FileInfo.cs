using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Files
{
    /// <summary>
    /// Object containing information about a file. Used to create a file in a list/library
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileName">The filename</param>
        /// <param name="data">The data stream</param>
        /// <param name="overwrite">If we overwrite the file</param>
        public FileInfo(string fileName, Stream data, bool overwrite)
        {
            this.FileName = fileName;
            this.Data = data;
            this.Overwrite = overwrite;
        }

        /// <summary>
        /// Constructor with overwrite at false
        /// </summary>
        /// <param name="fileName">The filename</param>
        /// <param name="data">The file data</param>
        public FileInfo(string fileName, Stream data)
            : this(fileName, data, false)
        {
        }

        /// <summary>
        /// The filename of the file. usually contain the extension
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The data stream to write in the file
        /// </summary>
        public Stream Data { get; set; }

        /// <summary>
        /// If we overwite the file or not
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        /// The Url of the file, to check if exist
        /// </summary>
        public Uri Url { get; set; }
    }
}
