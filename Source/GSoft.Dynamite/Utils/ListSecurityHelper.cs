using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Sharepoint2013.Utils
{
    /// <summary>
    /// Utility to change lists' permissions
    /// </summary>
    public class ListSecurityHelper
    {
        /// <summary>
        /// Method to remove the collaboration rights to all members excepts administrator
        /// </summary>
        /// <param name="list">The list to affect the change</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics in public facing members is discouraged for more consistency with dependency injection.")]
        public void SetListToReadOnlyExceptAdmin(SPSecurableObject list)
        {
            // Break List inheritance and remove
            list.BreakRoleInheritance(true); // Copy RoleAssignment

            // Modify the Contributor role assignment to read only.
            foreach (var roleAssignment in list.RoleAssignments.Cast<SPRoleAssignment>())
            {
                var roleDefinitionBindings = roleAssignment.RoleDefinitionBindings.Cast<SPRoleDefinition>();
                var collaboratorRole = roleDefinitionBindings.SingleOrDefault(roleDefinition => roleDefinition.Type == SPRoleType.Contributor);
                if (collaboratorRole != null && roleDefinitionBindings.SingleOrDefault(roleDefinition => roleDefinition.Type == SPRoleType.Administrator) == null)
                {
                    roleAssignment.RoleDefinitionBindings.Remove(collaboratorRole);
                    roleAssignment.Update();
                }
            }
        }

        /// <summary>
        /// Method to remove the collaboration rights to all members excepts administrator
        /// </summary>
        /// <param name="list">The list to affect the change</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics in public facing members is discouraged for more consistency with dependency injection.")]
        public void SetListHiddenExceptAdmin(SPSecurableObject list)
        {
            // Break permissions inheritance
            list.BreakRoleInheritance(true); // Copy RoleAssignment

            // Remove all role assignments except the Administrators'
            var roleAssignmentsToRemove = new List<SPRoleAssignment>();
            foreach (var roleAssignment in list.RoleAssignments.Cast<SPRoleAssignment>())
            {
                var roleDefinitionBindings = roleAssignment.RoleDefinitionBindings.Cast<SPRoleDefinition>();
                if (roleDefinitionBindings.SingleOrDefault(roleDefinition => roleDefinition.Type == SPRoleType.Administrator) == null)
                {
                    // Not an administrator group, mark the assignment for removal
                    roleAssignmentsToRemove.Add(roleAssignment);
                }
            }

            foreach (var roleAssignemntToRemove in roleAssignmentsToRemove)
            {
                list.RoleAssignments.Remove(roleAssignemntToRemove.Member);
            }
        }
    }
}
