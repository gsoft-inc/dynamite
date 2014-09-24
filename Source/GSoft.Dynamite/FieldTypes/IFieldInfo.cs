using GSoft.Dynamite.Binding;
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
        /// Unique identifier of the field
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The internal name of the field
        /// </summary>
        string InternalName { get; }

        /// <summary>
        /// Type of the field
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Field display title
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Field description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Content group in SharePoint definitions
        /// </summary>
        string Group { get; }
        
        /// <summary>
        /// Indicates if the field is required
        /// </summary>
        RequiredTypes Required { get; set; }

        /// <summary>
        /// Indicates if the field must enforce unique values
        /// </summary>
        bool EnforceUniqueValues { get; set; }

        /// <summary>
        /// Returns the FieldInfo's associated ValueType.
        /// For example, a TextFieldInfo should return typeof(string)
        /// and a TaxonomyFieldInfo should return typeof(TaxonomyValue)
        /// </summary>
        Type AssociatedValueType { get; }

        /// <summary>
        /// The XML schema of the field
        /// </summary>
        XElement Schema { get; }

        /// <summary>
        /// The string XML format of the field
        /// </summary>
        /// <returns>The XML schema of the field as string</returns>
        string ToString();

    }
}
