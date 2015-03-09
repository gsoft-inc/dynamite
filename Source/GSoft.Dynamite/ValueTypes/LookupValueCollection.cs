using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// Multiple lookup values.
    /// </summary>
    public class LookupValueCollection : Collection<LookupValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LookupValueCollection"/> class.
        /// </summary>
        public LookupValueCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupValueCollection"/> class.
        /// </summary>
        /// <param name="userValues">The user values.</param>
        public LookupValueCollection(IList<LookupValue> userValues) :
            base(userValues)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupValueCollection"/> class.
        /// </summary>
        /// <param name="lookupFieldList">The taxonomy values.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "SharePoint is the dirty culprit in exposing Generic Lists, isn't it?")]
        public LookupValueCollection(IList<SPFieldLookupValue> lookupFieldList) :
            this(new LookupValueCollection(lookupFieldList.Select(lookupFieldValue => new LookupValue(lookupFieldValue)).ToList()))
        {
        }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="LookupValueCollection"/> class.
        /// </summary>
        /// <remarks>This constructor will not ensure that the labels respect the CurrentUICulture</remarks>
        /// <param name="lookupFieldCollection">The taxonomy values.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "SharePoint is the dirty culprit in exposing Generic Lists, isn't it?")]
        public LookupValueCollection(SPFieldLookupValueCollection lookupFieldCollection) :
            this(new LookupValueCollection(lookupFieldCollection.Select(lookupFieldValue => new LookupValue(lookupFieldValue)).ToList()))
        {
        }
    }
}
