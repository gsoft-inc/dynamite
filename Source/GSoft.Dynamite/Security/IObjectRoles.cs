using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Security
{
    using Microsoft.SharePoint;

    /// <summary>
    /// The ObjectRoles interface.
    /// </summary>
    public interface IObjectRoles
    {
        /// <summary>
        /// Gets or sets the securable object location.
        /// </summary>
        string SecurableObjectLocation { get; }

        /// <summary>
        /// Gets the group role pairs.
        /// </summary>
        ICollection<GroupRolePair> GroupRolePairs { get; }

        /// <summary>
        /// The the group role pairs get applied to the target, permissions inheritance will be broken.
        /// At that point, you have the choice to copy the parent's roles (so that permissions will simply
        /// be added) or to completely wipe the permissions on the target (breaking inheritance without parent role copies)
        /// </summary>
        bool ShouldCopyParentRolesWhenBreakingInheritance { get; set; }

        /// <summary>
        /// Gets the target.
        /// </summary>
        SPSecurableObject Target { get; }
    }
}
