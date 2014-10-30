using Autofac;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Binding.Converters;
using GSoft.Dynamite.Branding;
using GSoft.Dynamite.Cache;
using GSoft.Dynamite.Caml;
using GSoft.Dynamite.Catalogs;
using GSoft.Dynamite.Configuration;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Documents;
using GSoft.Dynamite.Events;
using GSoft.Dynamite.Exceptions;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Folders;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Helpers;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Monitoring;
using GSoft.Dynamite.Navigation;
using GSoft.Dynamite.Pages;
using GSoft.Dynamite.Repositories;
using GSoft.Dynamite.Search;
using GSoft.Dynamite.Security;
using GSoft.Dynamite.Serializers;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.TimerJobs;
using GSoft.Dynamite.Utils;
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
            // Monitoring
            builder.RegisterType<AggregateTimeTracker>().As<IAggregateTimeTracker>().InstancePerSite();

            // Binding
            var entitySchemaBuilder = new EntitySchemaBuilder<SharePointDataRowEntitySchema>();
            var cachedSchemaBuilder = new CachedSchemaBuilder(entitySchemaBuilder, logger);

            builder.RegisterType<SharePointDataRowEntitySchema>();
            builder.RegisterInstance<IEntitySchemaBuilder>(cachedSchemaBuilder);
            builder.RegisterType<TaxonomyValueDataRowConverter>();
            builder.RegisterType<TaxonomyValueCollectionDataRowConverter>();
            builder.RegisterType<TaxonomyValueConverter>();
            builder.RegisterType<TaxonomyValueCollectionConverter>();
            builder.RegisterType<SharePointEntityBinder>().As<ISharePointEntityBinder>().InstancePerSite();  // Singleton-per-site entity binder

            // Cache
            builder.RegisterType<CacheHelper>().As<ICacheHelper>();

            // Configuration 
            builder.RegisterType<PropertyBagHelper>().As<IPropertyBagHelper>();
            builder.RegisterType<PropertyBagConfiguration>().As<IConfiguration>();

            // Definitions
            builder.RegisterType<ContentTypeHelper>().As<IContentTypeHelper>();
            builder.RegisterType<FieldHelper>().As<IFieldHelper>();
            builder.RegisterType<FolderHelper>().As<IFolderHelper>();
            builder.RegisterType<PageHelper>().As<IPageHelper>();

            // Exception
            builder.RegisterType<CatchAllExceptionHandler>().As<ICatchAllExceptionHandler>();

            // Globalization + Variations (with default en-CA as source + fr-CA as destination implementation)
            builder.RegisterType<ResourceLocator>().As<IResourceLocator>();     

            // It's the container user's responsibility to register a IResourceLocatorConfig implementation 
            builder.RegisterType<DefaultResourceLocatorConfig>().As<IResourceLocatorConfig>();
            builder.RegisterType<MuiHelper>().As<IMuiHelper>();
            builder.RegisterType<DateHelper>().As<IDateHelper>();
            builder.RegisterType<RegionalSettingsHelper>().As<IRegionalSettingsHelper>();

            builder.RegisterType<VariationExpert>().As<IVariationExpert>();
            builder.RegisterType<VariationHelper>().As<IVariationHelper>();
            builder.RegisterType<VariationSyncHelper>().As<IVariationSyncHelper>();

            // Lists
            builder.RegisterType<ListHelper>().As<IListHelper>();
            builder.RegisterType<ListLocator>().As<IListLocator>();
            builder.RegisterType<ListSecurityHelper>().As<IListSecurityHelper>();
            builder.RegisterType<CatalogHelper>().As<ICatalogHelper>();

            // MasterPages
            builder.RegisterType<MasterPageHelper>().As<IMasterPageHelper>();
            builder.RegisterType<ExtraMasterPageBodyCssClasses>().As<IExtraMasterPageBodyCssClasses>();

            // Navigation 
            builder.RegisterType<NavigationService>().As<INavigationService>();
            builder.RegisterType<NavigationNode>().As<INavigationNode>();
            builder.RegisterType<NavigationManagedProperties>();

            // Repositories
            builder.RegisterType<FolderRepository>().As<IFolderRepository>();
            builder.RegisterType<QueryHelper>().As<IQueryHelper>();
            builder.RegisterType<ItemLocator>().As<IItemLocator>();

            // Security
            builder.RegisterType<SecurityHelper>().As<ISecurityHelper>();
            builder.RegisterType<UserHelper>().As<IUserHelper>();

            // Serializers
            builder.RegisterType<XmlHelper>().As<IXmlHelper>();
            builder.RegisterType<JsonNetSerializer>().As<ISerializer>().SingleInstance();

            // Taxonomy
            builder.RegisterType<PerRequestSiteTaxonomyCacheManager>().As<ISiteTaxonomyCacheManager>();
            builder.RegisterType<TaxonomyService>().As<ITaxonomyService>();

            //// Example of monitored (profiled) instance:
            ////builder.RegisterType<TaxonomyService>().Named<ITaxonomyService>("decorated").InstancePerSite();
            ////builder.RegisterDecorator<ITaxonomyService>((c, inner) => new MonitoredTaxonomyService(inner, c.Resolve<IAggregateTimeTracker>()), fromKey: "decorated");

            builder.RegisterType<TaxonomyHelper>().As<ITaxonomyHelper>();

            // Timer Jobs
            builder.RegisterType<TimerJobHelper>().As<ITimerJobHelper>();

            // Utils
            builder.RegisterType<EventReceiverHelper>().As<IEventReceiverHelper>();
            builder.RegisterType<SearchHelper>().As<ISearchHelper>();
            builder.RegisterType<CustomActionHelper>().As<ICustomActionHelper>();
            builder.RegisterType<ContentOrganizerHelper>().As<IContentOrganizerHelper>();
            builder.RegisterType<NavigationHelper>().As<INavigationHelper>();
            builder.RegisterType<CatalogNavigation>().As<ICatalogNavigation>();

            // Branding
            builder.RegisterType<ComposedLookRepository>().As<IComposedLookRepository>();
            builder.RegisterType<DisplayTemplateHelper>().As<IDisplayTemplateHelper>();
            builder.RegisterType<ImageRenditionHelper>().As<IImageRenditionHelper>();

            // CAML query builder and utilities
            builder.RegisterType<CamlBuilder>().As<ICamlBuilder>();
            builder.RegisterType<CamlUtils>().As<ICamlUtils>();

            // Web config
            builder.RegisterType<WebConfigModificationHelper>().As<IWebConfigModificationHelper>();

            // Web Parts
            builder.RegisterType<WebPartHelper>().As<IWebPartHelper>();
        }
    }
}
