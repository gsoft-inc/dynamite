using System;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Binding.Converters;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// The default entity binder for SharePoint.
    /// </summary>
    public class SharePointEntityBinder : ISharePointEntityBinder
    {
        #region Fields

        private readonly IEntityBinder entityBinder;
        private readonly IEntitySchemaBuilder entitySchemaBuilder;
        private readonly TaxonomyValueConverter taxonomyValueConverter;
        private readonly TaxonomyValueCollectionConverter taxonomyValueCollectionConverter;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointEntityBinder"/> class.
        /// </summary>
        /// <param name="entitySchemaBuilder">The entity schema builder.</param>
        /// <param name="taxonomyValueConverter">The taxonomy value converter</param>
        /// <param name="taxonomyValueCollectionConverter">The Taxonomy value collection converter</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The types must be registred in the constructor.")]
        public SharePointEntityBinder(IEntitySchemaBuilder entitySchemaBuilder, TaxonomyValueConverter taxonomyValueConverter, TaxonomyValueCollectionConverter taxonomyValueCollectionConverter)
        {
            this.entitySchemaBuilder = entitySchemaBuilder;
            this.entityBinder = new EntityBinder(entitySchemaBuilder);
            this.taxonomyValueConverter = taxonomyValueConverter;
            this.taxonomyValueCollectionConverter = taxonomyValueCollectionConverter;
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
            this.entityBinder.FromEntity(entity, new ListItemValuesAdapter(listItem));
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
            return this.entityBinder.Get<T>(new ListItemValuesAdapter(listItem));
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
            return this.entityBinder.Get<T>(new ListItemVersionValuesAdapter(listItemVersion));
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItem">The list item.</param>
        public void ToEntity<T>(T entity, SPListItem listItem)
        {
            this.entityBinder.ToEntity(entity, new ListItemValuesAdapter(listItem));
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItemVersion">The list item version.</param>
        public void ToEntity<T>(T entity, SPListItemVersion listItemVersion)
        {
            this.entityBinder.ToEntity(entity, new ListItemVersionValuesAdapter(listItemVersion));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the type converters.
        /// </summary>
        protected internal virtual void RegisterTypeConverters()
        {
            this.entitySchemaBuilder.RegisterTypeConverter(typeof(LookupValue), new LookupValueConverter());
            this.entitySchemaBuilder.RegisterTypeConverter(typeof(PrincipalValue), new PrincipalValueConverter());
            this.entitySchemaBuilder.RegisterTypeConverter(typeof(UserValue), new UserValueConverter());
            this.entitySchemaBuilder.RegisterTypeConverter(typeof(UrlValue), new UrlValueConverter());
            this.entitySchemaBuilder.RegisterTypeConverter(typeof(TaxonomyValue), this.taxonomyValueConverter);
            this.entitySchemaBuilder.RegisterTypeConverter(typeof(TaxonomyValueCollection), this.taxonomyValueCollectionConverter);
            this.entitySchemaBuilder.RegisterTypeConverter(typeof(ImageValue), new ImageValueConverter());
        }

        #endregion
    }
}