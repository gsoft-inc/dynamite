using Microsoft.SharePoint;

namespace GSoft.Dynamite.Repositories
{
    /// <summary>
    /// Interface for Generic Repository
    /// </summary>
    /// <typeparam name="T">The Type that the Repository will serve</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Retrieves an entity by id
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="id">Id the list item</param>
        /// <returns>Returns an entity</returns>
        T GetById(SPWeb web, int id);

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="entity">The entity</param>
        void Create(SPWeb web, T entity);

        /// <summary>
        /// Update the entity
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="entity">The entity to update</param>
        void Update(SPWeb web, T entity);

        /// <summary>
        /// Publish the entity
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="entity">The entity to update</param>
        /// <param name="comment">The publishing comment</param>
        void Publish(SPWeb web, T entity, string comment);

        /// <summary>
        /// Approve the entity
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="entity">The entity to update</param>
        /// <param name="comment">The Approval comment</param>
        void Approve(SPWeb web, T entity, string comment);

        /// <summary>
        /// Delete a configuration value
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="entity">The entity</param>
        void Delete(SPWeb web, T entity);
    }
}
