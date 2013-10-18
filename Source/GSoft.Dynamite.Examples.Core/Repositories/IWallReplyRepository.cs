using System.Collections.Generic;
using GSoft.Dynamite.Examples.Core.Entities;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Examples.Core.Repositories
{
    /// <summary>
    /// Interface for wall replies repository
    /// </summary>
    public interface IWallReplyRepository
    {
        /// <summary>
        /// Retrieves all wall replies by their wall post id
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="parentPostId">Id of parent wall post</param>
        /// <returns>The replies of the post</returns>
        IEnumerable<WallReply> GetWallRepliesByPostId(SPWeb web, int parentPostId);

        /// <summary>
        /// Creates a new wall reply
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="newEntity">The new wall reply entity</param>
        void Create(SPWeb web, WallReply newEntity);
    }
}
