using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.UserProfile
{
    /// <summary>
    /// User profile helper interface,
    /// </summary>
    public interface IUserProfileHelper
    {
        /// <summary>
        /// Gets the user profile configuration manager.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The user profile configuration manager.</returns>
        UserProfileConfigManager GetUserProfileConfigManager(SPSite site);

        /// <summary>
        /// Gets the core property manager.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The core property manager.</returns>
        CorePropertyManager GetCorePropertyManager(SPSite site);

        /// <summary>
        /// Gets the profile type property manager.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The profile type property manager.</returns>
        ProfileTypePropertyManager GetProfileTypePropertyManager(SPSite site);

        /// <summary>
        /// Gets the profile subtype property manager.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The profile subtype property manager.</returns>
        ProfileSubtypePropertyManager GetProfileSubtypePropertyManager(SPSite site);

        /// <summary>
        /// Ensures the profile property.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="userProfilePropertyInfo">The user profile property information.</param>
        /// <returns>The user profile core property.</returns>
        CoreProperty EnsureProfileProperty(SPSite site, UserProfilePropertyInfo userProfilePropertyInfo);

        /// <summary>
        /// Removes the profile property.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="userProfilePropertyInfo">The user profile property information.</param>
        /// <returns>A boolean value if the property was removed or not.</returns>
        bool RemoveProfileProperty(SPSite site, UserProfilePropertyInfo userProfilePropertyInfo);
    }
}