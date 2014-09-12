using Autofac;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Binding.Converters;
using GSoft.Dynamite.Branding;
using GSoft.Dynamite.Cache;
using GSoft.Dynamite.Caching;
using GSoft.Dynamite.Catalogs;
using GSoft.Dynamite.Configuration;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Exceptions;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Helpers;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.MasterPages;
using GSoft.Dynamite.Navigation;
using GSoft.Dynamite.Repositories;
using GSoft.Dynamite.Security;
using GSoft.Dynamite.Serializers;
using GSoft.Dynamite.Setup;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.TimerJobs;
using GSoft.Dynamite.Utils;
using GSoft.Dynamite.WebConfig;
using GSoft.Dynamite.WebParts;
using Microsoft.Office.Server.Search;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Container registrations for GSoft.Dynamite core components
    /// </summary>
    public class AutofacDynamiteRegistrationModule : Module
    {
        private readonly string logCategoryName;

        /// <summary>
        /// Creates a new registration module to prepare dependency injection
        /// for GSoft.Dynamite components
        /// </summary>
        /// <param name="logCategoryName">The ULS category in use when interacting with ILogger</param>
        public AutofacDynamiteRegistrationModule(string logCategoryName)
        {
            this.logCategoryName = logCategoryName;
        }

        /// <summary>
        /// Registers the modules type bindings
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        protected override void Load(ContainerBuilder builder)
        {
            // Logging
#if DEBUG
            var logger = new TraceLogger(this.logCategoryName, this.logCategoryName, true);     // Logger with debug output
            builder.RegisterInstance<ILogger>(logger);
#else
            var logger = new TraceLogger(this.logCategoryName, this.logCategoryName, false);    // Logger without debug output
            builder.RegisterInstance<ILogger>(logger);
#endif

            // Binding
            var entitySchemaBuilder = new EntitySchemaBuilder<SharePointDataRowEntitySchema>();
            var cachedBuilder = new CachedSchemaBuilder(entitySchemaBuilder, logger);

            builder.RegisterType<SharePointDataRowEntitySchema>();
            builder.RegisterInstance<IEntitySchemaBuilder>(cachedBuilder);
            builder.RegisterType<TaxonomyValueDataRowConverter>();
            builder.RegisterType<TaxonomyValueCollectionDataRowConverter>();
            builder.RegisterType<TaxonomyValueConverter>();
            builder.RegisterType<TaxonomyValueCollectionConverter>();
            builder.RegisterType<SharePointEntityBinder>().As<ISharePointEntityBinder>().InstancePerSite();  // Singleton-per-site entity binder

            // Cache
            builder.RegisterType<CacheHelper>().As<ICacheHelper>();
            builder.RegisterType<AppCacheHelper>().As<IAppCacheHelper>();
            builder.RegisterType<SessionCacheHelper>().As<ISessionCacheHelper>();

            // Configuration 
            builder.RegisterType<PropertyBagHelper>();
            builder.RegisterType<PropertyBagConfiguration>().As<IConfiguration>();

            // Definitions
            builder.RegisterType<ContentTypeHelper>();
            builder.RegisterType<FieldHelper>();

            // Exception
            builder.RegisterType<CatchAllExceptionHandler>().As<ICatchAllExceptionHandler>();

            // Globalization + Variations (with default en-CA as source + fr-CA as destination implementation)
            builder.RegisterType<ResourceLocator>().As<IResourceLocator>();     // It's the container user's responsibility to register a IResourceLocatorConfig implementation 
            builder.RegisterType<DefaultResourceLocatorConfig>().As<IResourceLocatorConfig>();
            builder.RegisterType<MuiHelper>();
            builder.RegisterType<DateHelper>();
            builder.RegisterType<RegionalSettingsHelper>();

            builder.RegisterType<DefaultVariationDirector>().As<IVariationDirector>();
            builder.RegisterType<CanadianEnglishAndFrenchVariationBuilder>().As<IVariationBuilder>();
            builder.RegisterType<VariationExpert>().As<IVariationExpert>();
            builder.RegisterType<VariationHelper>();

            // Lists
            builder.RegisterType<ListHelper>();
            builder.RegisterType<ListLocator>();
            builder.RegisterType<ListLocator>().As<IListLocator>();
            builder.RegisterType<ListSecurityHelper>();
            builder.RegisterType<CatalogHelper>();

            // MasterPages
            builder.RegisterType<MasterPageHelper>();
            builder.RegisterType<ExtraMasterPageBodyCssClasses>().As<IExtraMasterPageBodyCssClasses>();

            // Navigation 
            builder.RegisterType<NavigationService>();
            builder.RegisterType<NavigationNode>().As<INavigationNode>();
            builder.RegisterType<NavigationManagedProperties>();

            // Repositories
            builder.RegisterType<FolderRepository>();
            builder.RegisterType<QueryHelper>().As<IQueryHelper>();

            // Security
            builder.RegisterType<SecurityHelper>();
            builder.RegisterType<UserHelper>();

            // Serializers
            builder.RegisterType<XmlHelper>();
            builder.RegisterType<JsonNetSerializer>().As<ISerializer>().SingleInstance();

            // Setup
            builder.RegisterType<FieldValueInfo>().As<IFieldValueInfo>();
            builder.RegisterType<FolderInfo>().As<IFolderInfo>();
            builder.RegisterType<PageInfo>().As<IPageInfo>();
            builder.RegisterType<TaxonomyInfo>().As<ITaxonomyInfo>();
            builder.RegisterType<TaxonomyMultiInfo>().As<ITaxonomyMultiInfo>();

            builder.RegisterType<FolderMaker>().As<IFolderMaker>();
            builder.RegisterType<PageCreator>();

            // Taxonomy
            builder.RegisterType<SiteTaxonomyCacheManager>().As<ISiteTaxonomyCacheManager>().SingleInstance();
            builder.RegisterType<TaxonomyService>().As<ITaxonomyService>().InstancePerSite();
            builder.RegisterType<TaxonomyHelper>();

            // Timer Jobs
            builder.RegisterType<TimerJobExpert>().As<ITimerJobExpert>();

            // Utils
            builder.RegisterType<EventReceiverHelper>();
            builder.RegisterType<SearchHelper>();
            builder.RegisterType<CustomActionHelper>();
            builder.RegisterType<ContentOrganizerHelper>();
            builder.RegisterType<NavigationHelper>();
            builder.RegisterType<CatalogNavigation>().As<ICatalogNavigation>();

            // Branding
            builder.RegisterType<ComposedLookRepository>().As<IComposedLookRepository>();
            builder.RegisterType<DisplayTemplateHelper>();
            builder.RegisterType<ImageRenditionHelper>();

            // Web config
            builder.RegisterType<WebConfigModificationHelper>();

            // Web Parts
            builder.RegisterType<WebPartHelper>();
        }
    }
}
