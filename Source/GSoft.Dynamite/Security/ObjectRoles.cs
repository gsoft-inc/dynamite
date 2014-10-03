using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Security
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;

    /// <summary>
    /// The object roles.
    /// </summary>
    public abstract class ObjectRoles : IObjectRoles
    {
        /// <summary>
        /// Abstract base constructor
        /// </summary>
        /// <param name="shouldCopyParentRolesWhenBreakingInheritance">Whether roles should be copied when inheritance is broken</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Just using default value for convenience")]
        protected ObjectRoles(bool shouldCopyParentRolesWhenBreakingInheritance = true)
        {
            this.GroupRolePairs = new List<GroupRolePair>();

            // By default, an object role adds to the Target's existing parent permissions.
            // If false is specified, then permissions on the target will be wiped when breaking
            // inheritance.
            this.ShouldCopyParentRolesWhenBreakingInheritance = shouldCopyParentRolesWhenBreakingInheritance;
        }

        /// <summary>
        /// Gets or sets the securable object url.
        /// </summary>
        public string SecurableObjectLocation { get; set; }

        /// <summary>
        /// The list of pair group and role.
        /// </summary>
        public ICollection<GroupRolePair> GroupRolePairs { get; private set; }

        /// <summary>
        /// The the group role pairs get applied to the target, permissions inheritance will be broken.
        /// At that point, you have the choice to copy the parent's roles (so that permissions will simply
        /// be added) or to completely wipe the permissions on the target (breaking inheritance without parent role copies)
        /// </summary>
        public bool ShouldCopyParentRolesWhenBreakingInheritance { get; set; }

        /// <summary>
        /// Gets the target.
        /// </summary>
        public abstract SPSecurableObject Target { get; }

        /// <summary>
        /// Gets or sets the web.
        /// </summary>
        protected SPWeb Web { get; set; }
    }
}
