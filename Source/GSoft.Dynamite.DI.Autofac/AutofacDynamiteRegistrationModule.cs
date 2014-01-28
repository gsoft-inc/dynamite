// -----------------------------------------------------------------------
// <copyright file="AutofacDynamiteRegistrationModule.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.DI.Autofac
{
    using global::Autofac;

    using GSoft.Dynamite.Binding;
    using GSoft.Dynamite.Logging;
    using GSoft.Dynamite.Repositories;
    using GSoft.Dynamite.Taxonomy;
    using GSoft.Dynamite.TimerJobs;
    using GSoft.Dynamite.Utils;

    /// <summary>
    /// Container registrations for GSoft.G.SharePoint components
    /// </summary>
    public class AutofacDynamiteRegistrationModule : Module
    {
        /// <summary>
        /// The application name
        /// </summary>
        private const string AppName = "IFC.IntactNet";

        private readonly string logCategoryName;
        private readonly string[] defaultResourceFileNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacDynamiteRegistrationModule"/> class.
        /// </summary>
        public AutofacDynamiteRegistrationModule()
        {
            this.logCategoryName = AppName;
            this.defaultResourceFileNames = new string[] { AppName, AppName + ".News", AppName + ".ConfigurationValues", AppName + ".ReusableContent", AppName + ".Navigation", AppName + ".ProvinceToBU" };
        }

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
            var binder = new SharePointEntityBinder(new CachedSchemaBuilder(entitySchemaBuilder, logger));
            builder.RegisterInstance<ISharePointEntityBinder>(binder);

            // Taxonomy
            builder.RegisterType<TaxonomyService>().As<ITaxonomyService>();

            builder.RegisterType<TaxonomyService>();
            builder.RegisterType<TaxonomyHelper>();

            // Repositories
            builder.RegisterType<FolderRepository>();
            builder.RegisterType<ListLocator>();

            // Utilities
            builder.RegisterInstance<IResourceLocator>(new ResourceLocator(this.defaultResourceFileNames));

            builder.RegisterType<ContentTypeHelper>();
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

            // Experts
            builder.RegisterType<TimerJobExpert>().As<ITimerJobExpert>();
        }
    }
}
