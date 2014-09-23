using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Definitions.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSoft.Dynamite.Definitions
{
    public interface IFieldInfo
    {
        /// <summary>
        /// The internal name of the field
        /// </summary>
        string InternalName { get; set; }
        
        /// <summary>
        /// Unique identifier of the field
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Indicates if the field is required
        /// </summary>
        RequiredTypes RequiredType { get; set; }

        /// <summary>
        /// Indicates if the field must enforce unique values
        /// </summary>
        bool EnforceUniqueValues { get; set; }

        /// <summary>
        /// The static name of the field
        /// </summary>
        string StaticName { get; set; }

        /// <summary>
        /// Type of the field
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// The XML schema of the field
        /// </summary>
        XElement Schema { get; set; }

        /// <summary>
        /// Default mapping configuration for the field
        /// </summary>
        object DefaultValue { get; set; }

        /// <summary>
        /// The XElement XML format of the field
        /// </summary>
        /// <returns>The XML schema of the field as XElement</returns>
        XElement ToXElement();

        /// <summary>
        /// The string XML format of the field
        /// </summary>
        /// <returns>The XML schema of the field as string</returns>
        string ToString();
    }
}
