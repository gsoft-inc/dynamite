using System;
using System.Globalization;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Definition of a NumberFieldInfo info
    /// </summary>
    public class NumberFieldInfo : BaseFieldInfoWithValueType<double?>
    {
        /// <summary>
        /// Initializes a new NumberFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public NumberFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "Number", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // default number of decimals places to keep on number field
            this.Decimals = 0;
            this.IsPercentage = false;
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public NumberFieldInfo(XElement fieldSchemaXml)
            : base(fieldSchemaXml)
        {
            if (fieldSchemaXml.Attribute("Decimals") != null)
            {
                this.Decimals = int.Parse(fieldSchemaXml.Attribute("Decimals").Value, CultureInfo.InvariantCulture);
            }

            if (fieldSchemaXml.Attribute("Percentage") != null
                && bool.Parse(fieldSchemaXml.Attribute("Percentage").Value))
            {
                this.IsPercentage = true;
            }

            if (fieldSchemaXml.Attribute("Min") != null)
            {
                this.Min = double.Parse(fieldSchemaXml.Attribute("Min").Value, CultureInfo.InvariantCulture);
            }

            if (fieldSchemaXml.Attribute("Max") != null)
            {
                this.Max = double.Parse(fieldSchemaXml.Attribute("Max").Value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Number of decimal places shown
        /// </summary>
        public int Decimals { get; set; }

        /// <summary>
        /// Percent display toggle
        /// </summary>
        public bool IsPercentage { get; set; }

        /// <summary>
        /// Minimum value allowed
        /// </summary>
        public double? Min { get; set; }

        /// <summary>
        /// Maximum value allowed
        /// </summary>
        public double? Max { get; set; }

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
            baseFieldSchema.Add(new XAttribute("Decimals", this.Decimals));

            if (this.IsPercentage)
            {
                baseFieldSchema.Add(new XAttribute("Percentage", "TRUE"));
            }
            else
            {
                baseFieldSchema.Add(new XAttribute("Percentage", "FALSE"));
            }

            if (this.Min.HasValue)
            {
                baseFieldSchema.Add(new XAttribute("Min", this.Min.Value));
            }

            if (this.Max.HasValue)
            {
                baseFieldSchema.Add(new XAttribute("Max", this.Max.Value));
            }

            return baseFieldSchema;
        }
    }
}
