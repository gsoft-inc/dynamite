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
#if DEBUG
            // Logger with debug output
            var logger = new TraceLogger(this.logCategoryName, this.logCategoryName, true);
            builder.RegisterInstance<ILogger>(logger);
#else
            // Logger without debug output
            var logger = new TraceLogger(this.logCategoryName, this.logCategoryName, false);
            builder.RegisterInstance<ILogger>(logger);
#endif

            // Binding
            var entitySchemaBuilder = new EntitySchemaBuilder<SharePointEntitySchema>();
            var cachedBuilder = new CachedSchemaBuilder(entitySchemaBuilder, logger);
            builder.RegisterInstance<IEntitySchemaBuilder>(cachedBuilder);
            builder.RegisterType<TaxonomyValueConverter>();
            builder.RegisterType<TaxonomyValueCollectionConverter>();

            // Singleton entity binder
            builder.RegisterType<SharePointEntityBinder>().As<ISharePointEntityBinder>().SingleInstance();

            // Setup
            builder.RegisterType<FieldValueInfo>().As<IFieldValueInfo>();
            builder.RegisterType<FolderInfo>().As<IFolderInfo>();
            builder.RegisterType<PageInfo>().As<IPageInfo>();
            builder.RegisterType<TaxonomyInfo>().As<ITaxonomyInfo>();
            builder.RegisterType<TaxonomyMultiInfo>().As<ITaxonomyMultiInfo>();

            builder.RegisterType<FolderMaker>().As<IFolderMaker>();

            // Taxonomy
            builder.RegisterType<TaxonomyService>().As<ITaxonomyService>();
            builder.RegisterType<TaxonomyService>();
            builder.RegisterType<TaxonomyHelper>();

            // Repositories
            builder.RegisterType<FolderRepository>();
            builder.RegisterType<ListLocator>();
            builder.RegisterType<QueryHelper>().As<IQueryHelper>();

            // Cache
            builder.RegisterType<CacheHelper>().As<ICacheHelper>();

            // Utilities
            builder.RegisterInstance<IResourceLocator>(new ResourceLocator(this.defaultResourceFileNames));

            builder.RegisterType<ContentTypeBuilder>();
            builder.RegisterType<EventReceiverHelper>();
            builder.RegisterType<FieldHelper>();
            builder.RegisterType<ListHelper>();
            builder.RegisterType<ListSecurityHelper>();
            builder.RegisterType<MuiHelper>(); 
            builder.RegisterType<SecurityHelper>();
            builder.RegisterType<SearchHelper>();
            builder.RegisterType<WebPartHelper>();
            builder.RegisterType<MasterPageHelper>();
            builder.RegisterType<RegionalSettingsHelper>();
            builder.RegisterType<CustomActionHelper>();
            builder.RegisterType<WebConfigModificationHelper>();
            builder.RegisterType<ContentOrganizerHelper>();
            builder.RegisterType<DateHelper>();
            builder.RegisterType<UserHelper>(); 
            builder.RegisterType<ExtraMasterPageBodyCssClasses>().As<IExtraMasterPageBodyCssClasses>();

            // Variations (with default en-CA as source + fr-CA as destination implementation)
            builder.RegisterType<DefaultVariationDirector>().As<IVariationDirector>();
            builder.RegisterType<CanadianEnglishAndFrenchVariationBuilder>().As<IVariationBuilder>();
            builder.RegisterType<VariationExpert>().As<IVariationExpert>();

            // Experts
            builder.RegisterType<TimerJobExpert>().As<ITimerJobExpert>();
        }
    }
}
