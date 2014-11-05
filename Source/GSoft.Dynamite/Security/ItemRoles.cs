using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Security
{
    using System.Diagnostics.CodeAnalysis;

    using GSoft.Dynamite.Collections;
    using GSoft.Dynamite.Repositories;

    using Microsoft.SharePoint;

    /// <summary>
    /// Defines the roles that must be set to a specific list
    /// </summary>
    public class ItemRoles : ObjectRoles
    {
        private string itemName;

        private IItemLocator itemLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListRoles"/> class.
        /// </summary>
        /// <param name="web">
        /// The web.
        /// </param>
        /// <param name="itemLocator">
        /// The item locator.
        /// </param>
        /// <param name="listLocation">
        /// The list url.
        /// </param>
        /// <param name="itemName">
        /// The item Name.
        /// </param>
        /// <param name="groupRolePairs">
        /// The group Role Pairs.
        /// </param>
        /// <param name="shouldCopyParentRolesWhenBreakingInheritance">Whether roles should be copied when inheritance is broken</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Just using default value for convenience")]
        public ItemRoles(SPWeb web, IItemLocator itemLocator, string listLocation, string itemName, IEnumerable<GroupRolePair> groupRolePairs, bool shouldCopyParentRolesWhenBreakingInheritance = true)
            : base(shouldCopyParentRolesWhenBreakingInheritance)
        {
            this.Web = web;
            this.SecurableObjectLocation = listLocation;
            this.itemName = itemName;

            this.itemLocator = itemLocator;

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
                return this.itemLocator.GetByTitle(this.Web, new Uri(this.SecurableObjectLocation, UriKind.Relative), this.itemName);
            }
        }
    }
}
