using System.Collections.Generic;
using System.Runtime.InteropServices;
using GSoft.Dynamite.Examples.Constants;
using GSoft.Dynamite.Examples.Unity;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;
using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.Examples.Definitions.Features.Wall_Content_Types
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("30ad8328-ba57-4811-ac84-2fa764f9b5d2")]
    public class Wall_Content_TypesEventReceiver : SPFeatureReceiver
    {
        /// <summary>
        /// The wall post fields
        /// </summary>
        public readonly ICollection<FieldInfo> WallPostFields = new List<FieldInfo>() 
        { 
            WallFields.TextContent,
            WallFields.Tags,
            WallFields.TagsTaxHT,
            WallFields.Author,
            WallFields.TaxCatchAll,
            WallFields.TaxCatchAllLabel
        };

        /// <summary>
        /// The wall reply fields
        /// </summary>
        public readonly ICollection<FieldInfo> WallReplyFields = new List<FieldInfo>()
        {
            WallFields.TextContent,
            WallFields.Tags,
            WallFields.TagsTaxHT,
            WallFields.Author,
            WallFields.PostLookup,
            WallFields.TaxCatchAll,
            WallFields.TaxCatchAllLabel
        };

        private ContentTypeHelper _contentTypeHelper;
        private TaxonomyHelper _taxonomyHelper;

        /// <summary>
        /// Features the activated.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            this.ResolveDependencies();
            SPSite site = properties.Feature.Parent as SPSite;

            if (site != null)
            {
                // Create Wall Post Content Type
                this.CreateWallPostContentType(site.RootWeb.ContentTypes);

                // Create Wall Reply Content Type
                this.CreateWallReplyContentType(site.RootWeb.ContentTypes);
            }
        }

        /// <summary>
        /// Features the deactivating.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            this.ResolveDependencies();
            SPSite site = properties.Feature.Parent as SPSite;

            if (site != null)
            {
                // Delete the content type if its not used.
                this._contentTypeHelper.DeleteContentTypeIfNotUsed(site.RootWeb.ContentTypes, ContentTypes.WallPostsContentTypeId);
                this._contentTypeHelper.DeleteContentTypeIfNotUsed(site.RootWeb.ContentTypes, ContentTypes.WallReplyContentTypeId);
            }
        }

        private void ResolveDependencies()
        {
            this._contentTypeHelper = AppContainer.Current.Resolve<ContentTypeHelper>();
            this._taxonomyHelper = AppContainer.Current.Resolve<TaxonomyHelper>();
        }

        /// <summary>
        /// Creates the wall reply content type.
        /// </summary>
        /// <param name="collection">The collection the content type will be added to.</param>
        private void CreateWallReplyContentType(SPContentTypeCollection collection)
        {
            var contentType = this._contentTypeHelper.EnsureContentType(
                collection, 
                ContentTypes.WallReplyContentTypeId, 
                "$Resources:GSoft.Dynamite.Examples.Global,ContentType_WallReply;");
            
            this._contentTypeHelper.EnsureFieldInContentType(contentType, this.WallReplyFields);
            
            contentType.Group = "$Resources:GSoft.Dynamite.Examples.Global,ContentGroup;";
            contentType.Description = "$Resources:GSoft.Dynamite.Examples.Global,ContentType_WallReplyDescription;";
            contentType.Update(true);

            this._taxonomyHelper.EnsureTaxonomyEventReceivers(contentType.EventReceivers);
        }

        /// <summary>
        /// Creates the wall post content type.
        /// </summary>
        /// <param name="collection">The collection the content type will be added to.</param>
        private void CreateWallPostContentType(SPContentTypeCollection collection)
        {
            SPContentType contentType = this._contentTypeHelper.EnsureContentType(
                collection, 
                ContentTypes.WallPostsContentTypeId, 
                "$Resources:GSoft.Dynamite.Examples.Global,ContentType_WallPost;");

            this._contentTypeHelper.EnsureFieldInContentType(contentType, this.WallPostFields);
            
            contentType.Group = "$Resources:GSoft.Dynamite.Examples.Global,ContentGroup;";
            contentType.Description = "$Resources:GSoft.Dynamite.Examples.Global,ContentType_WallPostDescription;";
            contentType.Update(true);

            this._taxonomyHelper.EnsureTaxonomyEventReceivers(contentType.EventReceivers);
        }
    }
}
