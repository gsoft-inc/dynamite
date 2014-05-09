using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Examples.Constants;
using GSoft.Dynamite.Examples.Entities;
using JohnHolliday.Caml.Net;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Examples.Repositories
{
    /// <summary>
    /// Data access for wall replies
    /// </summary>
    public class WallReplyRepository : IWallReplyRepository
    {
        private readonly ISharePointEntityBinder _binder;

        /// <summary>
        /// Constructor to inject repository dependencies
        /// </summary>
        /// <param name="binder">The entity binder for mappings between list items and entities</param>
        public WallReplyRepository(ISharePointEntityBinder binder)
        {
            this._binder = binder;
        }

        /// <summary>
        /// Retrieves all wall replies by their wall post id
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="parentPostId">Id of parent wall post</param>
        /// <returns>The replies of the post</returns>
        public IEnumerable<WallReply> GetWallRepliesByPostId(SPWeb web, int parentPostId)
        {
            // Use SPWebContext so that the repo can be used by code called from outside a web request context (e.g. Powershell or OWSTimer)
            var list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, ListUrls.WallReplies));

            var query = new SPQuery();

            // CAML.NET doesn't support the LookupId attrbute for FieldRefs
            query.Query = CAML.Where(CAML.Eq(CAML.FieldRef(WallFields.PostLookupName).Replace("/>", " LookupId=\"TRUE\"/>"), CAML.Value("Integer", parentPostId.ToString(CultureInfo.InvariantCulture))));
            
            var items = list.GetItems(query);

            return items.Cast<SPListItem>().Select(x => this._binder.Get<WallReply>(x)).ToList();            
        }

        /// <summary>
        /// Creates a new wall reply
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="newEntity">The new wall reply entity</param>
        public void Create(SPWeb web, WallReply newEntity)
        {
            // Use SPWebContext so that the repo can be used by code called from outside a web request context (e.g. Powershell or OWSTimer)
            var list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, ListUrls.WallReplies));

            var newListItem = list.AddItem();
            this._binder.FromEntity(newEntity, newListItem);

            newListItem.Update();
        }
    }
}
