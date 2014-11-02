using System;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Definition of a GUID field
    /// </summary>
    public class GuidFieldInfo : FieldInfo<Guid>
    {
        /// <summary>
        /// Initializes a new <see cref="GuidFieldInfo"/> instance
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public GuidFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "Guid", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
        }

        /// <summary>
        /// The XML schema of a GUID field as XElement
        /// </summary>
        /// <returns>The XML schema</returns>
        public override XElement Schema
        {
            get
            {
                // Assuming Guid field type has no special properties of its own
                return this.BasicFieldSchema;
            }
        }
    }
}
