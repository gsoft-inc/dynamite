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
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Files;
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
using GSoft.Dynamite.ServiceLocator.Lifetime;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.TimerJobs;
using GSoft.Dynamite.Utils;
using GSoft.Dynamite.ValueTypes.Readers;
using GSoft.Dynamite.ValueTypes.Writers;
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
            //var entitySchemaBuilder = new EntitySchemaBuilder<SharePointDataRowEntitySchema>();
            //var cachedSchemaBuilder = new CachedSchemaBuilder(entitySchemaBuilder, logger);

            //builder.RegisterType<SharePointDataRowEntitySchema>();
            //builder.RegisterInstance<IEntitySchemaBuilder>(cachedSchemaBuilder);
            //builder.RegisterType<TaxonomyValueDataRowConverter>();
            //builder.RegisterType<TaxonomyValueCollectionDataRowConverter>();
            //builder.RegisterType<TaxonomyValueConverter>();
            //builder.RegisterType<TaxonomyValueCollectionConverter>();
            //builder.RegisterType<SharePointEntityBinder>().As<ISharePointEntityBinder>().InstancePerSite();  // Singleton-per-site entity binder
            builder.RegisterType<EntitySchemaFactory>().As<IEntitySchemaFactory>();
            builder.RegisterType<SharePointEntityBinderNextGen>().As<ISharePointEntityBinder>().InstancePerSite();  // Singleton-per-site entity binder

            builder.RegisterType<FieldValueWriter>().As<IFieldValueWriter>();
            builder.RegisterType<StringValueWriter>();
            builder.RegisterType<BooleanValueWriter>();
            builder.RegisterType<IntegerValueWriter>();
            builder.RegisterType<DoubleValueWriter>();
            builder.RegisterType<DateTimeValueWriter>();
            builder.RegisterType<GuidValueWriter>();
            builder.RegisterType<TaxonomyFullValueWriter>();
            builder.RegisterType<TaxonomyFullValueCollectionWriter>();
            builder.RegisterType<LookupValueWriter>();
            builder.RegisterType<LookupValueCollectionWriter>();
            builder.RegisterType<PrincipalValueWriter>();
            builder.RegisterType<UserValueWriter>();
            builder.RegisterType<UserValueCollectionWriter>();
            builder.RegisterType<UrlValueWriter>();
            builder.RegisterType<ImageValueWriter>();
            builder.RegisterType<MediaValueWriter>();

            builder.RegisterType<FieldValueReader>().As<IFieldValueReader>();
            builder.RegisterType<StringValueReader>();
            builder.RegisterType<BooleanValueReader>();
            builder.RegisterType<IntegerValueReader>();
            builder.RegisterType<DoubleValueReader>();
            builder.RegisterType<DateTimeValueReader>();
            builder.RegisterType<GuidValueReader>();
            //builder.RegisterType<TaxonomyFullValueReader>();
            //builder.RegisterType<TaxonomyFullValueCollectionReader>();
            builder.RegisterType<LookupValueReader>();
            builder.RegisterType<LookupValueCollectionReader>();
            builder.RegisterType<PrincipalValueReader>();
            builder.RegisterType<UserValueReader>();
            builder.RegisterType<UserValueCollectionReader>();
            builder.RegisterType<UrlValueReader>();
            builder.RegisterType<ImageValueReader>();
            builder.RegisterType<MediaValueReader>();

            // Branding
            builder.RegisterType<MasterPageHelper>().As<IMasterPageHelper>();
            builder.RegisterType<ExtraMasterPageBodyCssClasses>().As<IExtraMasterPageBodyCssClasses>();
            builder.RegisterType<ComposedLookRepository>().As<IComposedLookRepository>();
            builder.RegisterType<DisplayTemplateHelper>().As<IDisplayTemplateHelper>();
            builder.RegisterType<ImageRenditionHelper>().As<IImageRenditionHelper>();

            // Cache
            builder.RegisterType<CacheHelper>().As<ICacheHelper>();

            // CAML query builder and utilities
            builder.RegisterType<CamlBuilder>().As<ICamlBuilder>();
            builder.RegisterType<CamlUtils>().As<ICamlUtils>();
            builder.RegisterType<QueryHelper>().As<IQueryHelper>();

            // Catalogs
            builder.RegisterType<CatalogHelper>().As<ICatalogHelper>();

            // Configuration
            builder.RegisterType<PropertyBagHelper>().As<IPropertyBagHelper>();
            builder.RegisterType<PropertyBagConfiguration>().As<IConfiguration>();
            builder.RegisterType<WebConfigModificationHelper>().As<IWebConfigModificationHelper>();

            // ContentTypes
            builder.RegisterType<ContentTypeHelper>().As<IContentTypeHelper>();

            // Documents
            builder.RegisterType<ContentOrganizerHelper>().As<IContentOrganizerHelper>();

            // Events
            builder.RegisterType<EventReceiverHelper>().As<IEventReceiverHelper>();

            // Fields
            builder.RegisterType<FieldHelper>().As<IFieldHelper>();
            builder.RegisterType<FieldLocator>().As<IFieldLocator>();
            builder.RegisterType<FieldSchemaHelper>().As<IFieldSchemaHelper>();
            builder.RegisterType<FieldLookupHelper>().As<IFieldLookupHelper>();

            // Folders
            builder.RegisterType<FolderHelper>().As<IFolderHelper>();
            builder.RegisterType<FolderRepository>().As<IFolderRepository>();

            // Files
            builder.RegisterType<FileHelper>().As<IFileHelper>();

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
            builder.RegisterType<PublishedLinksEditor>().As<IPublishedLinksEditor>();

            // Monitoring
            builder.RegisterType<AggregateTimeTracker>().As<IAggregateTimeTracker>().InstancePerSite();

            // Navigation
            builder.RegisterType<NavigationService>().As<INavigationService>();
            builder.RegisterType<NavigationHelper>().As<INavigationHelper>();
            builder.RegisterType<VariationNavigationHelper>().As<IVariationNavigationHelper>();

            // Pages
            builder.RegisterType<PageHelper>().As<IPageHelper>();

            // Repositories
            builder.RegisterType<ItemLocator>().As<IItemLocator>();

            // Search
            builder.RegisterType<SearchHelper>().As<ISearchHelper>();

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
            builder.RegisterType<CustomActionHelper>().As<ICustomActionHelper>();
            builder.RegisterType<CatchallExceptionHandler>().As<ICatchallExceptionHandler>();

            // Web Parts
            builder.RegisterType<WebPartHelper>().As<IWebPartHelper>();
        }
    }
}