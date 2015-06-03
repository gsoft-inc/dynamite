using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace GSoft.Dynamite.Email
{
    /// <summary>
    /// Information used in while sending emails.
    /// </summary>
    public class EmailInfo
    {
        private IList<string> to = new List<string>();
        private IList<string> carbonCopy = new List<string>();
        private IList<string> blindCarbonCopy = new List<string>();
        private StringDictionary otherHeaders = new StringDictionary();

        /// <summary>
        /// The email to show who the email is from.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// List of email addresses used when sending the email.
        /// </summary>
        public IList<string> To
        {
            get { return this.to; }
        }
        
        /// <summary>
        /// List of email addresses to include in carbon copy to the email.
        /// </summary>
        public IList<string> CarbonCopy 
        { 
            get { return this.carbonCopy; } 
        }

        /// <summary>
        /// List of email addresses to include in blind carbon copy to the email.
        /// </summary>
        public IList<string> BlindCarbonCopy
        {
            get { return this.blindCarbonCopy; }
        }

        /// <summary>
        /// The subject of the email to send.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The body of the email. The body can contain html.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The priority the email will be marked with.
        /// </summary>
        public EmailPriorityType Priority { get; set; }

        /// <summary>
        /// These headers will be added to the list of automatically generated headers.
        /// </summary>
        public StringDictionary OtherHeaders
        {
            get { return this.otherHeaders; }
        }
    }
}
