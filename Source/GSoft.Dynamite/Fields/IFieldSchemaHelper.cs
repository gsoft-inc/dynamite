using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Helps in formatting and ensuring SharePoint field schema XML
    /// </summary>
    public interface IFieldSchemaHelper
    {
        /// <summary>
        /// Generates the Field XML for a site column definition
        /// </summary>
        /// <param name="fieldInfo">The field definition for which we want to print out the full XML schema</param>
        /// <returns>The XML schema of the field</returns>
        XElement SchemaForField(IFieldInfo fieldInfo);

        /// <summary>
        /// Adds a field defined in xml to a collection of fields.
        /// </summary>
        /// <param name="fieldCollection">The SPField collection.</param>
        /// <param name="fieldXml">The field XML schema.</param>
        /// <returns>
        /// The new field.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// fieldCollection
        /// or
        /// fieldXml
        /// </exception>
        /// <exception cref="System.FormatException">Invalid xml.</exception>
        SPField EnsureFieldFromSchema(SPFieldCollection fieldCollection, XElement fieldXml);
    }
}
