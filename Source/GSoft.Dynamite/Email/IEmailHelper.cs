using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

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
        /// Enables the email Failsafe for the specified web application.
        /// When this Failsafe is Enabled, all emails send with this helper will only be send to the specified address clearing all original To, CC, and BCC addresses
        /// and a message will be added to the top of the email body listing the original To, CC, and BCC email addresses.
        /// </summary>
        /// <param name="webApplication">The web application.</param>
        /// <param name="emailAddress">
        /// The email address.
        /// Setting this to an empty string will disable the Failsafe.
        /// </param>
        void EnableFailsafe(SPWebApplication webApplication, string emailAddress);

        /// <summary>
        /// Adds the group members (including AD group members) to the 'To' property of the email information.
        /// Only users who have emails are added.
        /// </summary>
        /// <param name="group">The SharePoint group.</param>
        /// <param name="emailInformation">The email information.</param>
        void AddGroupMembersToRecipients(SPGroup group, EmailInfo emailInformation);

        /// <summary>
        /// Is the email Failsafe enabled.
        /// </summary>
        /// <param name="webApplication">The web application to check.</param>
        /// <returns>True if the Failsafe is activated for the specified web application.</returns>
        bool IsFailsafeEnabled(SPWebApplication webApplication);
    }
}
