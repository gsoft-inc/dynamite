namespace GSoft.Dynamite.Security
{
    using System.Collections.Generic;

    using Microsoft.SharePoint;

    /// <summary>
    /// SharePoint User Helper
    /// </summary>
    public interface IUserHelper
    {
        /// <summary>
        /// Get user SharePoint groups membership (bypass AD groups in SharePoint groups)
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="userName">The user name.</param>
        /// <returns>List of groups.</returns>
        ICollection<string> GetUserSharePointGroups(SPWeb web, string userName);

        /// <summary>
        /// Gets the users of the SharePoint principal.
        /// This method will also check users in the active directory groups.
        /// </summary>
        /// <param name="principal">The SharePoint principal.</param>
        /// <returns>A list of the SPUsers in the SharePoint principal</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Principal is null
        /// </exception>
        IList<SPUser> GetUsersInPrincipal(SPPrincipal principal);

        /// <summary>
        /// Determines whether The specified user is part of the specified SharePoint user group.
        /// This method will also check users in the active directory groups if any are in the SharePoint group.
        /// </summary>
        /// <param name="user">The SharePoint user.</param>
        /// <param name="group">The SharePoint group.</param>
        /// <returns>True if the user is part of the specified group.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// user or group is null
        /// </exception>
        bool IsUserInGroup(SPUser user, SPGroup group);
    }
}