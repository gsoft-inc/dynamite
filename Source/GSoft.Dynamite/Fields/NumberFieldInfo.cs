using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using System;
using System.Globalization;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Definition of a NumberFieldInfo info
    /// </summary>
    public class NumberFieldInfo : FieldInfo<float>
    {
        /// <summary>
        /// Initializes a new NumberFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Description resource key</param>
        public NumberFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "Number", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // default number of lines shown when editing
            this.Decimals = 6;
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
        public int? Min { get; set; }

        /// <summary>
        /// Maximum value allowed
        /// </summary>
        public int? Max { get; set; }

        /// <summary>
        /// The XML schema of the Note field
        /// </summary>
        public override XElement Schema
        {
            get
            {
                var schema = this.BasicFieldSchema;

                schema.Add(new XAttribute("Decimals", this.Decimals));

                if (this.IsPercentage)
                {
                    schema.Add(new XAttribute("Percentage", "TRUE"));
                }
                else
                {
                    schema.Add(new XAttribute("Percentage", "FALSE"));
                }

                if (this.Min.HasValue)
                {
                    schema.Add(new XAttribute("Min", this.Min.Value));
                }

                if (this.Max.HasValue)
                {
                    schema.Add(new XAttribute("Max", this.Max.Value));
                }

                return schema;
            }
        }
    }
}
