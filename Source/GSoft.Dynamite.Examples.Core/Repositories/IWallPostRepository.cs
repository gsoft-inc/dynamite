using System.Collections.Generic;
using GSoft.Dynamite.Examples.Entities;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Examples.Repositories
{
    /// <summary>
    /// Interface for wall posts repository
    /// </summary>
    public interface IWallPostRepository
    {
        /// <summary>
        /// Retrieves all wall posts from SharePoint list
        /// </summary>
        /// <param name="web">The current web</param>
        /// <returns>All wall posts entities</returns>
        IEnumerable<WallPost> AllWallPosts(SPWeb web);

        /// <summary>
        /// Creates a new wall post
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="newEntity">The new wall post entity</param>
        void Create(SPWeb web, WallPost newEntity);
    }
}
