using System;
using System.Collections.Specialized;
using System.Linq;
using GSoft.Dynamite.Extensions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Email
{
    /// <summary>
    /// Class used to send emails using the SharePoint API.
    /// </summary>
    public class EmailHelper : IEmailHelper
    {
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

        private static StringDictionary GetEmailHeaders(EmailInfo emailInfo)
        {
            // Make sure the email will be sent to someone
            if (!emailInfo.To.Any() && !emailInfo.CarbonCopy.Any() && !emailInfo.BlindCarbonCopy.Any())
            {
                throw new InvalidOperationException("When sending an email make sure to include one of the following pieces of information: To, Carbon Copy, or BlindCarbonCopy.");
            }

            var headers = emailInfo.OtherHeaders;

            if (emailInfo.To.Any())
            {
                headers.Add("to", string.Join(",", emailInfo.To));
            }

            if (emailInfo.CarbonCopy.Any())
            {
                headers.Add("cc", string.Join(",", emailInfo.CarbonCopy));
            }

            if (emailInfo.BlindCarbonCopy.Any())
            {
                headers.Add("bcc", string.Join(",", emailInfo.BlindCarbonCopy));
            }

            if (!string.IsNullOrEmpty(emailInfo.From))
            {
                headers.Add("from", emailInfo.From);
            }

            if (!string.IsNullOrEmpty(emailInfo.Subject))
            {
                headers.Add("subject", emailInfo.Subject);
            }

            switch (emailInfo.Priority)
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
