using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Helps in finding field definitions
    /// </summary>
    public class FieldLocator : IFieldLocator
    {
        /// <summary>
        /// Gets the field by identifier.
        /// Returns null if the field is not found in the collection.
        /// </summary>
        /// <param name="fieldCollection">The field collection.</param>
        /// <param name="fieldId">The field identifier.</param>
        /// <returns>The SPField.</returns>
        public SPField GetFieldById(SPFieldCollection fieldCollection, Guid fieldId)
        {
            if (fieldCollection == null)
            {
                throw new ArgumentNullException("fieldCollection");
            }

            if (fieldId == null)
            {
                throw new ArgumentNullException("fieldId");
            }

            SPField field = null;
            if (fieldCollection.Contains(fieldId))
            {
                field = fieldCollection[fieldId] as SPField;
            }

            return field;
        }
    }
}
