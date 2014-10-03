using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Security
{
    using Microsoft.SharePoint;

    /// <summary>
    /// Represents a security group and its role definition
    /// </summary>
    public class GroupRolePair
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRolePair"/> class.
        /// </summary>
        /// <param name="web">
        /// The web.
        /// </param>
        /// <param name="groupName">
        /// The group name.
        /// </param>
        /// <param name="roleDefinition">
        /// The role definition.
        /// </param>
        public GroupRolePair(SPWeb web, string groupName, string roleDefinition)
        {
            this.Principal = web.SiteGroups.Cast<SPGroup>().FirstOrDefault(x => x.Name == groupName);

            this.RoleDefinition = roleDefinition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRolePair"/> class with the group name only.
        /// </summary>
        /// <param name="web">
        /// The web.
        /// </param>
        /// <param name="groupName">
        /// The group name.
        /// </param>
        public GroupRolePair(SPWeb web, string groupName)
        {
            this.Principal = web.SiteGroups.Cast<SPGroup>().FirstOrDefault(x => x.Name == groupName);
        }

        /// <summary>
        /// Gets the principal.
        /// </summary>
        public SPPrincipal Principal { get; private set; }

        /// <summary>
        /// Gets the role definition.
        /// </summary>
        public string RoleDefinition { get; private set; }
    }
}
