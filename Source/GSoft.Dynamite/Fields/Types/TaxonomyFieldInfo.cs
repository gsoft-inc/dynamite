using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Definition for a Taxonomy field
    /// </summary>
    public class TaxonomyFieldInfo : FieldInfo<TaxonomyFullValue>
    {                
        /// <summary>
        /// Initializes a new FieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public TaxonomyFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "TaxonomyFieldType", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
        }

        /// <summary>
        /// If true, the full parent-to-children path to the term will be rendered in the UI whenever
        /// a term is associated to this field
        /// </summary>
        public bool IsPathRendered { get; set; }

        /// <summary>
        /// If the associated TermSet is open and this value is true, then contributors will
        /// be able to "fill-in" taxonomy value
        /// </summary>
        public bool CreateValuesInEditForm { get; set; }

        /// <summary>
        /// Determines to which term set (and, optionally, which sub-term) the taxonomy column
        /// will be mapped, limiting the user's choices in the Edit Form's taxonomy picker.
        /// </summary>
        public TaxonomyContext TermStoreMapping { get; set; }

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
            baseFieldSchema.Add(new XAttribute("Mult", "FALSE"));
            baseFieldSchema.Add(TaxonomyFieldCustomizationSchema(Guid.NewGuid(), this.IsPathRendered, this.CreateValuesInEditForm));

            return baseFieldSchema;
        }

        /// <summary>
        /// Helper method to format a taxonomy field XML schema
        /// </summary>
        /// <param name="associatedNoteFieldId">Unique ID of the taxonomy field's associated note field</param>
        /// <param name="isPathRendered">Whether the full parent-to-child path to the term should be rendered in the SharePoint UI</param>
        /// <param name="createValuesInEditForm">Whether the field allow creating new values from the taxonomy picker</param>
        /// <returns>The field schema XML</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Schema requires lowercase.")]
        public static XElement TaxonomyFieldCustomizationSchema(Guid associatedNoteFieldId, bool isPathRendered, bool createValuesInEditForm)
        {
            XNamespace p4 = "http://www.w3.org/2001/XMLSchema-instance";

            return new XElement(
                "Customization",
                new XElement(
                    "ArrayOfProperty",
                    new XElement(
                        "Property",
                        new XElement("Name", "TextField"),
                        new XElement(
                            "Value",
                            new XAttribute(XNamespace.Xmlns + "q6", "http://www.w3.org/2001/XMLSchema"),
                            new XAttribute(p4 + "type", "q6:string"),
                            new XAttribute(XNamespace.Xmlns + "p4", "http://www.w3.org/2001/XMLSchema-instance"),
                            "{" + associatedNoteFieldId + "}")),
                    new XElement(
                        "Property",
                        new XElement("Name", "IsPathRendered"),
                        new XElement(
                            "Value",
                            new XAttribute(XNamespace.Xmlns + "q7", "http://www.w3.org/2001/XMLSchema"),
                            new XAttribute(p4 + "type", "q7:boolean"),
                            new XAttribute(XNamespace.Xmlns + "p4", "http://www.w3.org/2001/XMLSchema-instance"),
                            isPathRendered.ToString().ToLowerInvariant()),
                    new XElement(
                        "Property",
                        new XElement("Name", "CreateValuesInEditForm"),
                        new XElement(
                            "Value",
                            new XAttribute(XNamespace.Xmlns + "q9", "http://www.w3.org/2001/XMLSchema"),
                            new XAttribute(p4 + "type", "q9:boolean"),
                            new XAttribute(XNamespace.Xmlns + "p4", "http://www.w3.org/2001/XMLSchema-instance"),
                            createValuesInEditForm.ToString().ToLowerInvariant())))));
        }
    }
}
