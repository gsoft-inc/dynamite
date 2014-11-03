namespace GSoft.Dynamite.Security
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;

    /// <summary>
    /// Helper for managing Role-based security.
    /// </summary>
    public interface ISecurityHelper
    {
        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="listItem">The list item.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleType">Type of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AddRole(SPListItem listItem, SPPrincipal principal, SPRoleType roleType);

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="list">The list to add the role to.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleType">Type of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AddRole(SPList list, SPPrincipal principal, SPRoleType roleType);

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="web">The web to add the role to.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleType">Type of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AddRole(SPWeb web, SPPrincipal principal, SPRoleType roleType);

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="listItem">The list item.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleDefinitionName">Name of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AddRole(SPListItem listItem, SPPrincipal principal, string roleDefinitionName);

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="list">The list to add the role to.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleDefinitionName">Name of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AddRole(SPList list, SPPrincipal principal, string roleDefinitionName);

        /// <summary>
        /// Adds the role.
        /// </summary>
        /// <param name="web">The web to add the role to.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleDefinitionName">Name of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void AddRole(SPWeb web, SPPrincipal principal, string roleDefinitionName);

        /// <summary>
        /// Removes the role.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="principal">The principal.</param>
        /// <param name="roleType">Type of the role.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        void RemoveRole(SPSecurableObject target, SPPrincipal principal, SPRoleType roleType);

        /// <summary>
        /// Remove all roles the principal has on the target
        /// </summary>
        /// <param name="target">The securable object</param>
        /// <param name="principal">The security principal</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        void RemoveAllRoles(SPSecurableObject target, SPPrincipal principal);

        /// <summary>
        /// Find a role definition by its role type
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="roleType">The role type</param>
        /// <returns>The role definition</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        SPRoleDefinition GetRoleDefinitionByRoleType(SPWeb web, SPRoleType roleType);

        /// <summary>
        /// Method to copy all roleAssignment from one item to the other. It completely override all permission in the target.
        /// </summary>
        /// <param name="source">The reference ListItem or other securable object</param>
        /// <param name="target">The destination ListItem to modify exactly like the source</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        void CopyRoleToItem(SPSecurableObject source, SPListItem target);

        /// <summary>
        /// Checks if the user is member of the visitors group
        /// and also makes sure the user can't edit the current
        /// list item.
        /// </summary>
        /// <returns>True is member of visitor's group and can't edit current list item, false otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        bool IsCurrentUserVisitor();

        /// <summary>
        /// Checks if the user is member of the members group.
        /// </summary>
        /// <returns>True of part of site's associated member group, false otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        bool CanCurrentUserContribute();

        /// <summary>
        /// Checks if the user is member of the approvers group
        /// </summary>
        /// <returns>True of part of site's associated owners group, false otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        bool CanCurrentUserApprove();

        /// <summary>
        /// Checks if the user is member of the members group.
        /// </summary>
        /// <returns>True is member of visitor's group and can't edit current list item, false otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        bool DoesCurrentUserHaveFullControl();
    }
}