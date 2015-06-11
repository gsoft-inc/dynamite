using System;
using System.Collections.Generic;
using System.Linq;
using GSoft.Dynamite.Extensions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Security
{
    /// <summary>
    /// SharePoint User Helper Class
    /// </summary>
    public class UserHelper : IUserHelper
    {
        /// <summary>
        /// Get user SharePoint groups membership (bypass AD groups in SharePoint groups)
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="userName">The user name.</param>
        /// <returns>List of groups.</returns>
        public ICollection<string> GetUserSharePointGroups(SPWeb web, string userName)
        {
            return (from SPGroup @group in web.Groups let isMember = this.IsUserInGroup(web.EnsureUser(userName), @group) where isMember select @group.Name).ToList();
        }

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
        public bool IsUserInGroup(SPUser user, SPGroup group)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            var usersInGroup = this.GetUsersInPrincipal(group);
            return HasUserInList(usersInGroup, user);
        }

        /// <summary>
        /// Gets the users of the SharePoint principal.
        /// This method will also check users in the active directory groups.
        /// </summary>
        /// <param name="principal">The SharePoint principal.</param>
        /// <returns>A list of the SPUsers in the SharePoint principal</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Principal is null
        /// </exception>
        public IList<SPUser> GetUsersInPrincipal(SPPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            List<SPUser> allUsers = new List<SPUser>();

            principal.ParentWeb.RunAsSystem(elevatedWeb =>
            {
                SPUser user = principal as SPUser;
                if (user != null)
                {
                    if (user.IsDomainGroup)
                    {
                        bool reachedMaxCount;

                        // Be careful, this method return AD groups too regardless to their permissions in the current SharePoint site
                        SPPrincipalInfo[] groupMembers = SPUtility.GetPrincipalsInGroup(elevatedWeb, principal.LoginName, 9999, out reachedMaxCount);
                        if (groupMembers != null)
                        {
                            foreach (SPPrincipalInfo member in groupMembers)
                            {
                                switch (member.PrincipalType)
                                {
                                    case SPPrincipalType.SecurityGroup:
                                    case SPPrincipalType.DistributionList:
                                        {
                                            var usersInPrincipal = GetUsersInPrincipal(elevatedWeb.EnsureUser(member.LoginName));

                                            // Only add users to the all users list if they are not already there.
                                            allUsers.AddRange(usersInPrincipal.Where(u => !HasUserInList(allUsers, u)));
                                            break;
                                        }

                                    case SPPrincipalType.User:
                                        {
                                            var memberUser = elevatedWeb.EnsureUser(member.LoginName);
                                            if (!HasUserInList(allUsers, memberUser))
                                            {
                                                allUsers.Add(memberUser);
                                            }

                                            break;
                                        }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Only add the user to the all users list if they are not already there.
                        if (!HasUserInList(allUsers, user))
                        {
                            allUsers.Add(user);
                        }
                    }
                }
                else
                {
                    SPGroup group = principal as SPGroup;
                    foreach (SPUser groupUser in group.Users)
                    {
                        var usersInPrincipal = GetUsersInPrincipal(groupUser);

                        // Only add users to the all users list if they are not already there.
                        allUsers.AddRange(usersInPrincipal.Where(u => !HasUserInList(allUsers, u)));
                    }
                }
            });

            return allUsers;
        }

        private static bool HasUserInList(IList<SPUser> users, SPUser user)
        {
            return users.Any(u => u.ID == user.ID);
        }
    }
}
