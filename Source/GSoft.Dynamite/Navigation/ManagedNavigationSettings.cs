using System;
using System.Xml.Linq;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Settings for managed navigation on an SPWeb
    /// </summary>
    [Obsolete("Use NavigationSettingsInfo instead")]
    public class ManagedNavigationSettings
    {
        private string _termStoreName = "Managed Metadata Service";

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedNavigationSettings"/> class.
        /// </summary>
        public ManagedNavigationSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedNavigationSettings"/> class.
        /// </summary>
        /// <param name="managedNavigationSettingsXml">The managed navigation settings XML.</param>
        public ManagedNavigationSettings(XElement managedNavigationSettingsXml)
        {
            this.AddNewPagesToNavigation = bool.Parse(managedNavigationSettingsXml.Attribute("AddNewPagesToNavigation").Value);
            var termStoreNameAttribute = managedNavigationSettingsXml.Attribute("TermStoreName");
            if (termStoreNameAttribute != null)
            {
                this.TermStoreName = termStoreNameAttribute.Value;
            }

            this.TermGroupName = managedNavigationSettingsXml.Attribute("TermGroupName").Value;
            this.TermSetName = managedNavigationSettingsXml.Attribute("TermSetName").Value;

            var taggingAttribute = managedNavigationSettingsXml.Attribute("PreserveTaggingOnTermSet");
            if (taggingAttribute != null)
            {
                this.PreserveTaggingOnTermSet = bool.Parse(taggingAttribute.Value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to [add new pages to navigation].
        /// </summary>
        /// <value>
        /// <c>true</c> if [add new pages to navigation]; otherwise, <c>false</c>.
        /// </value>
        public bool AddNewPagesToNavigation { get; set; }

        /// <summary>
        /// Gets or sets the name of the term store.
        /// </summary>
        /// <value>
        /// The name of the term store.
        /// </value>
        public string TermStoreName
        {
            get { return this._termStoreName; }
            set { this._termStoreName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the term group.
        /// </summary>
        /// <value>
        /// The name of the term group.
        /// </value>
        public string TermGroupName { get; set; }

        /// <summary>
        /// Gets or sets the name of the term set.
        /// </summary>
        /// <value>
        /// The name of the term set.
        /// </value>
        public string TermSetName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [preserve tagging on term set].
        /// </summary>
        /// <value>
        /// <c>true</c> if [preserve tagging on term set]; otherwise, <c>false</c>.
        /// </value>
        public bool PreserveTaggingOnTermSet { get; set; }
    }
}
