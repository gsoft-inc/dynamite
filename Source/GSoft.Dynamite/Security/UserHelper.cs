using System.Collections.Generic;
using System.Linq;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Security
{
    /// <summary>
    /// SharePoint User Helper Class
    /// </summary>
    public class UserHelper
    {
        /// <summary>
        /// Get user SharePoint groups membership (bypass AD groups in SharePoint groups)
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="userName">The user name.</param>
        /// <returns>List of groups.</returns>
        public List<string> GetUserSharePointGroups(SPWeb web, string userName)
        {
            return (from SPGroup @group in web.Groups let isMember = this.FetchUserFromGroup(web, @group.Name, userName) where isMember select @group.Name).ToList();
        }

        /// <summary>
        /// Determines if the user is in the specified group
        /// </summary>
        /// <param name="web">Current web.</param>
        /// <param name="grpName">The group name.</param>
        /// <param name="userName">The user name.</param>
        /// <returns>True if the user is in the group, false otherwise.</returns>
        private bool FetchUserFromGroup(SPWeb web, string grpName, string userName)
        {
            bool isMember = false;

            SPSecurity.RunWithElevatedPrivileges(
                () =>
                    {
                        // Reinit SharePoint context
                        using (var newSite = new SPSite(web.Site.ID))
                        {
                            using (var newWeb = newSite.OpenWeb(web.ID))
                            {
                                bool hasMaxCount;

                                // Be careful, this method return AD groups too regardless to their permissions in the current SharePoint site
                                SPPrincipalInfo[] peoples = SPUtility.GetPrincipalsInGroup(newWeb, grpName, 1000, out hasMaxCount);

                                if (peoples != null)
                                {
                                    foreach (SPPrincipalInfo people in peoples)
                                    {
                                        if (people.PrincipalType == SPPrincipalType.SecurityGroup)
                                        {
                                            // If its a security group, make recursive calls
                                            isMember = FetchUserFromGroup(newWeb, people.LoginName, userName);
                                        }
                                        else
                                        {
                                            if (string.CompareOrdinal(
                                                people.LoginName.ToUpperInvariant(), userName.ToUpperInvariant()) == 0
                                                && isMember == false)
                                            {
                                                isMember = true;
                                            }
                                        }
                                    }
                                } 
                            }
                        }                   
                    });

            return isMember;
        }
    }
}
