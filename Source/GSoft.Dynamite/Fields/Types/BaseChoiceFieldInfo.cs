using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Choice field information.
    /// </summary>
    public abstract class BaseChoiceFieldInfo : BaseFieldInfoWithValueType<string>
    {
        private readonly IList<string> choices = new List<string>(); 

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseChoiceFieldInfo"/> class.
        /// </summary>
        /// <param name="internalName">Name of the internal.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="fieldTypeName">Name of the field type.</param>
        /// <param name="displayNameResourceKey">The display name resource key.</param>
        /// <param name="descriptionResourceKey">The description resource key.</param>
        /// <param name="groupResourceKey">The group resource key.</param>
        protected BaseChoiceFieldInfo(            
            string internalName, 
            Guid id,
            string fieldTypeName,
            string displayNameResourceKey, 
            string descriptionResourceKey, 
            string groupResourceKey)
            : base(internalName, id, fieldTypeName, displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field allows users to fill in values for the column.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [the field allows users to fill in values for the column]; otherwise, <c>false</c>.
        /// </value>
        public bool FillInChoice { get; set; }

        /// <summary>
        /// Gets the choices.
        /// </summary>
        /// <value>
        /// The choices.
        /// </value>
        public IList<string> Choices
        {
            get
            {
                return this.choices;
            }
        }

        /// <summary>
        /// Builds the additional schema elements for this field.
        /// </summary>
        /// <param name="baseFieldSchema">The base field schema.</param>
        /// <returns>The field schema XML.</returns>
        public override XElement Schema(XElement baseFieldSchema)
        {
            baseFieldSchema.Add(new XAttribute("FillInChoice", this.FillInChoice.ToString().ToUpperInvariant()));
            baseFieldSchema.Add(GetChoicesSchema(this.Choices));
            return baseFieldSchema;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Schema requires lowercase.")]
        private static XElement GetChoicesSchema(IEnumerable<string> choices)
        {
            return new XElement("CHOICES", choices.Select(choice => new XElement("CHOICE", choice)));
        }
    }
}
