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

        /// <summary>
        /// Creates a new simple cache key to cache the same items regardless of the current language
        /// </summary>
        /// <param name="keyForBothLanguages">The key to share between both English and French content items</param>
        public SimpleCacheKey(string keyForBothLanguages)
            : this(keyForBothLanguages, keyForBothLanguages)
        {
        }

        /// <summary>
        /// Creates a new simple cache key to cache different items depending on the current language
        /// </summary>
        /// <param name="englishKey">English key</param>
        /// <param name="frenchKey">French key</param>
        public SimpleCacheKey(string englishKey, string frenchKey)
        {
            var groupDiscrimitator = BuildSecurityGroupDiscriminatorPrefix();
            this.InEnglish = SimpleCacheKey.Prefix + groupDiscrimitator + englishKey;
            this.InFrench = SimpleCacheKey.Prefix + groupDiscrimitator + frenchKey;
        }

        /// <summary>
        /// Get english key
        /// </summary>
        public string InEnglish { get; private set; }

        /// <summary>
        /// Get french key
        /// </summary>
        public string InFrench { get; private set; }

        private static string BuildSecurityGroupDiscriminatorPrefix()
        {
            string discriminator = string.Empty;

            if (SPContext.Current != null
                && SPContext.Current.Web != null
                && SPContext.Current.Web.CurrentUser != null)
            {
                // We have an authenticated user
                if (SPContext.Current.Web.CurrentUser.Groups != null
                    && SPContext.Current.Web.CurrentUser.Groups.Count > 0)
                {
                    // Add the ordered list of all group names as second prefix to the cache key
                    var groupNamesArray = SPContext.Current.Web.CurrentUser.Groups
                        .Cast<SPGroup>()
                        .OrderBy(group => group.Name)
                        .Select(group => group.Name)
                        .ToArray();
                    discriminator = string.Join("_", groupNamesArray);
                }
                else
                {
                    discriminator = "NoGroup";
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
