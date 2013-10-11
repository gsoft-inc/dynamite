using System.Collections.Generic;

namespace GSoft.Dynamite.Sharepoint2013.Binding
{
    /// <summary>
    /// The default entity binder class.
    /// </summary>
    public class EntityBinder : IEntityBinder
    {
        #region Fields

        private readonly IEntitySchemaBuilder _schemaBuilder;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBinder"/> class.
        /// </summary>
        public EntityBinder()
            : this(new EntitySchemaBuilder<EntitySchema>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBinder"/> class.
        /// </summary>
        /// <param name="schemaBuilder">The schema builder.</param>
        public EntityBinder(IEntitySchemaBuilder schemaBuilder)
        {
            this._schemaBuilder = schemaBuilder;
        }

        #endregion

        #region IEntityBinder Members

        /// <summary>
        /// Extracts the values from the entity to fill the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="values">The values.</param>
        public void FromEntity<T>(T entity, IDictionary<string, object> values)
        {
            this._schemaBuilder.GetSchema(typeof(T)).FromEntity(entity, values);
        }

        /// <summary>
        /// Gets the specified item.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="values">The values.</param>
        /// <returns>
        /// The new entity.
        /// </returns>
        public T Get<T>(IDictionary<string, object> values) where T : new()
        {
            var entity = new T();

            this.ToEntity(entity, values);

            return entity;
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="values">The values.</param>
        public void ToEntity<T>(T entity, IDictionary<string, object> values)
        {
            this._schemaBuilder.GetSchema(typeof(T)).ToEntity(entity, values);
        }

        #endregion
    }
}