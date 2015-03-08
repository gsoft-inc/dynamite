using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Best best definition (search promoted results)
    /// </summary>
    public class BestBetInfo
    {
        /// <summary>
        /// Creates a new <see cref="BestBetInfo"/>
        /// </summary>
        /// <param name="title">The title</param>
        /// <param name="url">The URL</param>
        /// <param name="description">A description</param>
        public BestBetInfo(string title, Uri url, string description)
        {
            this.Title = title;
            this.Url = url;
            this.Description = description;
            this.IsVisualBestBet = false;
            this.DeleteIfUnused = false;
        }

        /// <summary>
        /// Best bet title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Best bet URL
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Best bet description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether the promoted result should be shown as images.
        /// False by default.
        /// </summary>
        public bool IsVisualBestBet { get; set; }

        /// <summary>
        /// Whether, if unused, it should get automatically deleted.
        /// False by default.
        /// </summary>
        public bool DeleteIfUnused { get; set; }
    }
}
