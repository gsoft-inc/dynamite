using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GSoft.Dynamite.Extensions;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Security
{
    /// <summary>
    /// Helper class for managing Role-based security.
    /// </summary>
    public class SecurityHelper : ISecurityHelper
    {
        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="listItem">The list item.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleType">Type of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AddRole(SPListItem listItem, SPPrincipal principal, SPRoleType roleType)
        {
            AddRole(listItem.Web, listItem, principal, roleType);
        }

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="list">The list to add the role to.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleType">Type of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AddRole(SPList list, SPPrincipal principal, SPRoleType roleType)
        {
            AddRole(list.ParentWeb, list, principal, roleType);
        }

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="web">The web to add the role to.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleType">Type of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AddRole(SPWeb web, SPPrincipal principal, SPRoleType roleType)
        {
            AddRole(web, web, principal, roleType);
        }

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="listItem">The list item.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleDefinitionName">Name of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AddRole(SPListItem listItem, SPPrincipal principal, string roleDefinitionName)
        {
            AddRole(listItem.Web, listItem, principal, roleDefinitionName);
        }

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="list">The list to add the role to.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleDefinitionName">Name of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AddRole(SPList list, SPPrincipal principal, string roleDefinitionName)
        {
            AddRole(list.ParentWeb, list, principal, roleDefinitionName);
        }

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="web">The web to add the role to.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleDefinitionName">Name of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void AddRole(SPWeb web, SPPrincipal principal, string roleDefinitionName)
        {
            AddRole(web, web, principal, roleDefinitionName);
        }

        /// <summary>
        /// Removes the role.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleType">Type of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void RemoveRole(SPSecurableObject target, SPPrincipal principal, SPRoleType roleType)
        {
            if (roleType == SPRoleType.None)
            {
                throw new ArgumentException("Removing custom RoleDefinitions is not supported.");
            }

            var assignment = target.RoleAssignments.Cast<SPRoleAssignment>().FirstOrDefault(x => x.Member.ID == principal.ID && x.RoleDefinitionBindings.Cast<SPRoleDefinition>().Any(r => r.Type == roleType));
            if (assignment != null)
            {
                EnsureBrokenRoleInheritance(target);

                assignment = target.RoleAssignments.Cast<SPRoleAssignment>().FirstOrDefault(x => x.Member.ID == principal.ID);

                foreach (var role in assignment.RoleDefinitionBindings.Cast<SPRoleDefinition>().Where(x => x.Type == roleType).ToArray())
                {
                    assignment.RoleDefinitionBindings.Remove(role);
                }

                assignment.Update();
            }
        }

        /// <summary>
        /// Remove all roles the principal has on the target
        /// </summary>
        /// <param name="target">The securable object</param>
        /// <param name="principal">The security principal</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        public void RemoveAllRoles(SPSecurableObject target, SPPrincipal principal)
        {
            // Break the Role Inheritance if it's necessary
            EnsureBrokenRoleInheritance(target);

            // remove the roleAssignment from the item
            target.RoleAssignments.Remove(principal);
        }

        /// <summary>
        /// Find a role definition by its role type
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="roleType">The role type</param>
        /// <returns>The role definition</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        public SPRoleDefinition GetRoleDefinitionByRoleType(SPWeb web, SPRoleType roleType)
        {
            return web.RoleDefinitions.Cast<SPRoleDefinition>().First(x => x.Type == roleType);
        }

        /// <summary>
        /// Method to copy all roleAssignment from one item to the other. It completely override all permission in the target.
        /// </summary>
        /// <param name="source">The reference ListItem or other securable object</param>
        /// <param name="target">The destination ListItem to modify exactly like the source</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        public void CopyRoleToItem(SPSecurableObject source, SPListItem target)
        {
            EnsureBrokenRoleInheritance(target);

            // Remove current role assignemnts
            while (target.RoleAssignments.Count > 0)
            {
                target.RoleAssignments.Remove(0);
            }

            target.Update();

            // Copy Role Assignments from source to destination list.
            foreach (SPRoleAssignment sourceRole in source.RoleAssignments)
            {
                target.RoleAssignments.Add(sourceRole);
            }

            // Ensure item update metadata is not affected.
            target.SystemUpdate(false);
        }

        /// <summary>
        /// Checks if the user is member of the visitors group
        /// and also makes sure the user can't edit the current
        /// list item.
        /// </summary>
        /// <returns>True is member of visitor's group and can't edit current list item, false otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        public bool IsCurrentUserVisitor()
        {
            var currentWeb = SPContext.Current.Web;
            var currentUser = currentWeb.CurrentUser;

            // Is an Anonymous user
            if (currentUser == null)
            {
                return true;
            }

            bool isReadOnlyOnCurrentListItem = (SPContext.Current.ListItem == null) || (SPContext.Current.ListItem != null
                                 && !SPContext.Current.ListItem.DoesUserHavePermissions(SPContext.Current.Web.CurrentUser, SPBasePermissions.EditListItems));
            return isReadOnlyOnCurrentListItem
                && !this.DoesCurrentUserHaveFullControl()
                && !this.CanCurrentUserApprove()
                && !this.CanCurrentUserContribute();
        }

        /// <summary>
        /// Checks if the user is member of the members group.
        /// </summary>
        /// <returns>True of part of site's associated member group, false otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        public bool CanCurrentUserContribute()
        {
            // Is an Anonymous user
            if (SPContext.Current.Web.CurrentUser == null)
            {
                return false;
            }

            // Check the current list item, then the current folder.
            // Somewhat weird, but we assume also that if you have the perms on the Pages
            // library, then you should be granted those permissions throughout.
            return DoesUserHavePermissionOnCurrentListItem(SPBasePermissions.AddListItems)
                || DoesUserHavePermissionOnCurrentFolder(SPBasePermissions.AddListItems)
                || DoesUserHavePermissionOnPagesLibrary(SPBasePermissions.AddListItems);
        }

        /// <summary>
        /// Checks if the user is member of the approvers group
        /// </summary>
        /// <returns>True of part of site's associated owners group, false otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        public bool CanCurrentUserApprove()
        {
            // Is an Anonymous user
            if (SPContext.Current.Web.CurrentUser == null)
            {
                return false;
            }

            // Check the current list item, then the current folder.
            // Somewhat weird, but we assume also that if you have the perms on the Pages
            // library, then you should be granted those permissions throughout.
            return DoesUserHavePermissionOnCurrentListItem(SPBasePermissions.ApproveItems)
                || DoesUserHavePermissionOnCurrentFolder(SPBasePermissions.ApproveItems)
                || DoesUserHavePermissionOnPagesLibrary(SPBasePermissions.ApproveItems);
        }

        /// <summary>
        /// Checks if the user is member of the members group.
        /// </summary>
        /// <returns>True is member of visitor's group and can't edit current list item, false otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        public bool DoesCurrentUserHaveFullControl()
        {
            // Is an Anonymous user
            if (SPContext.Current.Web.CurrentUser == null)
            {
                return false;
            }
            
            return SPContext.Current.Web.DoesUserHavePermissions(SPBasePermissions.FullMask);
        }

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="web">The web containing the role definitions.</param>
        /// <param name="target">The target.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleType">Type of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        internal static void AddRole(SPWeb web, SPSecurableObject target, SPPrincipal principal, SPRoleType roleType)
        {
            if (roleType == SPRoleType.None)
            {
                throw new ArgumentException("Adding custom RoleDefinitions is not supported.");
            }

            var roleToAdd = web.RoleDefinitions.Cast<SPRoleDefinition>().FirstOrDefault(x => x.Type == roleType);
            if (roleToAdd != null)
            {
                EnsureBrokenRoleInheritance(target);

                var assignments = target.RoleAssignments.Cast<SPRoleAssignment>().FirstOrDefault(x => x.Member.ID == principal.ID);
                if (assignments == null)
                {
                    assignments = new SPRoleAssignment(principal);
                    assignments.RoleDefinitionBindings.Add(roleToAdd);
                    target.RoleAssignments.Add(assignments);
                }
                else
                {
                    assignments.RoleDefinitionBindings.Add(roleToAdd);
                    assignments.Update();
                }
            }
            else
            {
                throw new ArgumentException("No RoleDefinition found for type " + roleType);
            }
        }

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="web">The web containing the role definitions.</param>
        /// <param name="target">The target.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleDefinitionName">Name of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        internal static void AddRole(SPWeb web, SPSecurableObject target, SPPrincipal principal, string roleDefinitionName)
        {
            var roleToAdd = web.RoleDefinitions.Cast<SPRoleDefinition>().FirstOrDefault(x => x.Name == roleDefinitionName);
            if (roleToAdd != null)
            {
                EnsureBrokenRoleInheritance(target);

                var assignments = target.RoleAssignments.Cast<SPRoleAssignment>().FirstOrDefault(x => x.Member.ID == principal.ID);
                if (assignments == null)
                {
                    assignments = new SPRoleAssignment(principal);
                    assignments.RoleDefinitionBindings.Add(roleToAdd);
                    target.RoleAssignments.Add(assignments);
                }
                else
                {
                    assignments.RoleDefinitionBindings.Add(roleToAdd);
                    assignments.Update();
                }
            }
            else
            {
                throw new ArgumentException("No RoleDefinition found for the name " + roleDefinitionName);
            }
        }

        private static void EnsureBrokenRoleInheritance(SPSecurableObject target)
        {
            if (!target.HasUniqueRoleAssignments)
            {
                target.BreakRoleInheritance(true, false);
            }
        }

        private static bool DoesUserHavePermissionOnPagesLibrary(SPBasePermissions permissionsMask)
        {
            bool hasPermissions = false;

            var pagesLibrary = SPContext.Current.Web.GetPagesLibrary();
            if (pagesLibrary != null)
            {
                hasPermissions = pagesLibrary.DoesUserHavePermissions(permissionsMask);
            }

            return hasPermissions;
        }

        private static bool DoesUserHavePermissionOnCurrentListItem(SPBasePermissions permissionsMask)
        {
            return SPContext.Current.ListItem != null
                && SPContext.Current.ListItem.DoesUserHavePermissions(SPContext.Current.Web.CurrentUser, permissionsMask);
        }

        private static bool DoesUserHavePermissionOnCurrentFolder(SPBasePermissions permissionsMask)
        {
            bool hasPermissionsOnCurrentRootFolderUrl = false;

            if (!string.IsNullOrEmpty(SPContext.Current.RootFolderUrl))
            {
                SPFolder currentRootFolder = SPContext.Current.Web.GetFolder(SPContext.Current.RootFolderUrl);

                if (currentRootFolder != null
                    && currentRootFolder.Item != null
                    && currentRootFolder.Item.DoesUserHavePermissions(permissionsMask))
                {
                    hasPermissionsOnCurrentRootFolderUrl = true;
                }
            }

            return hasPermissionsOnCurrentRootFolderUrl
                || (SPContext.Current.ListItem != null
                    && SPContext.Current.File != null
                    && SPContext.Current.File.ParentFolder != null
                    && SPContext.Current.File.ParentFolder.Item != null
                    && SPContext.Current.File.ParentFolder.Item.DoesUserHavePermissions(SPContext.Current.Web.CurrentUser, permissionsMask));
        }
    }
}
