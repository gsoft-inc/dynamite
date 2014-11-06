using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Globalization;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Helps in formatted SharePoint field schema XML
    /// </summary>
    public class FieldSchemaHelper : IFieldSchemaHelper
    {
        private IResourceLocator resourceLocator;

        /// <summary>
        /// Initializes a new <see cref="FieldChemaHelper"/> instance
        /// </summary>
        /// <param name="resourceLocator">The resource locator</param>
        public FieldSchemaHelper(IResourceLocator resourceLocator)
        {
            this.resourceLocator = resourceLocator;
        }

        /// <summary>
        /// Generates the Field XML for a site column definition
        /// </summary>
        /// <param name="fieldInfo">The field definition for which we want to print out the full XML schema</param>
        /// <returns>The XML schema of the field</returns>
        public XElement SchemaForField(IFieldInfo fieldInfo)
        {
            var schema = new XElement(
                "Field",
                new XAttribute("Name", fieldInfo.InternalName),
                new XAttribute("Type", fieldInfo.Type),
                new XAttribute("ID", "{" + fieldInfo.Id + "}"),
                new XAttribute("StaticName", fieldInfo.InternalName),
                new XAttribute("DisplayName", this.resourceLocator.GetResourceString(fieldInfo.DisplayNameResourceKey)),
                new XAttribute("Description", this.resourceLocator.GetResourceString(fieldInfo.DescriptionResourceKey)),
                new XAttribute("Group", this.resourceLocator.GetResourceString(fieldInfo.GroupResourceKey)),
                new XAttribute("EnforceUniqueValues", fieldInfo.EnforceUniqueValues.ToString().ToUpper(CultureInfo.InvariantCulture)));

            // Check the Required type
            if (fieldInfo.Required == RequiredType.Required)
            {
                schema.Add(new XAttribute("Required", "TRUE"));
            }

            if (fieldInfo.Required == RequiredType.NotRequired)
            {
                schema.Add(new XAttribute("Required", "FALSE"));
            }

            // Hidden state
            if (fieldInfo.IsHidden)
            {
                schema.Add(new XAttribute("Hidden", "TRUE"));
            }

            // Show in Display Form
            if (fieldInfo.IsHiddenInDisplayForm)
            {
                schema.Add(new XAttribute("ShowInDisplayForm", "FALSE"));
            }

            // Show in Edit Form
            if (fieldInfo.IsHiddenInEditForm)
            {
                schema.Add(new XAttribute("ShowInEditForm", "FALSE"));
            }

            // Show in new Form
            if (fieldInfo.IsHiddenInNewForm)
            {
                schema.Add(new XAttribute("ShowInNewForm", "FALSE"));
            }

            // Show in List settings
            if (fieldInfo.IsHiddenInListSettings)
            {
                schema.Add(new XAttribute("ShowInListSettings", "FALSE"));
            }
            else
            {
                schema.Add(new XAttribute("ShowInListSettings", "TRUE"));
            }

            // Extend the basic field scheme (everything listed above here) with the specific field type's extra attributes
            return fieldInfo.Schema(schema);
        }
    }
}
