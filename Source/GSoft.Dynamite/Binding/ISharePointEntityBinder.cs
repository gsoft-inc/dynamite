using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding
{
    using System.Data;

    /// <summary>
    /// The binder for SharePoint entities.
    /// </summary>
    [CLSCompliant(false)]
    public interface ISharePointEntityBinder
    {
        #region Methods

        /// <summary>
        /// Extracts the values from the entity to fill the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItem">The list item.</param>
        void FromEntity<T>(T entity, SPListItem listItem);

        /// <summary>
        /// Creates an entity of the specified type and fills it using the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="listItem">The list item.</param>
        /// <returns>
        /// The newly created and filled entity.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Get is the right word in this context.")]
        T Get<T>(SPListItem listItem) where T : new();

        /// <summary>
        /// Creates an entity of the specified type and fills it using the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="dataRow">The data row.</param>
        /// <returns>
        /// The newly created and filled entity.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Get is the right word in this context.")]
        T Get<T>(DataRow dataRow, SPFieldCollection fieldCollection, SPWeb web) where T : new();

        /// <summary>
        /// Creates an entity of the specified type and fills it using the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="listItemVersion">The list item version.</param>
        /// <returns>
        /// The newly created and filled entity.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Get is the right word in this context.")]
        T Get<T>(SPListItemVersion listItemVersion) where T : new();

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItem">The list item.</param>
        void ToEntity<T>(T entity, SPListItem listItem);

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItemVersion">The list item version.</param>
        void ToEntity<T>(T entity, SPListItemVersion listItemVersion);

        #endregion
    }
}