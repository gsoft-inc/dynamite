namespace GSoft.Dynamite.DI.Unity
{
    using GSoft.Dynamite.Binding;
    using GSoft.Dynamite.Logging;
    using GSoft.Dynamite.Repositories;
    using GSoft.Dynamite.Taxonomy;
    using GSoft.Dynamite.TimerJobs;
    using GSoft.Dynamite.Utils;

    using Microsoft.Practices.Unity;

    /// <summary>
    /// Container registrations for GSoft.G.SharePoint components
    /// </summary>
    public class UnityDynamiteUnityIRegistrationModule : UnityIRegistrationModule
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
#if DEBUG
            // Logger with debug output
            var logger = new TraceLogger(this.logCategoryName, this.logCategoryName, true);
            container.RegisterInstance<ILogger>(logger);
#else
            // Logger without debug output
            var logger = new TraceLogger(this.logCategoryName, this.logCategoryName, false);
            container.RegisterInstance<ILogger>(logger);
#endif

            // Binding
            var builder = new EntitySchemaBuilder<SharePointEntitySchema>();
            var binder = new SharePointEntityBinder(new CachedSchemaBuilder(builder, logger));
            container.RegisterInstance<ISharePointEntityBinder>(binder);

            // Taxonomy
            container.RegisterType<ITaxonomyService, TaxonomyService>();
            container.RegisterType<TaxonomyService>();
            container.RegisterType<TaxonomyHelper>();

            // Repositories
            container.RegisterType<FolderRepository>();
            container.RegisterType<ListLocator>();

            // Utilities
            container.RegisterInstance<IResourceLocator>(new ResourceLocator(this.defaultResourceFileNames));

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
            container.RegisterType<ContentOrganizerHelper>();
            container.RegisterType<DateHelper>();
            container.RegisterType<UserHelper>();

            // Experts
            container.RegisterType<ITimerJobExpert, TimerJobExpert>();
        }
    }
}
