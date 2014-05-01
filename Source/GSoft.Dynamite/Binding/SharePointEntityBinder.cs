using System;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Binding.Converters;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding
{
    using System.Data;

    using GSoft.Dynamite.Logging;

    /// <summary>
    /// The default entity binder for SharePoint.
    /// </summary>
    [CLSCompliant(false)]
    public class SharePointEntityBinder : ISharePointEntityBinder
    {
        #region Fields

        private readonly IEntitySchemaBuilder entitySchemaDataRowBuilder;

        private readonly IEntitySchemaBuilder entityListItemSchemaBuilder;

        private readonly TaxonomyValueDataRowConverter taxonomyValueDataRowConverter;
        private readonly TaxonomyValueCollectionDataRowConverter taxonomyValueCollectionDataRowConverter;

        private readonly TaxonomyValueConverter taxonomyValueConverter;
        private readonly TaxonomyValueCollectionConverter taxonomyValueCollectionConverter;


        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointEntityBinder"/> class.
        /// </summary>
        /// <param name="entitySchemaBuilder">The entity schema builder.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The types must be registred in the constructor.")]
        public SharePointEntityBinder(ILogger logger, IEntitySchemaBuilder entitySchemaDataRowBuilder, TaxonomyValueDataRowConverter taxonomyValueDataRowConverter, TaxonomyValueCollectionDataRowConverter taxonomyValueCollectionDataRowConverter, TaxonomyValueConverter taxonomyValueConverter, TaxonomyValueCollectionConverter taxonomyValueCollectionConverter)
        {
            // Create a new instance of the schema builder to patch the binder.
            var schemaBuilder = new EntitySchemaBuilder<SharePointEntitySchema>();
             var cachedBuilder = new CachedSchemaBuilder(schemaBuilder, logger);

             this.entitySchemaDataRowBuilder = entitySchemaDataRowBuilder;
            this.entityListItemSchemaBuilder = cachedBuilder;

            this.taxonomyValueConverter = taxonomyValueConverter;
            this.taxonomyValueCollectionConverter = taxonomyValueCollectionConverter;

            this.taxonomyValueDataRowConverter = taxonomyValueDataRowConverter;
            this.taxonomyValueCollectionDataRowConverter = taxonomyValueCollectionDataRowConverter;

            this.RegisterTypeConverters();
        }

        #endregion

        #region ISharePointEntityBinder Members

        /// <summary>
        /// Extracts the values from the entity to fill the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItem">The list item.</param>
        public void FromEntity<T>(T entity, SPListItem listItem)
        {
            this.entityListItemSchemaBuilder.GetSchema(typeof(T)).FromEntity(entity, new ListItemValuesAdapter(listItem), listItem.Fields, listItem.Web);
        }

        /// <summary>
        /// Creates an entity of the specified type and fills it using the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="listItem">The list item.</param>
        /// <returns>
        /// The newly created and filled entity.
        /// </returns>
        public T Get<T>(SPListItem listItem) where T : new()
        {
            var entity = new T();

            this.ToEntity(entity, listItem);

            return entity;
        }

        /// <summary>
        /// Creates an entity of the specified type and fills it using the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="listItemVersion">The list item version.</param>
        /// <returns>
        /// The newly created and filled entity.
        /// </returns>
        public T Get<T>(SPListItemVersion listItemVersion) where T : new()
        {
            var entity = new T();

            this.ToEntity(entity, listItemVersion);

            return entity;
        }

        /// <summary>
        /// Creates an entity of the specified type and fills it using the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="listItemVersion">The list item version.</param>
        /// <returns>
        /// The newly created and filled entity.
        /// </returns>
        public T Get<T>(DataRow dataRow, SPFieldCollection fieldCollection, SPWeb web) where T : new()
        {
            var entity = new T();

            this.ToEntity(entity, dataRow, fieldCollection, web);

            return entity;
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItem">The list item.</param>
        public void ToEntity<T>(T entity, SPListItem listItem)
        {
            this.entityListItemSchemaBuilder.GetSchema(typeof(T)).ToEntity(entity, new ListItemValuesAdapter(listItem), listItem.Fields, listItem.Web);
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the entity.
        /// </typeparam>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="dataRow">
        /// The data Row.
        /// </param>
        /// <param name="fieldCollection">
        /// The field Collection.
        /// </param>
        /// <param name="web">
        /// The web.
        /// </param>
        public void ToEntity<T>(T entity, DataRow dataRow, SPFieldCollection fieldCollection, SPWeb web)
        {
            this.entitySchemaDataRowBuilder.GetSchema(typeof(T)).ToEntity(entity, new DataRowValuesAdapter(dataRow), fieldCollection, web);
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItemVersion">The list item version.</param>
        public void ToEntity<T>(T entity, SPListItemVersion listItemVersion)
        {
            this.entityListItemSchemaBuilder.GetSchema(typeof(T)).ToEntity(entity, new ListItemVersionValuesAdapter(listItemVersion), listItemVersion.Fields, listItemVersion.ListItem.Web);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the type converters.
        /// </summary>
        protected internal virtual void RegisterTypeConverters()
        {
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(LookupValue), new LookupValueConverter());
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(PrincipalValue), new PrincipalValueConverter());
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(UserValue), new UserValueDataRowConverter());
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(UrlValue), new UrlValueConverter());
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(TaxonomyValue), this.taxonomyValueDataRowConverter);
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(TaxonomyValueCollection), this.taxonomyValueCollectionDataRowConverter);
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(ImageValue), new ImageValueConverter());

            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(LookupValue), new LookupValueConverter());
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(PrincipalValue), new PrincipalValueConverter());
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(UserValue), new UserValueConverter());
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(UrlValue), new UrlValueConverter());
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(TaxonomyValue), this.taxonomyValueConverter);
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(TaxonomyValueCollection), this.taxonomyValueCollectionConverter);
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(ImageValue), new ImageValueConverter());
        }

        #endregion
    }
}