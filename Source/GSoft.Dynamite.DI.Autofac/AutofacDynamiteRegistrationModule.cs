using global::Autofac;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Binding.Converters;
using GSoft.Dynamite.Cache;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Repositories;
using GSoft.Dynamite.Setup;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.TimerJobs;
using GSoft.Dynamite.Utils;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Security;
using GSoft.Dynamite.WebParts;
using GSoft.Dynamite.MasterPages;
using GSoft.Dynamite.WebConfig;
using GSoft.Dynamite.Globalization.Variations;

namespace GSoft.Dynamite.DI.Autofac
{
    using GSoft.Dynamite.Serializers;

    /// <summary>
    /// Container registrations for GSoft.G.SharePoint components
    /// </summary>
    public class AutofacDynamiteRegistrationModule : Module
    {
        private readonly string logCategoryName;
        private readonly string[] defaultResourceFileNames;

        /// <summary>
        /// Creates a new registration module to prepare dependency injection
        /// for GSoft.Dynamite components
        /// </summary>
        /// <param name="logCategoryName">The ULS category in use when interacting with ILogger</param>
        /// <param name="defaultResourceFileName">The default resource file name when interacting with IResourceLocator</param>
        public AutofacDynamiteRegistrationModule(string logCategoryName, string defaultResourceFileName)
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
        public AutofacDynamiteRegistrationModule(string logCategoryName, string[] defaultResourceFileNames)
        {
            this.logCategoryName = logCategoryName;
            this.defaultResourceFileNames = defaultResourceFileNames;
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
            var entitySchemaBuilder = new EntitySchemaBuilder<SharePointEntitySchema>();
            var cachedBuilder = new CachedSchemaBuilder(entitySchemaBuilder, logger);
            builder.RegisterInstance<IEntitySchemaBuilder>(cachedBuilder);
            builder.RegisterType<TaxonomyValueConverter>();
            builder.RegisterType<TaxonomyValueCollectionConverter>();

            builder.RegisterType<SharePointEntityBinder>().As<ISharePointEntityBinder>().SingleInstance();  // Singleton entity binder

            // Cache
            builder.RegisterType<CacheHelper>().As<ICacheHelper>();

            // Definitions
            builder.RegisterType<ContentTypeBuilder>();
            builder.RegisterType<FieldHelper>();

            // Globalization + Variations (with default en-CA as source + fr-CA as destination implementation)
            builder.RegisterInstance<IResourceLocator>(new ResourceLocator(this.defaultResourceFileNames));
            builder.RegisterType<MuiHelper>();
            builder.RegisterType<DateHelper>();
            builder.RegisterType<RegionalSettingsHelper>();

            builder.RegisterType<DefaultVariationDirector>().As<IVariationDirector>();
            builder.RegisterType<CanadianEnglishAndFrenchVariationBuilder>().As<IVariationBuilder>();
            builder.RegisterType<VariationExpert>().As<IVariationExpert>();

            // Lists
            builder.RegisterType<ListHelper>();
            builder.RegisterType<ListLocator>();
            builder.RegisterType<ListSecurityHelper>();

            // MasterPages
            builder.RegisterType<MasterPageHelper>();
            builder.RegisterType<ExtraMasterPageBodyCssClasses>().As<IExtraMasterPageBodyCssClasses>();

            // Repositories
            builder.RegisterType<FolderRepository>();
            builder.RegisterType<QueryHelper>().As<IQueryHelper>();

            // Security
            builder.RegisterType<SecurityHelper>();
            builder.RegisterType<UserHelper>(); 

            // Serializers
            builder.RegisterType<Serializer>().As<ISerializer>().SingleInstance();

            // Setup
            builder.RegisterType<FieldValueInfo>().As<IFieldValueInfo>();
            builder.RegisterType<FolderInfo>().As<IFolderInfo>();
            builder.RegisterType<PageInfo>().As<IPageInfo>();
            builder.RegisterType<TaxonomyInfo>().As<ITaxonomyInfo>();
            builder.RegisterType<TaxonomyMultiInfo>().As<ITaxonomyMultiInfo>();

            builder.RegisterType<FolderMaker>().As<IFolderMaker>();
            builder.RegisterType<PageCreator>();

            // Taxonomy
            builder.RegisterType<TaxonomyService>().Named<ITaxonomyService>("implementor");
            builder.RegisterDecorator<ITaxonomyService>((c, inner) => new CachedTaxonomyService(inner), fromKey: "implementor");
            builder.RegisterType<TaxonomyService>();
            builder.RegisterType<TaxonomyHelper>();

            // Timer Jobs
            builder.RegisterType<TimerJobExpert>().As<ITimerJobExpert>();
            
            // Utils
            builder.RegisterType<EventReceiverHelper>();
            builder.RegisterType<SearchHelper>();
            builder.RegisterType<CustomActionHelper>();
            builder.RegisterType<ContentOrganizerHelper>();

            // Web config
            builder.RegisterType<WebConfigModificationHelper>();

            // Web Parts
            builder.RegisterType<WebPartHelper>();
        }
    }
}
