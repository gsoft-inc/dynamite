using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Binding.Converters;
using GSoft.Dynamite.Cache;
using GSoft.Dynamite.Caching;
using GSoft.Dynamite.Catalogs;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.MasterPages;
using GSoft.Dynamite.Navigation;
using GSoft.Dynamite.Repositories;
using GSoft.Dynamite.Security;
using GSoft.Dynamite.Setup;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.TimerJobs;
using GSoft.Dynamite.Utils;
using GSoft.Dynamite.WebConfig;
using GSoft.Dynamite.WebParts;
using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.DI.Unity
{
    /// <summary>
    /// Container registrations for GSoft.G.SharePoint components
    /// </summary>
    public class UnityDynamiteUnityIRegistrationModule : IUnityRegistrationModule
    {
        private readonly string logCategoryName;
        private readonly string[] defaultResourceFileNames;

        /// <summary>
        /// Creates a new registration module to prepare dependency injection
        /// for GSoft.Dynamite components
        /// </summary>
        /// <param name="logCategoryName">The ULS category in use when interacting with ILogger</param>
        /// <param name="defaultResourceFileName">The default resource file name when interacting with IResourceLocator</param>
        public UnityDynamiteUnityIRegistrationModule(string logCategoryName, string defaultResourceFileName)
        {
            this.logCategoryName = logCategoryName;
            this.defaultResourceFileNames = new string[] { defaultResourceFileName };
        }

        /// <summary>
        /// Creates a new registration module to prepare dependency injection
        /// for GSoft.Dynamite components
        /// </summary>
        /// <param name="logCategoryName">The ULS category in use when interacting with ILogger</param>
        /// <param name="defaultResourceFileNames">The default resource file names when interacting with IResourceLocator</param>
        public UnityDynamiteUnityIRegistrationModule(string logCategoryName, string[] defaultResourceFileNames)
        {
            this.logCategoryName = logCategoryName;
            this.defaultResourceFileNames = defaultResourceFileNames;
        }

        /// <summary>
        /// Registers the modules type bindings
        /// </summary>
        /// <param name="container">The container on which to register type bindings</param>
        public void Register(IUnityContainer container)
        {
            // Logging
#if DEBUG
            var logger = new TraceLogger(this.logCategoryName, this.logCategoryName, true);     // Logger with debug output
            container.RegisterInstance<ILogger>(logger);
#else
            var logger = new TraceLogger(this.logCategoryName, this.logCategoryName, false);    // Logger without debug output
            container.RegisterInstance<ILogger>(logger);
#endif

            // Binding
            var builder = new EntitySchemaBuilder<SharePointEntitySchema>();
            var cachedBuilder = new CachedSchemaBuilder(builder, logger);
            container.RegisterInstance<IEntitySchemaBuilder>(cachedBuilder);
            container.RegisterType<TaxonomyValueConverter>();
            container.RegisterType<TaxonomyValueCollectionConverter>();
            container.RegisterType<ISharePointEntityBinder, SharePointEntityBinder>(new ContainerControlledLifetimeManager());

            // Cache
            container.RegisterType<ICacheHelper, CacheHelper>();

            // Definitions
            container.RegisterType<ContentTypeBuilder>();
            container.RegisterType<FieldHelper>();

            // Globalization + Variations (with default en-CA as source + fr-CA as destination implementation)
            container.RegisterInstance<IResourceLocator>(new ResourceLocator(this.defaultResourceFileNames));
            container.RegisterType<MuiHelper>();
            container.RegisterType<DateHelper>();
            container.RegisterType<RegionalSettingsHelper>();       

            container.RegisterType<IVariationDirector, DefaultVariationDirector>();
            container.RegisterType<IVariationBuilder, CanadianEnglishAndFrenchVariationBuilder>();
            container.RegisterType<IVariationExpert, VariationExpert>();

            // TODO: Consolidate with VariationExpert
            container.RegisterType<VariationsHelper>();

            // Lists
            container.RegisterType<ListHelper>();
            container.RegisterType<ListSecurityHelper>();

            // Catalogs
            container.RegisterType<CatalogBuilder>();

            // Master Pages
            container.RegisterType<MasterPageHelper>();
            container.RegisterType<IExtraMasterPageBodyCssClasses, ExtraMasterPageBodyCssClasses>();

            // Repositories
            container.RegisterType<FolderRepository>();
            container.RegisterType<ListLocator>();
            container.RegisterType<IQueryHelper, QueryHelper>();

            // Security
            container.RegisterType<SecurityHelper>();
            container.RegisterType<UserHelper>();

            // Setup
            container.RegisterType<IFieldValueInfo, FieldValueInfo>();
            container.RegisterType<IFolderInfo, FolderInfo>();
            container.RegisterType<IPageInfo, PageInfo>();
            container.RegisterType<ITaxonomyInfo, TaxonomyInfo>();
            container.RegisterType<ITaxonomyMultiInfo, TaxonomyMultiInfo>();

            container.RegisterType<IFolderMaker, FolderMaker>();
            container.RegisterType<PageCreator>();

            // Taxonomy
            container.RegisterType<ISiteTaxonomyCacheManager, SiteTaxonomyCacheManager>();
            container.RegisterType<ITaxonomyService, TaxonomyService>();
            container.RegisterType<TaxonomyService>();
            container.RegisterType<TaxonomyHelper>();

            // Timer Jobs
            container.RegisterType<ITimerJobExpert, TimerJobExpert>();

            // Utilities
            container.RegisterType<EventReceiverHelper>();
            container.RegisterType<SearchHelper>();
            container.RegisterType<CustomActionHelper>();
            container.RegisterType<ContentOrganizerHelper>();

            // Web config
            container.RegisterType<WebConfigModificationHelper>();

            // Web Parts
            container.RegisterType<WebPartHelper>();

            // Navigation
            container.RegisterType<ICatalogNavigation, CatalogNavigation>();

            // TODO: Caching - Obsolete helpers
            container.RegisterType<IAppCacheHelper, AppCacheHelper>();
            container.RegisterType<ISessionCacheHelper, SessionCacheHelper>();
        }
    }
}
