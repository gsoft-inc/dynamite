using GSoft.Dynamite.Globalization;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Microsoft.SharePoint.Taxonomy;

    /// <summary>
    /// An entity type for a user collection.
    /// </summary>
    public class UserValueCollection : Collection<UserValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyValueCollection"/> class.
        /// </summary>
        public UserValueCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaxonomyValue"/> class.
        /// </summary>
        /// <param name="taxonomyValues">The taxonomy value.</param>
        public UserValueCollection(IList<UserValue> userValues) :
            base(userValues)
        {
        }
    }
}