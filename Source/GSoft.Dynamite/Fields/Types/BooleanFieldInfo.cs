using System;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Definition of a Boolean field
    /// </summary>
    public class BooleanFieldInfo : BaseFieldInfoWithValueType<bool?>
    {
        /// <summary>
        /// Initializes a new <see cref="BooleanFieldInfo"/> instance
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public BooleanFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "Boolean", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
        }

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
            // Assuming Boolean field type has no special properties of its own
            return baseFieldSchema;
        }
    }
}
