using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Taxonomy;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.UserProfile
{
    /// <summary>
    /// User profile helper methods.
    /// </summary>
    public class UserProfileHelper : IUserProfileHelper
    {
        private readonly ILogger logger;
        private readonly ITaxonomyHelper taxonomyHelper;
        private readonly ISiteTaxonomyCacheManager siteTaxonomyCacheManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="taxonomyHelper">The taxonomy helper.</param>
        /// <param name="siteTaxonomyCacheManager">The site taxonomy cache manager.</param>
        public UserProfileHelper(
            ILogger logger, 
            ITaxonomyHelper taxonomyHelper, 
            ISiteTaxonomyCacheManager siteTaxonomyCacheManager)
        {
            this.logger = logger;
            this.taxonomyHelper = taxonomyHelper;
            this.siteTaxonomyCacheManager = siteTaxonomyCacheManager;
        }

        #region Interface implementation

        /// <summary>
        /// Gets the user profile configuration manager.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The user profile configuration manager.</returns>
        public UserProfileConfigManager GetUserProfileConfigManager(SPSite site)
        {
            var context = SPServiceContext.GetContext(site);
            return new UserProfileConfigManager(context);
        }

        /// <summary>
        /// Gets the core property manager.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The core property manager.</returns>
        public CorePropertyManager GetCorePropertyManager(SPSite site)
        {
            var userProfileConfigManager = this.GetUserProfileConfigManager(site);
            return userProfileConfigManager.ProfilePropertyManager.GetCoreProperties();
        }

        /// <summary>
        /// Gets the profile type property manager.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The profile type property manager.</returns>
        public ProfileTypePropertyManager GetProfileTypePropertyManager(SPSite site)
        {
            var userProfileConfigManager = this.GetUserProfileConfigManager(site);
            var propertyManager = userProfileConfigManager.ProfilePropertyManager;
            return propertyManager.GetProfileTypeProperties(ProfileType.User);
        }

        /// <summary>
        /// Gets the profile subtype property manager.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The profile subtype property manager.</returns>
        public ProfileSubtypePropertyManager GetProfileSubtypePropertyManager(SPSite site)
        {
            var context = SPServiceContext.GetContext(site);
            var profileSubTypeManager = ProfileSubtypeManager.Get(context);
            var profileSubtype = profileSubTypeManager.GetProfileSubtype(ProfileSubtypeManager.GetDefaultProfileName(ProfileType.User));
            return profileSubtype.Properties;
        }

        /// <summary>
        /// Ensures the profile property.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="userProfilePropertyInfo">The user profile property information.</param>
        /// <returns>The user profile core property.</returns>
        public CoreProperty EnsureProfileProperty(SPSite site, UserProfilePropertyInfo userProfilePropertyInfo)
        {
            // Ensure core property
            var property = this.EnsureCoreProperty(
                site,
                userProfilePropertyInfo,
                this.GetCorePropertyManager(site));

            // Ensure profile type property
            var profileTypeProperty = EnsureProfileTypeProperty(
                property,
                userProfilePropertyInfo,
                this.GetProfileTypePropertyManager(site));

            // Ensure profile subtype property
            EnsureProfileSubtypeProperty(
                profileTypeProperty,
                userProfilePropertyInfo,
                this.GetProfileSubtypePropertyManager(site));

            return property;
        }

        /// <summary>
        /// Removes the profile property.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="userProfilePropertyInfo">The user profile property information.</param>
        /// <returns>A boolean value if the property was removed or not.</returns>
        public bool RemoveProfileProperty(SPSite site, UserProfilePropertyInfo userProfilePropertyInfo)
        {
            var userProfileCoreProperties = this.GetCorePropertyManager(site);
            var property = userProfileCoreProperties.GetPropertyByName(userProfilePropertyInfo.Name);
            if (property != null)
            {
                userProfileCoreProperties.RemovePropertyByName(userProfilePropertyInfo.Name);
                return true;
            }

            return false;
        }

        #endregion

        private CoreProperty EnsureCoreProperty(
            SPSite site,
            UserProfilePropertyInfo userProfilePropertyInfo,
            CorePropertyManager userProfileCoreProperties)
        {
            // If property is null, create it. Else, update it
            var property = userProfileCoreProperties.GetPropertyByName(userProfilePropertyInfo.Name);
            if (property == null)
            {
                this.logger.Info("Creating user profile property '{0}'", userProfilePropertyInfo.Name);

                // Create property (false here means it's not a section)
                property = userProfileCoreProperties.Create(false);
                property.Name = userProfilePropertyInfo.Name;
                property.Type = userProfilePropertyInfo.PropertyDataType;
                property.Length = userProfilePropertyInfo.Length;
                property.IsMultivalued = userProfilePropertyInfo.IsMultivalued;
            }
            else
            {
                this.logger.Info("Updating user profile property '{0}'", userProfilePropertyInfo.Name);
            }

            property.DisplayName = userProfilePropertyInfo.DisplayName;
            property.Description = userProfilePropertyInfo.Description;
            property.IsAlias = userProfilePropertyInfo.IsAlias;
            property.IsSearchable = userProfilePropertyInfo.IsSearchable;

            // Setup localized display name
            if (userProfilePropertyInfo.DisplayNameLocalized.Count > 0)
            {
                foreach (var displayName in userProfilePropertyInfo.DisplayNameLocalized)
                {
                    property.DisplayNameLocalized[displayName.Key] = displayName.Value;
                }
            }

            // Setup localized description
            if (userProfilePropertyInfo.DescriptionLocalized.Count > 0)
            {
                foreach (var description in userProfilePropertyInfo.DescriptionLocalized)
                {
                    property.DescriptionLocalized[description.Key] = description.Value;
                }
            }

            // Setup taxonomy mappings if configured
            if (userProfilePropertyInfo.TermSetInfo != null)
            {
                var taxonomyCache = this.siteTaxonomyCacheManager.GetSiteTaxonomyCache(site, null, this.taxonomyHelper);
                var termStore = this.taxonomyHelper.GetDefaultSiteCollectionTermStore(taxonomyCache.TaxonomySession);
                property.TermSet = termStore.GetTermSet(userProfilePropertyInfo.TermSetInfo.Id);
            }

            property.Commit();
            return property;
        }

        private static ProfileTypeProperty EnsureProfileTypeProperty(
            CoreProperty property,
            UserProfilePropertyInfo userProfilePropertyInfo,
            ProfileTypePropertyManager profileTypePropertyManager)
        {
            var profileTypeProperty = profileTypePropertyManager.GetPropertyByName(userProfilePropertyInfo.Name);
            if (profileTypeProperty == null)
            {
                profileTypeProperty = profileTypePropertyManager.Create(property);
                profileTypeProperty.IsVisibleOnViewer = userProfilePropertyInfo.IsVisibleOnViewer;
                profileTypeProperty.IsVisibleOnEditor = userProfilePropertyInfo.IsVisibleOnEditor;
                profileTypeProperty.IsReplicable = userProfilePropertyInfo.IsReplicable;
                profileTypePropertyManager.Add(profileTypeProperty);
            }
            else
            {
                profileTypeProperty.IsVisibleOnViewer = userProfilePropertyInfo.IsVisibleOnViewer;
                profileTypeProperty.IsVisibleOnEditor = userProfilePropertyInfo.IsVisibleOnEditor;
                profileTypeProperty.IsReplicable = userProfilePropertyInfo.IsReplicable;
            }

            return profileTypeProperty;
        }

        private static ProfileSubtypeProperty EnsureProfileSubtypeProperty(
            ProfileTypeProperty profileTypeProperty,
            UserProfilePropertyInfo userProfilePropertyInfo,
            ProfileSubtypePropertyManager profileSubtypePropertyManager)
        {
            var profileSubtypeProperty = profileSubtypePropertyManager.GetPropertyByName(userProfilePropertyInfo.Name);
            if (profileSubtypeProperty == null)
            {
                profileSubtypeProperty = profileSubtypePropertyManager.Create(profileTypeProperty);
                profileSubtypeProperty.IsUserEditable = userProfilePropertyInfo.IsUserEditable;
                profileSubtypePropertyManager.Add(profileSubtypeProperty);
            }
            else
            {
                profileSubtypeProperty.IsUserEditable = userProfilePropertyInfo.IsUserEditable;
            }

            return profileSubtypeProperty;
        }
    }
}
