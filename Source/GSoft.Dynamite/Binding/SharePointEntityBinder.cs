using System;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Binding.Converters;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// The default entity binder for SharePoint.
    /// </summary>
    [CLSCompliant(false)]
    public class SharePointEntityBinder : ISharePointEntityBinder
    {
        #region Fields

        private readonly IEntityBinder _entityBinder;

        private readonly IEntitySchemaBuilder _entitySchemaBuilder;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointEntityBinder"/> class.
        /// </summary>
        public SharePointEntityBinder()
            : this(new EntitySchemaBuilder<SharePointEntitySchema>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointEntityBinder"/> class.
        /// </summary>
        /// <param name="entitySchemaBuilder">The entity schema builder.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The types must be registred in the constructor.")]
        public SharePointEntityBinder(IEntitySchemaBuilder entitySchemaBuilder)
        {
            this._entitySchemaBuilder = entitySchemaBuilder;
            this._entityBinder = new EntityBinder(entitySchemaBuilder);

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
            this._entityBinder.FromEntity(entity, new ListItemValuesAdapter(listItem));
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
            return this._entityBinder.Get<T>(new ListItemValuesAdapter(listItem));
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
            return this._entityBinder.Get<T>(new ListItemVersionValuesAdapter(listItemVersion));
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItem">The list item.</param>
        public void ToEntity<T>(T entity, SPListItem listItem)
        {
            this._entityBinder.ToEntity(entity, new ListItemValuesAdapter(listItem));
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItemVersion">The list item version.</param>
        public void ToEntity<T>(T entity, SPListItemVersion listItemVersion)
        {
            this._entityBinder.ToEntity(entity, new ListItemVersionValuesAdapter(listItemVersion));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the type converters.
        /// </summary>
        protected internal virtual void RegisterTypeConverters()
        {
            this._entitySchemaBuilder.RegisterTypeConverter(typeof(LookupValue), new LookupValueConverter());
            this._entitySchemaBuilder.RegisterTypeConverter(typeof(PrincipalValue), new PrincipalValueConverter());
            this._entitySchemaBuilder.RegisterTypeConverter(typeof(UserValue), new UserValueConverter());
            this._entitySchemaBuilder.RegisterTypeConverter(typeof(UrlValue), new UrlValueConverter());
            this._entitySchemaBuilder.RegisterTypeConverter(typeof(TaxonomyValue), new TaxonomyValueConverter());
            this._entitySchemaBuilder.RegisterTypeConverter(typeof(TaxonomyValueCollection), new TaxonomyValueCollectionConverter());
            this._entitySchemaBuilder.RegisterTypeConverter(typeof(ImageValue), new ImageValueConverter());
        }

        #endregion
    }
}