using System;
using System.Collections.Generic;
using GSoft.Dynamite.Taxonomy;
using Microsoft.Office.Server.UserProfiles;

namespace GSoft.Dynamite.UserProfile
{
    /// <summary>
    /// User profile property information.
    /// </summary>
    public class UserProfilePropertyInfo
    {
        private readonly IDictionary<int, string> displayNameLocalized = new Dictionary<int, string>();
        private readonly IDictionary<int, string> descriptionsLocalized = new Dictionary<int, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfilePropertyInfo"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="type">The type (PropertyDataType).</param>
        public UserProfilePropertyInfo(string name, string displayName, string type) :
            this(name, displayName, type, 25, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfilePropertyInfo"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="type">The type (PropertyDataType).</param>
        /// <param name="length">The length.</param>
        public UserProfilePropertyInfo(string name, string displayName, string type, int length) : 
            this(name, displayName, type, length, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfilePropertyInfo"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="type">The type (PropertyDataType).</param>
        /// <param name="length">The length.</param>
        /// <param name="isMultivalued">if set to <c>true</c> [is multivalued].</param>
        public UserProfilePropertyInfo(string name, string displayName, string type, int length, bool isMultivalued)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentNullException("displayName");
            }

            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("type");
            }

            this.Name = name;
            this.DisplayName = displayName;
            this.Length = length;
            this.PropertyDataType = type;
            this.IsMultivalued = isMultivalued;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; private set; }

        /// <summary>
        /// Gets or sets the property data type (use PropertyDataType class).
        /// </summary>
        /// <value>
        /// The property data type.
        /// </value>
        public string PropertyDataType { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is multivalued].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is multivalued]; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultivalued { get; private set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the localized display names (LCID and name key/value pairs).
        /// </summary>
        /// <value>
        /// The localized display names.
        /// </value>
        public IDictionary<int, string> DisplayNameLocalized 
        {
            get
            {
                return this.displayNameLocalized;
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the localized descriptions (LCID and name key/value pairs).
        /// </summary>
        /// <value>
        /// The localized descriptions.
        /// </value>
        public IDictionary<int, string> DescriptionLocalized
        {
            get
            {
                return this.descriptionsLocalized;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is alias].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is alias]; otherwise, <c>false</c>.
        /// </value>
        public bool IsAlias { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is searchable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is searchable]; otherwise, <c>false</c>.
        /// </value>
        public bool IsSearchable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is visible on viewer].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is visible on viewer]; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisibleOnViewer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is visible on editor].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is visible on editor]; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisibleOnEditor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is user editable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is user editable]; otherwise, <c>false</c>.
        /// </value>
        public bool IsUserEditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is replicable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is replicable]; otherwise, <c>false</c>.
        /// </value>
        public bool IsReplicable { get; set; }

        /// <summary>
        /// Gets or sets the separator.
        /// </summary>
        /// <value>
        /// The separator.
        /// </value>
        public MultiValueSeparator Separator { get; set; }

        /// <summary>
        /// Gets or sets the term set.
        /// </summary>
        /// <value>
        /// The term set.
        /// </value>
        public TermSetInfo TermSetInfo { get; set; }
    }
}
