using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Cache
{
    /// <summary>
    /// Simple cache key
    /// </summary>
    public class SimpleCacheKey : ICacheKey
    {
        /// <summary>
        /// The prefix of the keep to identify the Dynamite cache keys in the HttpCache.
        /// </summary>
        public const string Prefix = "Dynamite-";

        private string englishKey = string.Empty;
        private string frenchKey = string.Empty;
        private bool discriminateBetweenGroups = true;

        // keep one cached discriminator per user, per site (not threadsafe, but no worries)
        private static IDictionary<string, string> userSiteGroupDiscriminators = new Dictionary<string, string>();

        /// <summary>
        /// Creates a new simple cache key to cache the same items regardless of the current language. 
        /// Will discriminate against the current users SharePoint Groups by adding them to the key as a prefix.
        /// </summary>
        /// <param name="keyForBothLanguages">The key to share between both English and French content items</param>
        public SimpleCacheKey(string keyForBothLanguages)
            : this(keyForBothLanguages, true)
        {
        }

        /// <summary>
        /// Creates a new simple cache key to cache the same items regardless of the current language
        /// </summary>
        /// <param name="keyForBothLanguages">The key to share between both English and French content items</param>
        /// <param name="discriminateBetweenGroups">if set to <c>true</c> add a prefix in the key with the groups the current user belongs to.</param>
        public SimpleCacheKey(string keyForBothLanguages, bool discriminateBetweenGroups)
            : this(keyForBothLanguages, keyForBothLanguages, discriminateBetweenGroups)
        {
        }

        /// <summary>
        /// Creates a new simple cache key to cache different items depending on the current language.
        /// Will discriminate against the current users SharePoint Groups by adding them to the key as a prefix.
        /// </summary>
        /// <param name="englishKey">English key</param>
        /// <param name="frenchKey">French key</param>
        public SimpleCacheKey(string englishKey, string frenchKey)
            : this(englishKey, englishKey, true)
        {
        }

        /// <summary>
        /// Creates a new simple cache key to cache different items depending on the current language
        /// </summary>
        /// <param name="englishKey">English key</param>
        /// <param name="frenchKey">French key</param>
        /// <param name="discriminateBetweenGroups">if set to <c>true</c> add a prefix in the key with the groups the current user belongs to.</param>
        public SimpleCacheKey(string englishKey, string frenchKey, bool discriminateBetweenGroups)
        {
            this.englishKey = englishKey;
            this.frenchKey = frenchKey;
            this.discriminateBetweenGroups = discriminateBetweenGroups;
        }

        /// <summary>
        /// Get english key (only members of the same set of groups will share a key)
        /// </summary>
        public string InEnglish 
        {
            get
            {
                var groupDiscrimitator = string.Empty;
                if (this.discriminateBetweenGroups)
                {
                    groupDiscrimitator = BuildSecurityGroupDiscriminatorPrefix();
                }

                return SimpleCacheKey.Prefix + groupDiscrimitator + this.englishKey;
            }
        }

        /// <summary>
        /// Get french key (only members of the same set of groups will share a key)
        /// </summary>
        public string InFrench 
        {
            get
            {
                var groupDiscrimitator = string.Empty;
                if (this.discriminateBetweenGroups)
                {
                    groupDiscrimitator = BuildSecurityGroupDiscriminatorPrefix();
                }

                return SimpleCacheKey.Prefix + groupDiscrimitator + this.frenchKey;
            }
        }

        private static string BuildSecurityGroupDiscriminatorPrefix()
        {
            string discriminator = string.Empty;

            if (SPContext.Current != null
                && SPContext.Current.Web != null
                && SPContext.Current.Web.CurrentUser != null)
            {
                // We have an authenticated user
                var currentWeb = SPContext.Current.Web;
                var currentUser = currentWeb.CurrentUser;

                var discriminatorCacheKey = currentWeb.Site.Url + "_" + currentUser.LoginName;

                if (userSiteGroupDiscriminators.ContainsKey(discriminatorCacheKey))
                {
                    discriminator = userSiteGroupDiscriminators[discriminatorCacheKey];
                }
                else
                {
                    // no cached discriminator for that site+user combination yet
                    var currentUserGroups = new List<SPGroup>();

                    foreach (SPGroup siteGroup in currentWeb.SiteGroups)
                    {
                        if (siteGroup.ContainsCurrentUser)
                        {
                            currentUserGroups.Add(siteGroup);
                        }
                    }

                    if (currentUserGroups.Count > 0)
                    {
                        // Add the ordered list of all group names as second prefix to the cache key
                        var groupNamesArray = currentUserGroups.OrderBy(group => group.Name)
                            .Select(group => group.Name).ToArray();
                        discriminator = string.Join("_", groupNamesArray);
                    }
                    else
                    {
                        discriminator = "NoGroup";
                    }

                    // cache the discriminator value for later (don't worry too much about multithreaded
                    // access - at worse we evaluate the discriminator a couple of times in a row)
                    userSiteGroupDiscriminators[discriminatorCacheKey] = discriminator;
                }
            }
            else
            {
                // We have an anonymous user
                discriminator = "Anonymous";
            }

            return discriminator + "-";
        }
    }
}
