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
        string DisplayNameResourceKey { get; }

        /// <summary>
        /// Field description
        /// </summary>
        string DescriptionResourceKey { get; }

        /// <summary>
        /// Content group in SharePoint definitions
        /// </summary>
        string GroupResourceKey { get; }
        
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
        /// Indicates if field is hidden by default
        /// </summary>
        bool IsHidden { get; set; }

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
        /// Default formula for the field
        /// </summary>
        string DefaultFormula { get; set; }

        /// <summary>
        /// Extends a basic XML schema with the field type's extra attributes
        /// </summary>
        /// <param name="baseFieldSchema">
        /// The basic field schema XML (Id, InternalName, DisplayName, etc.) on top of which 
        /// we want to add field type-specific attributes
        /// </param>
        /// <returns>The full field XML schema</returns>
        XElement Schema(XElement baseFieldSchema);
    }
}
