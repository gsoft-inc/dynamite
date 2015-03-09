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
    public interface IFieldLocator
    {
        /// <summary>
        /// Gets the field by identifier.
        /// Returns null if the field is not found in the collection.
        /// </summary>
        /// <param name="fieldCollection">The field collection.</param>
        /// <param name="fieldId">The field identifier.</param>
        /// <returns>The SPField or null if field doesn't exist in the field collection</returns>
        SPField GetFieldById(SPFieldCollection fieldCollection, Guid fieldId);
    }
}
