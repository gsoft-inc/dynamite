using System;
using System.Globalization;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Definition of a CurrencyFieldInfo info
    /// </summary>
    public class CurrencyFieldInfo : BaseFieldInfoWithValueType<double?>
    {
        /// <summary>
        /// Initializes a new CurrencyFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public CurrencyFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "Currency", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // en-US currency format by default (LCID = 1033)
            this.LocaleId = Language.English.Culture.LCID;
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public CurrencyFieldInfo(XElement fieldSchemaXml)
            : base(fieldSchemaXml)
        {
            if (fieldSchemaXml.Attribute("CurrencyLocaleId") != null)
            {
                this.LocaleId = int.Parse(fieldSchemaXml.Attribute("CurrencyLocaleId").Value, CultureInfo.InvariantCulture);
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
        /// LCID of the culture that will determine the currency formatting
        /// </summary>
        public int LocaleId { get; set; }

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
            // CurrencyLocaleId won't get persisted through SchemaXML (gotta set the property on SPCurrencyField instance directly).
            // Do Min and Max at least through XML.
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
