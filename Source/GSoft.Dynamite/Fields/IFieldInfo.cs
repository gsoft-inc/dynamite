using System;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Metadata about a SharePoint field/site column
    /// </summary>
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
        RequiredType Required { get; set; }

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
        /// Indicates if field should be shown in the display form
        /// </summary>
        bool IsHiddenInDisplayForm { get; set; }

        /// <summary>
        /// Indicates if field should be shown in the new form
        /// </summary>
        bool IsHiddenInNewForm { get; set; }

        /// <summary>
        /// Indicates if field should be shown in the edit form
        /// </summary>
        bool IsHiddenInEditForm { get; set; }

        /// <summary>
        /// Indicates if field should be shown in the list settings
        /// </summary>
        bool IsHiddenInListSettings { get; set; }

        /// <summary>
        /// The string XML format of the field
        /// </summary>
        /// <returns>The XML schema of the field as string</returns>
        string ToString();
    }
}
