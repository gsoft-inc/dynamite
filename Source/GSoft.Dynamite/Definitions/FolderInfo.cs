using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a folder in a library
    /// </summary>
    public class FolderInfo
    {
        private IList<PageInfo> pages;
        private IList<FolderInfo> subFolders;

        /// <summary>
        /// Name of the folder
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Pages in the folder
        /// </summary>
        public IList<PageInfo> Pages 
        {
            get
            {
                if (this.pages == null)
                {
                    return new List<PageInfo>();
                }

                return this.pages;
            }

            set
            {
                this.pages = value;
            }
        }

        /// <summary>
        /// Sub folders
        /// </summary>
        public IList<FolderInfo> SubFolders
        {
            get
            {
                if (this.subFolders == null)
                {
                    return new List<FolderInfo>();
                }

                return this.subFolders;
            }

            set
            {
                this.subFolders = value;
            }
        } 

        /// <summary>
        /// Values for the folder should be stored in the DefaultValue
        /// property of the FieldInfo objects.
        /// </summary>
        public ICollection<IFieldInfo> ItemFieldValues { get; set; }

        /// <summary>
        /// True if the folder is a root folder
        /// </summary>
        public bool IsRootFolder { get; set; }

        /// <summary>
        /// The Welcome Page of the folder
        /// </summary>
        public PageInfo WelcomePage { get; set; }

        /// <summary>
        /// The culture to create that folder hierarchie. If null, create in all culture.
        /// </summary>
        public CultureInfo Locale { get; set; }
    }
}
