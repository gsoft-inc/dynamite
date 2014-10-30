using System;

namespace GSoft.Dynamite.Sites
{
    /// <summary>
    /// Site Collection defintion
    /// </summary>
    public class SiteInfo
    {
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
        /// Name of the site collecion
        /// </summary>
        public string Name { get; set; }
    }
}
