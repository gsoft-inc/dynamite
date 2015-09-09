using System;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Choice field information.
    /// </summary>
    public class ChoiceFieldInfo : BaseChoiceFieldInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceFieldInfo"/> class.
        /// </summary>
        /// <param name="internalName">Internal name of the field.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="displayNameResourceKey">The display name resource key.</param>
        /// <param name="descriptionResourceKey">The description resource key.</param>
        /// <param name="groupResourceKey">The group resource key.</param>
        public ChoiceFieldInfo(
            string internalName, 
            Guid id, 
            string displayNameResourceKey, 
            string descriptionResourceKey, 
            string groupResourceKey)
            : base(internalName, id, "Choice", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
        }

        /// <summary>
        /// Format can be drop-down or radio buttons
        /// </summary>
        public ChoiceFieldFormat Format { get; set; }

        /// <summary>
        /// Builds the additional schema elements for this field.
        /// </summary>
        /// <param name="baseFieldSchema">The base field schema.</param>
        /// <returns>The field schema XML.</returns>
        public override XElement Schema(XElement baseFieldSchema)
        {
            baseFieldSchema.Add(new XAttribute("Format", this.Format.ToString()));
            return base.Schema(baseFieldSchema);
        }
    }
}
