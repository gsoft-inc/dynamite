using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Security
{
    using System.Diagnostics.CodeAnalysis;

    using GSoft.Dynamite.Collections;

    using Microsoft.SharePoint;

    /// <summary>
    /// Defines the roles that must be set to a specific folder
    /// </summary>
    public class FolderRoles : ObjectRoles
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FolderRoles"/> class.
        /// </summary>
        /// <param name="web">
        /// The web.
        /// </param>
        /// <param name="folder">
        /// The folder.
        /// </param>
        /// <param name="groupRolePairs">
        /// The group Role Pair.
        /// </param>
        /// <param name="shouldCopyParentRolesWhenBreakingInheritance">Whether roles should be copied when inheritance is broken</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Just using default value for convenience")]
        public FolderRoles(SPWeb web, string folder, IEnumerable<GroupRolePair> groupRolePairs, bool shouldCopyParentRolesWhenBreakingInheritance = true)
            : base(shouldCopyParentRolesWhenBreakingInheritance)
        {
            this.Web = web;

            this.SecurableObjectLocation = folder;

            this.GroupRolePairs.Clear();
            this.GroupRolePairs.AddRange(groupRolePairs);
        }

        /// <summary>
        /// Gets the target.
        /// </summary>
        public override SPSecurableObject Target
        {
            get
            {
                var folder = this.Web.GetFolder(this.SecurableObjectLocation);

                return folder != null ? folder.Item : null;
            }
        }
    }
}
