using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Helps in configuring lookup fields
    /// </summary>
    public interface IFieldLookupHelper
    {
        /// <summary>
        /// Sets the lookup to a list.
        /// </summary>
        /// <param name="fieldCollection">The field collection.</param>
        /// <param name="fieldId">The field identifier of the lookup field.</param>
        /// <param name="lookupList">The lookup list.</param>
        /// <exception cref="System.ArgumentNullException">
        /// fieldCollection
        /// or
        /// fieldId
        /// or
        /// lookupList
        /// </exception>
        /// <exception cref="System.ArgumentException">Unable to find the lookup field.;fieldId</exception>
        void SetLookupToList(SPFieldCollection fieldCollection, Guid fieldId, SPList lookupList);

        /// <summary>
        /// Sets the lookup to a list.
        /// </summary>
        /// <param name="lookupField">The lookup field.</param>
        /// <param name="lookupList">The lookup list.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The parameter 'lookupField' cannot be null.;lookupField
        /// or
        /// The parameter 'lookupList' cannot be null.;lookupList
        /// </exception>
        void SetLookupToList(SPFieldLookup lookupField, SPList lookupList);
    }
}
