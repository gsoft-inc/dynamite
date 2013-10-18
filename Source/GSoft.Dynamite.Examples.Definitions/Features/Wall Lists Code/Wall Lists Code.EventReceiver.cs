using System.Runtime.InteropServices;
using GSoft.Dynamite.Examples.Core.Constants;
using GSoft.Dynamite.Examples.Core.Unity;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.Examples.Definitions.Features.Wall_Lists_Code
{
    /// <summary>
    /// Handles the Event Receivers for the Wall_Lists_Code Feature.
    /// </summary>
    [Guid("8d51d5b5-a7e1-4eac-97d5-48ce1f7af5ad")]
    public class Wall_Lists_CodeEventReceiver : SPFeatureReceiver
    {
        private FieldHelper _fieldHelper;
        private ListHelper _listHelper;
        private ContentTypeHelper _contentTypeHelper;

        /// <summary>
        /// Features the activated.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            this.ResolveDependencies();
            SPWeb web = properties.Feature.Parent as SPWeb;

            if (web != null)
            {
                this._fieldHelper.SetLookupToList(web, WallFields.PostLookup.ID, ListUrls.WallPosts);

                this.SetupList(web, ListUrls.WallPosts, ContentTypes.WallPostsContentTypeId);
                this.SetupList(web, ListUrls.WallReplies, ContentTypes.WallReplyContentTypeId);
            }
        }

        /// <summary>
        /// Features the deactivating.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            this.ResolveDependencies();
            SPWeb web = properties.Feature.Parent as SPWeb;

            if (web != null)
            {
                this.DeleteContentTypeFromList(web, ListUrls.WallPosts, ContentTypes.WallPostsContentTypeId);
                this.DeleteContentTypeFromList(web, ListUrls.WallReplies, ContentTypes.WallReplyContentTypeId);
            }
        }

        private void ResolveDependencies()
        {
            this._fieldHelper = AppContainer.Current.Resolve<FieldHelper>();
            this._listHelper = AppContainer.Current.Resolve<ListHelper>();
            this._contentTypeHelper = AppContainer.Current.Resolve<ContentTypeHelper>();
        }

        private void SetupList(SPWeb web, string listUrl, SPContentTypeId contentTypeId)
        {
            SPList list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, listUrl));
            this._listHelper.AddContentType(list, contentTypeId);
            this._contentTypeHelper.DeleteContentTypeIfNotUsed(list.ContentTypes, SPBuiltInContentTypeId.Item);
        }

        private void DeleteContentTypeFromList(SPWeb web, string listUrl, SPContentTypeId contentTypeId)
        {
            SPList list = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, listUrl));
            this._contentTypeHelper.DeleteContentTypeIfNotUsed(list.ContentTypes, contentTypeId);
        }
    }
}
