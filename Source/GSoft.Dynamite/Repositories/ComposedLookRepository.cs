using System.Web;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Repositories.Entities;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Repositories
{
    /// <summary>
    /// Composed look repository
    /// </summary>
    public class ComposedLookRepository : IComposedLookRepository
    {
        private readonly ISharePointEntityBinder _binder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComposedLookRepository"/> class.
        /// </summary>
        /// <param name="binder">The binder.</param>
        public ComposedLookRepository(ISharePointEntityBinder binder)
        {
            this._binder = binder;
        }

        /// <summary>
        /// Gets the composed look by id.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="id">The id.</param>
        /// <returns>The composed look.</returns>
        public ComposedLook GetById(SPWeb web, int id)
        {
            var designCatalog = web.GetCatalog(SPListTemplateType.DesignCatalog);
            var composedLook = this._binder.Get<ComposedLook>(designCatalog.GetItemById(id));
            return DecodeUrls(composedLook);
        }

        /// <summary>
        /// Retrieves a composed look by name.
        /// </summary>
        /// <param name="web">The current web.</param>
        /// <param name="name">The name of the composed look.</param>
        /// <returns>
        /// Returns a composed look by name.
        /// </returns>
        public ComposedLook GetByName(SPWeb web, string name)
        {
            var designCatalog = web.GetCatalog(SPListTemplateType.DesignCatalog);
            var items = designCatalog.GetItems(new SPQuery()
            {
                RowLimit = 1u,
                ViewFields = @"
                    <FieldRef Name=""Name"" />
                    <FieldRef Name=" + BuiltInFields.MasterPageUrlName + @" />
                    <FieldRef Name=" + BuiltInFields.ThemeUrlName + @" />
                    <FieldRef Name=" + BuiltInFields.ImageUrlName + @" />
                    <FieldRef Name=" + BuiltInFields.FontSchemeUrlName + @" />",
                Query = @"
                    <Where>
                        <Eq>
                            <FieldRef Name=""Name"" />
                            <Value Type=""Text"">" + name + @"</Value>
                        </Eq>
                    </Where>"
            });

            return items.Count != 1 ? null : this._binder.Get<ComposedLook>(items[0]);
        }

        /// <summary>
        /// Creates the specified composed look.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// The newly created composed look.
        /// </returns>
        public ComposedLook Create(SPWeb web, ComposedLook entity)
        {
            var designCatalog = web.GetCatalog(SPListTemplateType.DesignCatalog);
            var item = designCatalog.Items.Add();
            this._binder.FromEntity(entity, item);
            item.Update();

            return this._binder.Get<ComposedLook>(item);
        }

        /// <summary>
        /// Updates the specified composed look.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="entity">The entity.</param>
        public void Update(SPWeb web, ComposedLook entity)
        {
            var designCatalog = web.GetCatalog(SPListTemplateType.DesignCatalog);
            var item = designCatalog.Items.GetItemById(entity.Id);
            this._binder.FromEntity(entity, item);
            item.Update();
        }

        /// <summary>
        /// Deletes the specified composed look.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="entity">The entity.</param>
        public void Delete(SPWeb web, ComposedLook entity)
        {
            var designCatalog = web.GetCatalog(SPListTemplateType.DesignCatalog);
            var item = designCatalog.GetItemById(entity.Id);
            item.Delete();
        }

        private static ComposedLook DecodeUrls(ComposedLook composedLook)
        {
            if (composedLook.ImagePath != null)
            {
                composedLook.ImagePath.Url = HttpUtility.UrlDecode(composedLook.ImagePath.Url); 
            }

            if (composedLook.MasterPagePath != null)
            {
                composedLook.MasterPagePath.Url = HttpUtility.UrlDecode(composedLook.MasterPagePath.Url); 
            }

            if (composedLook.ThemePath != null)
            {
                composedLook.ThemePath.Url = HttpUtility.UrlDecode(composedLook.ThemePath.Url); 
            }

            return composedLook;
        }
    }
}
