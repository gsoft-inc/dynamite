using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Email
{
    /// <summary>
    /// Definition of the methods used to manage emails.
    /// </summary>
    public interface IEmailHelper
    {
        /// <summary>
        /// Send an email depending on the specified email information.
        /// </summary>
        /// <param name="web">The SharePoint Web.</param>
        /// <param name="emailInformation">The email information.</param>
        void SendEmail(SPWeb web, EmailInfo emailInformation);

        /// <summary>
        /// Adds the group members (including AD group members) to the 'To' property of the email information.
        /// Only users who have emails are added.
        /// </summary>
        /// <param name="group">The SharePoint group.</param>
        /// <param name="emailInformation">The email information.</param>
        void AddGroupMembersToRecipients(SPGroup group, EmailInfo emailInformation);
    }
}
