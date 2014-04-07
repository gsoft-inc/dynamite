using GSoft.Dynamite.Repositories.Entities;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Repositories
{
    /// <summary>
    /// Composed look repository interface
    /// </summary>
    public interface IComposedLookRepository
    {
        /// <summary>
        /// Gets the composed look by id.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="id">The id.</param>
        /// <returns>The composed look.</returns>
        ComposedLook GetById(SPWeb web, int id);

        /// <summary>
        /// Retrieves a composed look by name.
        /// </summary>
        /// <param name="web">The current web.</param>
        /// <param name="name">The name of the composed look.</param>
        /// <returns>
        /// Returns a composed look by name.
        /// </returns>
        ComposedLook GetByName(SPWeb web, string name);

        /// <summary>
        /// Creates the specified composed look.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The newly created composed look.</returns>
        ComposedLook Create(SPWeb web, ComposedLook entity);

        /// <summary>
        /// Updates the specified composed look.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="entity">The entity.</param>
        void Update(SPWeb web, ComposedLook entity);

        /// <summary>
        /// Deletes the specified composed look.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="entity">The entity.</param>
        void Delete(SPWeb web, ComposedLook entity);
    }
}