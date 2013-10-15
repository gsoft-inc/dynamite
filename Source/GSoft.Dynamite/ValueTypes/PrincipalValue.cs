using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// A SharePoint principal.
    /// </summary>
    public class PrincipalValue
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PrincipalValue"/> class.
        /// </summary>
        /// <param name="principalId">The principal id.</param>
        public PrincipalValue(int principalId)
        {
            this.Id = principalId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrincipalValue"/> class.
        /// </summary>
        /// <param name="principal">The principal.</param>
        internal PrincipalValue(SPPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            this.Id = principal.ID;
            this.DisplayName = principal.Name;
            this.LoginName = principal.LoginName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets the name of the login.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Domain language.")]
        public string LoginName { get; private set; }

        #endregion
    }
}