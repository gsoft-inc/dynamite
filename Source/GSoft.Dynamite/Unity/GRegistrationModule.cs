using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Repositories;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;
using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.Unity
{
    /// <summary>
    /// Container registrations for GSoft.G.SharePoint components
    /// </summary>
    public class GRegistrationModule : IRegistrationModule
    {
        private readonly string _logCategoryName;
        private readonly string _defaultResourceFileName;

        /// <summary>
        /// Creates a new registration module to prepare dependency injection
        /// for GSoft.G.SharePoint components
        /// </summary>
        /// <param name="logCategoryName">The ULS category in use when interacting with ILogger</param>
        /// <param name="defaultResourceFileName">The default resource file name when interacting with IResourceLocator</param>
        public GRegistrationModule(string logCategoryName, string defaultResourceFileName)
        {
            this._logCategoryName = logCategoryName;
            this._defaultResourceFileName = defaultResourceFileName;
        }

        /// <summary>
        /// Registers the modules type bindings
        /// </summary>
        /// <param name="container">The container on which to register type bindings</param>
        public void Register(IUnityContainer container)
        {
            // Binding
            var builder = new EntitySchemaBuilder<SharePointEntitySchema>();
            var binder = new SharePointEntityBinder(new CachedSchemaBuilder(builder));
            container.RegisterInstance<ISharePointEntityBinder>(binder);

            // Taxonomy
            container.RegisterType<ITaxonomyService, TaxonomyService>();
            container.RegisterType<TaxonomyService>();
            container.RegisterType<TaxonomyHelper>();

            // Repositories
            container.RegisterType<FolderRepository>();
            container.RegisterType<ListLocator>();

            // Utilities
            container.RegisterInstance<IResourceLocator>(new ResourceLocator(this._defaultResourceFileName));
#if DEBUG
            // Logger with debug output
            container.RegisterInstance<ILogger>(new TraceLogger(this._logCategoryName, this._logCategoryName, true));
#else
            // Logger without debug output
            container.RegisterInstance<ILogger>(new TraceLogger(this._logCategoryName, this._logCategoryName, false));
#endif
            container.RegisterType<ContentTypeHelper>();
            container.RegisterType<EventReceiverHelper>();
            container.RegisterType<FieldHelper>();
            container.RegisterType<ListHelper>();
            container.RegisterType<ListSecurityHelper>();
            container.RegisterType<MuiHelper>();
            container.RegisterType<SecurityHelper>();
            container.RegisterType<SearchHelper>();
            container.RegisterType<WebPartHelper>();
            container.RegisterType<MasterPageHelper>();
            container.RegisterType<RegionalSettingsHelper>();
            container.RegisterType<CustomActionHelper>();
            container.RegisterType<WebConfigModificationHelper>();
        }
    }
}
