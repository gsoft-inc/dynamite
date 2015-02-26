using System;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Binding.Converters;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding
{
    using System.Collections.Generic;
    using System.Data;

    using GSoft.Dynamite.Logging;

    /// <summary>
    /// The default entity binder for SharePoint.
    /// </summary>
    public class SharePointEntityBinder : ISharePointEntityBinder
    {
        #region Fields

        private readonly ILogger logger;

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
        /// <param name="logger">The logger</param>
        /// <param name="entitySchemaDataRowBuilder">Entity schema data builder</param>
        /// <param name="taxonomyValueDataRowConverter">Data row converter</param>
        /// <param name="taxonomyValueCollectionDataRowConverter">Taxonomy collection data row converter</param>
        /// <param name="taxonomyValueConverter">The taxonomy value converter</param>
        /// <param name="taxonomyValueCollectionConverter">The Taxonomy value collection converter</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The types must be registred in the constructor.")]
        public SharePointEntityBinder(ILogger logger, IEntitySchemaBuilder entitySchemaDataRowBuilder, TaxonomyValueDataRowConverter taxonomyValueDataRowConverter, TaxonomyValueCollectionDataRowConverter taxonomyValueCollectionDataRowConverter, TaxonomyValueConverter taxonomyValueConverter, TaxonomyValueCollectionConverter taxonomyValueCollectionConverter)
        {
            // Create a new instance of the schema builder to patch the binder.
            var schemaBuilder = new EntitySchemaBuilder<SharePointEntitySchema>();
             var cachedBuilder = new CachedSchemaBuilder(schemaBuilder, logger);

            this.logger = logger;
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
            this.entityListItemSchemaBuilder.GetSchema(typeof(T)).FromEntity(entity, new ListItemValuesAdapter(listItem), listItem.Fields);
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
        /// The get.
        /// </summary>
        /// <param name="listItems">
        /// The list items.
        /// </param>
        /// <typeparam name="T"> The type of object to return
        /// </typeparam>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<T> Get<T>(SPListItemCollection listItems) where T : new()
        {
            var returnList = new List<T>();

            if (listItems.Count > 0)
            {
                var table = listItems.GetDataTable();
                var rows = table.AsEnumerable();

                foreach (var dataRow in rows)
                {
                    returnList.Add(this.Get<T>(dataRow, listItems.Fields));
                }
            }

            return returnList;
        }

        /// <summary>
        /// Creates an entity of the specified type and fills it using the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="dataRow">The data row.</param>
        /// <param name="fieldCollection">The collection of field to get</param>
        /// <returns>
        /// The newly created and filled entity.
        /// </returns>
        public T Get<T>(DataRow dataRow, SPFieldCollection fieldCollection) where T : new()
        {
            var entity = new T();

            this.ToEntity(entity, dataRow, fieldCollection);

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
            this.entityListItemSchemaBuilder.GetSchema(typeof(T)).ToEntity(entity, new ListItemValuesAdapter(listItem), listItem.Fields);
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
        public void ToEntity<T>(T entity, DataRow dataRow, SPFieldCollection fieldCollection)
        {
            this.entitySchemaDataRowBuilder.GetSchema(typeof(T)).ToEntity(entity, new DataRowValuesAdapter(dataRow), fieldCollection);
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItemVersion">The list item version.</param>
        public void ToEntity<T>(T entity, SPListItemVersion listItemVersion)
        {
            this.entityListItemSchemaBuilder.GetSchema(typeof(T)).ToEntity(entity, new ListItemVersionValuesAdapter(listItemVersion), listItemVersion.Fields);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the type converters.
        /// </summary>
        protected internal virtual void RegisterTypeConverters()
        {
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(LookupValue), new LookupValueConverter(this.logger));
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(PrincipalValue), new PrincipalValueConverter());
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(UserValue), new UserValueDataRowConverter());
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(UrlValue), new UrlValueConverter());
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(TaxonomyValue), this.taxonomyValueDataRowConverter);
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(TaxonomyValueCollection), this.taxonomyValueCollectionDataRowConverter);
            this.entitySchemaDataRowBuilder.RegisterTypeConverter(typeof(ImageValue), new ImageValueConverter());

            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(LookupValue), new LookupValueConverter(this.logger));
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(PrincipalValue), new PrincipalValueConverter());
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(UserValue), new UserValueConverter());
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(UserValueCollection), new UserValueCollectionConverter());
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(UrlValue), new UrlValueConverter());
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(TaxonomyValue), this.taxonomyValueConverter);
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(TaxonomyValueCollection), this.taxonomyValueCollectionConverter);
            this.entityListItemSchemaBuilder.RegisterTypeConverter(typeof(ImageValue), new ImageValueConverter());
        }

        #endregion
    }
}