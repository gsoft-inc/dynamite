using System.Collections.Generic;

namespace GSoft.Dynamite.Binding
{
    using Microsoft.SharePoint;

    /// <summary>
    /// A schema to apply on entities.
    /// </summary>
    public interface IEntitySchema
    {
        #region Methods

        /// <summary>
        /// Fills the values from the entity properties.
        /// </summary>
        /// <param name="sourceEntity">
        /// The source entity.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="fieldCollection">
        /// The field Collection.
        /// </param>
        /// <param name="web">
        /// The web.
        /// </param>
        void FromEntity(
            object sourceEntity,
            IDictionary<string, object> values,
            SPFieldCollection fieldCollection,
            SPWeb web);

        /// <summary>
        /// Fills the entity from the values.
        /// </summary>
        /// <param name="targetEntity">
        /// The target entity.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="fieldCollection">
        /// The field Collection.
        /// </param>
        /// <param name="web">
        /// The web.
        /// </param>
        void ToEntity(object targetEntity, IDictionary<string, object> values, SPFieldCollection fieldCollection, SPWeb web);

        #endregion
    }
}