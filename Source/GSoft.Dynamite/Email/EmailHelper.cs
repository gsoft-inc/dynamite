using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using GSoft.Dynamite.Configuration;
using GSoft.Dynamite.Extensions;
using GSoft.Dynamite.Security;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Email
{
    /// <summary>
    /// Class used to send emails using the SharePoint API.
    /// </summary>
    public class EmailHelper : IEmailHelper
    {
        private const string FailsafePropertyBagKey = "DynamiteEmailFailsafeAddress";
        private readonly IUserHelper userHelper;
        private readonly IPropertyBagHelper propertyBagHelper;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailHelper" /> class.
        /// </summary>
        /// <param name="userHelper">The user helper.</param>
        /// <param name="propertyBagHelper">The property bag helper.</param>
        public EmailHelper(IUserHelper userHelper, IPropertyBagHelper propertyBagHelper)
        {
            this.userHelper = userHelper;
            this.propertyBagHelper = propertyBagHelper;
        }

        /// <summary>
        /// Send an email depending on the specified email information.
        /// </summary>
        /// <param name="web">The SharePoint Web.</param>
        /// <param name="emailInformation">The email information.</param>
        public void SendEmail(SPWeb web, EmailInfo emailInformation)
        {
            if (this.IsFailsafeEnabled(web.Site.WebApplication))
            {
                emailInformation.Body = GetFailsafeMessage(emailInformation) + emailInformation.Body;
                emailInformation.To.Clear();
                emailInformation.To.Add(this.propertyBagHelper.GetWebApplicationValue(web.Site.WebApplication, FailsafePropertyBagKey));
                emailInformation.CarbonCopy.Clear();
                emailInformation.BlindCarbonCopy.Clear();
            }

            var headers = EmailHelper.GetEmailHeaders(emailInformation);
            web.RunAsSystem(elevatedWeb =>
            {
                SPUtility.SendEmail(elevatedWeb, headers, emailInformation.Body);
            });
        }

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
        public void EnableFailsafe(SPWebApplication webApplication, string emailAddress)
        {
            var uri = webApplication.AlternateUrls[0].Uri;
            var property = new PropertyBagValue()
            {
                Indexed = false,
                Key = FailsafePropertyBagKey,
                Overwrite = true,
                Value = emailAddress
            };

            this.propertyBagHelper.SetWebApplicationValue(uri, new List<PropertyBagValue>() { property });
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

        /// <summary>
        /// Is the email Failsafe enabled.
        /// </summary>
        /// <param name="webApplication">The web application to check.</param>
        /// <returns>True if the Failsafe is activated for the specified web application.</returns>
        public bool IsFailsafeEnabled(SPWebApplication webApplication)
        {
            var value = this.propertyBagHelper.GetWebApplicationValue(webApplication, FailsafePropertyBagKey);
            return !string.IsNullOrEmpty(value);
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

        private static string GetFailsafeMessage(EmailInfo emailInformation)
        {
            var originalTo = emailInformation.To.ToList();
            var originalCC = emailInformation.CarbonCopy.ToList();
            var originalBCC = emailInformation.BlindCarbonCopy.ToList();

            var builder = new StringBuilder();
            builder.Append("<p style=\"color:red; font-weight: bold; font-size: 20px;\">This email was send using a Failsafe in order to not accidentally spam people who should not receive emails from a development environment.</p>");
            builder.Append("<table>");
            builder.Append("<tr>");
            builder.Append("<th>Original To:</th>");
            builder.Append("<th>Original CC:</th>");
            builder.Append("<th>Original BCC:</th>");
            builder.Append("</tr>");
            builder.Append("<tr>");

            AppendEmailList(builder, originalTo);
            AppendEmailList(builder, originalCC);
            AppendEmailList(builder, originalBCC);

            builder.Append("</tr>");
            builder.Append("</table>");
            builder.Append("<hr/>");
            builder.Append("<p style=\"text-align: center;\">Start of original message contents</p>");
            builder.Append("<hr/>");

            return builder.ToString();
        }

        private static void AppendEmailList(StringBuilder builder, List<string> emails)
        {
            builder.Append("<td>");
            builder.Append("<ul>");

            emails.ForEach(e => builder.AppendFormat("<li>{0}</li>", e));

            builder.Append("</ul>");
            builder.Append("</td>");
        }
    }
}
