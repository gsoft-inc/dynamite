using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.SiteColumns
{
    /// <summary>
    /// The SiteColumn base class
    /// </summary>
    [Obsolete]
    public abstract class SiteColumnBase
    {
        /// <summary>
        /// Gets or sets the name of the internal.
        /// </summary>
        /// <value>
        /// The name of the internal.
        /// </value>
        public string InternalName { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is required]; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in display form].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in display form]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInDisplayForm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in edit form].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in edit form]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInEditForm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in list settings].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in list settings]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInListSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in new form].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in new form]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInNewForm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in version history].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show in version history]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInVersionHistory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in view form].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in view form]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInViewForm { get; set; }
    }
}
