using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Security
{
    using System.Diagnostics.CodeAnalysis;

    using GSoft.Dynamite.Collections;
    using GSoft.Dynamite.Lists;

    using Microsoft.SharePoint;

    /// <summary>
    /// Defines the roles that must be set to a specific list
    /// </summary>
    public class ListRoles : ObjectRoles
    {
        private IListLocator listLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListRoles"/> class.
        /// </summary>
        /// <param name="web">
        /// The web.
        /// </param>
        /// <param name="listLocator">
        /// The list Locator.
        /// </param>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <param name="groupRolePairs">
        /// The group Role Pairs.
        /// </param>
        /// <param name="shouldCopyParentRolesWhenBreakingInheritance">Whether roles should be copied when inheritance is broken</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Just using default value for convenience")]
        public ListRoles(SPWeb web, IListLocator listLocator, string list, IEnumerable<GroupRolePair> groupRolePairs, bool shouldCopyParentRolesWhenBreakingInheritance = true)
            : base(shouldCopyParentRolesWhenBreakingInheritance)
        {
            this.Web = web;
            this.listLocator = listLocator;
            this.SecurableObjectLocation = list;

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
                return this.listLocator.GetByUrl(this.Web, this.SecurableObjectLocation);
            }
        }
    }
}
