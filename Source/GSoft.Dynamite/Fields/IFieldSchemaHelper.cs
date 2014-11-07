using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Helps in formatted SharePoint field schema XML
    /// </summary>
    public interface IFieldSchemaHelper
    {
        /// <summary>
        /// Generates the Field XML for a site column definition
        /// </summary>
        /// <param name="fieldInfo">The field definition for which we want to print out the full XML schema</param>
        /// <returns>The XML schema of the field</returns>
        XElement SchemaForField(IFieldInfo fieldInfo);
    }
}
