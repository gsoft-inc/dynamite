using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Field Info type mostly used to document SharePoint OOTB (built-in) field definitions
    /// </summary>
    public class MinimalFieldInfo : FieldInfo<string>
    {
        /// <summary>
        /// TODO: document proper field types and get rid of this
        /// </summary>
        /// <param name="internalName">Internal name of the site column</param>
        /// <param name="id">Unique ID of the column</param>
        public MinimalFieldInfo(string internalName, Guid id) 
            : base(internalName, id, string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// The XML schema of the field
        /// </summary>
        public override XElement Schema
        {
            get { return this.BasicFieldSchema; }
        }
    }
}
