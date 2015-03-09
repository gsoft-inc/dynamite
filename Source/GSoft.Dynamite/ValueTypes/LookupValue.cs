using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// The lookup value.
    /// </summary>
    public class LookupValue
    {
        #region Constructors

        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public LookupValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupValue"/> class.
        /// </summary>
        /// <param name="lookupId">The lookup id.</param>
        /// <param name="value">The value of the looked-up item for the ShowField/LookupField field</param>
        public LookupValue(int lookupId, string value)
        {
            this.Id = lookupId;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupValue"/> class.
        /// </summary>
        /// <param name="fieldLookupValue">The field lookup value.</param>
        public LookupValue(SPFieldLookupValue fieldLookupValue)
        {
            this.Id = fieldLookupValue.LookupId;
            this.Value = fieldLookupValue.LookupValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value { get; set; }

        #endregion
    }
}