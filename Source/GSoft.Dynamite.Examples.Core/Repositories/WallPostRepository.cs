using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Examples.Constants;
using GSoft.Dynamite.Examples.Entities;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Examples.Repositories
{
    /// <summary>
    /// Data access for wall posts
    /// </summary>
    public class WallPostRepository : IWallPostRepository
    {
        private readonly ISharePointEntityBinder _binder;
        private IWallReplyRepository _wallRepliesRepository;
        private ILogger _log;

        /// <summary>
        /// Constructor to inject repository dependencies
        /// </summary>
        /// <param name="binder">The entity binder for mappings between list items and entities</param>
        /// <param name="wallRepliesRepository">Wall replies repository</param>
        /// <param name="log">Logging utility</param>
        public WallPostRepository(ISharePointEntityBinder binder, IWallReplyRepository wallRepliesRepository, ILogger log)
        {
            this._binder = binder;
            this._wallRepliesRepository = wallRepliesRepository;
            this._log = log;
        }

        /// <summary>
        /// Retrieves all wall posts from SharePoint list
        /// </summary>
        /// <param name="web">The current web</param>
        /// <returns>All wall post entities</returns>
        public IEnumerable<WallPost> AllWallPosts(SPWeb web)
        {
            // Use SPWebContext so that the repo can be used by code called from outside a web request context (e.g. Powershell or OWSTimer)
            var list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, ListUrls.WallPosts));

            var items = list.Items;

            var mappedPosts = items.Cast<SPListItem>().Select(x => this._binder.Get<WallPost>(x)).ToList();

            // Join with wall replies to populate each post's replies collection
            mappedPosts.ForEach(post => 
                {
                    post.Replies = this._wallRepliesRepository.GetWallRepliesByPostId(web, post.Id).ToList();
                    this._log.Info(string.Format(CultureInfo.InvariantCulture, "Found {0} replies for post {1}", post.Replies.Count(), post.Id));
                });

            return mappedPosts;
        }

        /// <summary>
        /// Creates a new wall post
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="newEntity">The new wall post entity</param>
        public void Create(SPWeb web, WallPost newEntity)
        {
            // Use SPWebContext so that the repo can be used by code called from outside a web request context (e.g. Powershell or OWSTimer)
            var list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, ListUrls.WallPosts));

            var newListItem = list.AddItem();
            this._binder.FromEntity(newEntity, newListItem);

            newListItem.Update();
        }
    }
}
