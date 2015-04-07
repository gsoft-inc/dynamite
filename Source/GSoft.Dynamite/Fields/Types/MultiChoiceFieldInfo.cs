using System;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Multiple choice field information.
    /// </summary>
    public class MultiChoiceFieldInfo : BaseChoiceFieldInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiChoiceFieldInfo"/> class.
        /// </summary>
        /// <param name="internalName">Name of the internal.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="displayNameResourceKey">The display name resource key.</param>
        /// <param name="descriptionResourceKey">The description resource key.</param>
        /// <param name="groupResourceKey">The group resource key.</param>
        public MultiChoiceFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "MultiChoice", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
        }
    }
}
