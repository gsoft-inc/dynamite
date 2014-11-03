using System;

namespace GSoft.Dynamite.Sites
{
    /// <summary>
    /// Site Collection definition
    /// </summary>
    public class SiteInfo
    {
        /// <summary>
        /// Initializes a new site metadata object
        /// </summary>
        /// <param name="url">The site's Uri-parse-able URL</param>
        /// <param name="name">The site's name</param>
        public SiteInfo(string url, string name)
        {
           this.Url = new Uri(url);
           this.Name = name;
        }

        /// <summary>
        /// Site collection Url
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Name of the site collection
        /// </summary>
        public string Name { get; set; }
    }
}
