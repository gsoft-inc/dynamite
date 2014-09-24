using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.ServiceLocator;
using Autofac;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Base definition for a SharePoint structural entity (list, field, content type, web and site)
    /// </summary>
    public abstract class BaseTypeInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public BaseTypeInfo()
        {       
        }

        /// <summary>
        /// Creates a new base information objects with keys to resources
        /// </summary>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Description resource key</param>
        public BaseTypeInfo(string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
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

        private static string FindResourceValueForKey(string resourceKey)
        {
            string displayName = string.Empty;

            using(var injectionScope = InternalServiceLocator.BeginLifetimeScope())
            {
                var resourceLocator = injectionScope.Resolve<IResourceLocator>();
                displayName = resourceLocator.Find(resourceKey);
            }

            // if resource value wasn't found, return the key at least
            return string.IsNullOrEmpty(displayName) ? resourceKey : displayName;
        }
    }
}
