using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Globalization;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// An entity type for a user.
    /// </summary>
    public class UserValue : PrincipalValue
    {
        #region Constructors
        
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public UserValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserValue"/> class.
        /// </summary>
        /// <param name="principalId">The principal id.</param>
        public UserValue(int principalId)
            : base(principalId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserValue"/> class.
        /// </summary>
        /// <param name="loginName">The principal login name.</param>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Domain language.")]
        public UserValue(string loginName)
            : base(loginName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserValue"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        public UserValue(SPUser user)
            : base(user)
        {
            this.Email = user.Email;
            this.IsDomainGroup = user.IsDomainGroup;
            this.IsSiteAdmin = user.IsSiteAdmin;
            this.IsSiteAuditor = user.IsSiteAuditor;
            this.Notes = user.Notes;
            this.Sid = user.Sid;

            // If the user doesn't have any regional settings of his own, that means he's defaulting to the current Web's settings
            this.RegionalSettings = new RegionalSettings(user.RegionalSettings ?? user.ParentWeb.RegionalSettings);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is domain group.
        /// </summary>
        public bool IsDomainGroup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is site admin.
        /// </summary>
        public bool IsSiteAdmin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is site auditor.
        /// </summary>
        public bool IsSiteAuditor { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the regional settings.
        /// </summary>
        public RegionalSettings RegionalSettings { get; set; }

        /// <summary>
        /// Gets or sets the sid.
        /// </summary>
        public string Sid { get; set; }

        #endregion
    }
}