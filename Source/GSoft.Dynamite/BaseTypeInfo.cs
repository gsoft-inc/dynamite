using Autofac;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.ServiceLocator;

namespace GSoft.Dynamite
{
    /// <summary>
    /// Base definition for a SharePoint structural entity (list, field, content type, web and site)
    /// </summary>
    public abstract class BaseTypeInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        protected BaseTypeInfo()
        {
        }

        /// <summary>
        /// Creates a new base information objects with keys to resources
        /// </summary>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        protected BaseTypeInfo(string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : this()
        {
            this.DisplayNameResourceKey = displayNameResourceKey;
            this.DescriptionResourceKey = descriptionResourceKey;
            this.GroupResourceKey = groupResourceKey;
        }

        /// <summary>
        /// Creates a new base information objects with keys to resources
        /// </summary>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        /// <param name="resourceFileName">Name of the resource file.</param>
        protected BaseTypeInfo(string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey, string resourceFileName)
            : this(displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            this.ResourceFileName = resourceFileName;
        }

        /// <summary>
        /// The display name resource key
        /// </summary>
        public string DisplayNameResourceKey { get; set; }

        /// <summary>
        /// The description resource key
        /// </summary>
        public string DescriptionResourceKey { get; set; }

        /// <summary>
        /// The content group resource key
        /// </summary>
        public string GroupResourceKey { get; set; }

        /// <summary>
        /// The name of the resource file.
        /// </summary>
        public string ResourceFileName { get; set; }
    }
}
