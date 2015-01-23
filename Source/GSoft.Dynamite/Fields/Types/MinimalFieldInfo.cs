using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields.Types
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
        /// Extends a basic XML schema with the field type's extra attributes
        /// </summary>
        /// <param name="baseFieldSchema">
        /// The basic field schema XML (Id, InternalName, DisplayName, etc.) on top of which 
        /// we want to add field type-specific attributes
        /// </param>
        /// <returns>The full field XML schema</returns>
        public override XElement Schema(XElement baseFieldSchema)
        {
            throw new NotSupportedException("A MinimalFieldInfo does not contain enough data to generate a SchemaXML.");
        }
    }
}
