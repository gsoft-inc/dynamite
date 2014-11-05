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
        /// The display name resource key
        /// </summary>
        public string DisplayNameResourceKey { get; set; }

        /// <summary>
        /// The display name
        /// </summary>
        public string DisplayName
        {
            get
            {
                return FindResourceValueForKey(this.DisplayNameResourceKey);
            }
        }

        /// <summary>
        /// The resource string for the display name
        /// </summary>
        public string DisplayNameResourceString
        {
            get
            {
                return FindResourceStringForKey(this.DisplayNameResourceKey); 
            }
        }

        /// <summary>
        /// The description resource key
        /// </summary>
        public string DescriptionResourceKey { get; set; }

        /// <summary>
        /// The description
        /// </summary>
        public string Description
        {
            get
            {
                return FindResourceValueForKey(this.DescriptionResourceKey);
            }
        }

        /// <summary>
        /// The description resource string
        /// </summary>
        public string DescriptionResourceString
        {
            get
            {
                return FindResourceStringForKey(this.DescriptionResourceKey); 
            }
        }

        /// <summary>
        /// The content group resource key
        /// </summary>
        public string GroupResourceKey { get; set; }

        /// <summary>
        /// The content group
        /// </summary>
        public string Group
        {
            get
            {
                return FindResourceValueForKey(this.GroupResourceKey);
            }
        }

        /// <summary>
        /// The content group resource string
        /// </summary>
        public string GroupResourceString
        {
            get
            {
                return FindResourceStringForKey(this.GroupResourceKey);
            }
        }

        private static string FindResourceValueForKey(string resourceKey)
        {
            string displayName = string.Empty;

            using (var injectionScope = InternalServiceLocator.BeginLifetimeScope())
            {
                var resourceLocator = injectionScope.Resolve<IResourceLocator>();
                displayName = resourceLocator.Find(resourceKey);
            }

            // if resource value wasn't found, return the key at least
            return string.IsNullOrEmpty(displayName) ? resourceKey : displayName;
        }

        private static string FindResourceStringForKey(string resourceKey)
        {
            string displayName = string.Empty;

            using (var injectionScope = InternalServiceLocator.BeginLifetimeScope())
            {
                var resourceLocator = injectionScope.Resolve<IResourceLocator>();
                displayName = resourceLocator.GetResourceString(resourceKey);
            }

            // if resource value wasn't found, return the key at least
            return string.IsNullOrEmpty(displayName) ? resourceKey : displayName;
        }
    }
}
