using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using GSoft.Dynamite.Extensions;
using GSoft.Dynamite.Security;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Email
{
    /// <summary>
    /// Class used to send emails using the SharePoint API.
    /// </summary>
    public class EmailHelper : IEmailHelper
    {
        private readonly IUserHelper userHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailHelper"/> class.
        /// </summary>
        /// <param name="userHelper">The user helper.</param>
        public EmailHelper(IUserHelper userHelper)
        {
            this.userHelper = userHelper;
        }

        /// <summary>
        /// Send an email depending on the specified email information.
        /// </summary>
        /// <param name="web">The SharePoint Web.</param>
        /// <param name="emailInformation">The email information.</param>
        public void SendEmail(SPWeb web, EmailInfo emailInformation)
        {
            var headers = EmailHelper.GetEmailHeaders(emailInformation);
            web.RunAsSystem(elevatedWeb =>
            {
                SPUtility.SendEmail(elevatedWeb, headers, emailInformation.Body);
            });
        }

        /// <summary>
        /// Adds the group members (including AD group members) to the 'To' property of the email information.
        /// Only users who have emails are added.
        /// </summary>
        /// <param name="group">The SharePoint group.</param>
        /// <param name="emailInformation">The email information.</param>
        public void AddGroupMembersToRecipients(SPGroup group, EmailInfo emailInformation)
        {
            var users = this.userHelper.GetUsersInPrincipal(group);
            var userEmails = users.Where(u => !string.IsNullOrEmpty(u.Email)).Select(u => u.Email).ToList();
            userEmails.ForEach(ue => emailInformation.To.Add(ue));
        }

        private static StringDictionary GetEmailHeaders(EmailInfo emailInformation)
        {
            // Make sure the email will be sent to someone
            if (!emailInformation.To.Any() && !emailInformation.CarbonCopy.Any() && !emailInformation.BlindCarbonCopy.Any())
            {
                throw new InvalidOperationException("When sending an email make sure to include one of the following pieces of information: To, Carbon Copy, or BlindCarbonCopy.");
            }

            var headers = emailInformation.OtherHeaders;

            if (emailInformation.To.Any())
            {
                headers.Add("to", string.Join(",", emailInformation.To));
            }

            if (emailInformation.CarbonCopy.Any())
            {
                headers.Add("cc", string.Join(",", emailInformation.CarbonCopy));
            }

            if (emailInformation.BlindCarbonCopy.Any())
            {
                headers.Add("bcc", string.Join(",", emailInformation.BlindCarbonCopy));
            }

            if (!string.IsNullOrEmpty(emailInformation.From))
            {
                headers.Add("from", emailInformation.From);
            }

            if (!string.IsNullOrEmpty(emailInformation.Subject))
            {
                headers.Add("subject", emailInformation.Subject);
            }

            switch (emailInformation.Priority)
            {
                case EmailPriorityType.High:
                    headers.Add("Importance", "high");
                    headers.Add("X-Priority", "1");
                    break;

                case EmailPriorityType.Low:
                    headers.Add("Importance", "low");
                    headers.Add("X-Priority", "5");
                    break;
            }

            return headers;
        }
    }
}
