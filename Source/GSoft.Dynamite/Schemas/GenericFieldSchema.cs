using System;
using System.Xml.Linq;

namespace GSoft.Dynamite.Schemas
{
    /// <summary>
    /// Generic XML schema for a SharePoint field
    /// </summary>
    public abstract class GenericFieldSchema
    {
        private Guid fieldId;

        /// <summary>
        /// Default constructor
        /// </summary>
        protected GenericFieldSchema()
        {
            this.fieldId = Guid.NewGuid();
        }

        #region Properties

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the name of the field static.
        /// </summary>
        /// <value>
        /// The name of the field static.
        /// </value>
        public string FieldStaticName { get; set; }

        /// <summary>
        /// Gets or sets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public string FieldType { get; set; }

        /// <summary>
        /// Gets or sets the display name of the field.
        /// </summary>
        /// <value>
        /// The display name of the field.
        /// </value>
        public string FieldDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the field description.
        /// </summary>
        /// <value>
        /// The field description.
        /// </value>
        public string FieldDescription { get; set; }

        /// <summary>
        /// Gets or sets the field group.
        /// </summary>
        /// <value>
        /// The field group.
        /// </value>
        public string FieldGroup { get; set; }

        /// <summary>
        /// Gets or sets the field identifier.
        /// </summary>
        /// <value>
        /// The field identifier.
        /// </value>
        public Guid FieldId
        {
            get { return this.fieldId; }
            set { this.fieldId = value; }
        }

        /// <summary>
        /// Gets or sets the field schema.
        /// </summary>
        /// <value>
        /// The field schema.
        /// </value>
        public XElement FieldSchema { get; set; }

        #endregion

        /// <summary>
        /// Get the XML schema of the field.
        /// </summary>
        /// <returns>The XML schema.</returns>
        public abstract XElement ToXElement();

        /// <summary>
        /// Get the XML schema as string of the field.
        /// </summary>
        /// <returns>A string that represents the XML schema.</returns>
        public abstract override string ToString();
    }
}
