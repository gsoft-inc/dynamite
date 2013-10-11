using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Sharepoint.Binding
{
    /// <summary>
    /// The interface for the entity binder.
    /// </summary>
    public interface IEntityBinder
    {
        #region Methods

        /// <summary>
        /// Extracts the values from the entity to fill the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="values">The values.</param>
        void FromEntity<T>(T entity, IDictionary<string, object> values);

        /// <summary>
        /// Creates an entity of the specified type and fills it using the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="values">The values.</param>
        /// <returns>The newly created and filled entity.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Get is the right word in this context.")]
        T Get<T>(IDictionary<string, object> values) where T : new();

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="values">The values.</param>
        void ToEntity<T>(T entity, IDictionary<string, object> values);

        #endregion
    }
}