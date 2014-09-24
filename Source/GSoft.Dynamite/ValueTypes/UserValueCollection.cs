using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// Multiple user values.
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
        /// <param name="userCollection">The taxonomy values.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "SharePoint is the dirty culprit in exposing Generic Lists, isn't it?")]
        public UserValueCollection(IList<SPUser> userCollection) :
            this(new UserValueCollection(userCollection.Select(user => new UserValue(user)).ToList()))
        {
        }
    }
}
