using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using System;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a NoteField info
    /// </summary>
    public class NoteFieldInfo : FieldInfo<string>
    {
        /// <summary>
        /// Initializes a new NoteFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Description resource key</param>
        public NoteFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "Note", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
        }

        /// <summary>
        /// The XML schema of the Note field
        /// </summary>
        public override XElement Schema
        {
            get
            {
                return this.BasicFieldSchema;
            }
        }
    }
}
