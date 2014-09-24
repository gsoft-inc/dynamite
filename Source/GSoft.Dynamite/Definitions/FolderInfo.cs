using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Definitions.Values;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a folder in a library
    /// </summary>
    public class FolderInfo
    {
        #region Backing fields

        private IDictionary<string, PageInfo> _pages;
        private IList<FolderInfo> _subFolders;

        #endregion

        /// <summary>
        /// Name of the folder
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Pages in the folder
        /// </summary>
        public IDictionary<string, PageInfo> Pages 
        {
            get
            {
                if (this._pages == null)
                {
                    return new Dictionary<string, PageInfo>();
                }

                return this._pages;
            }

            set
            {
                this._pages = value;
            }
        }

        /// <summary>
        /// Sub folders
        /// </summary>
        public IList<FolderInfo> SubFolders
        {
            get
            {
                if (this._subFolders == null)
                {
                    return new List<FolderInfo>();
                }

                return this._subFolders;
            }

            set
            {
                this._subFolders = value;
            }
        } 

        /// <summary>
        /// Values for the folder
        /// </summary>
        public IDictionary<FieldInfo, IFieldInfoValue> Values { get; set; }

        /// <summary>
        /// True if the folder is a root folder
        /// </summary>
        public bool IsRootFolder { get; set; }
    }
}
