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
        /// Initializes a new instance of the <see cref="UserValueCollection"/> class.
        /// </summary>
        public UserValueCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserValueCollection"/> class.
        /// </summary>
        /// <param name="userValues">The user values.</param>
        public UserValueCollection(IList<UserValue> userValues) :
            base(userValues)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserValueCollection"/> class.
        /// </summary>
        /// <param name="userFieldValueCollection">The user values.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "SharePoint is the dirty culprit in exposing Generic Lists, isn't it?")]
        public UserValueCollection(SPFieldUserValueCollection userFieldValueCollection) :
            this(new UserValueCollection(userFieldValueCollection.Select(userFieldValue => new UserValue(userFieldValue.User)).ToList()))
        {
        }
    }
}