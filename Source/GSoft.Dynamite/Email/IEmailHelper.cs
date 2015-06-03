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
    }
}
