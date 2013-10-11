namespace GSoft.Dynamite.Sharepoint.ValueTypes
{
    /// <summary>
    /// The lookup value.
    /// </summary>
    public class LookupValue
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LookupValue"/> class.
        /// </summary>
        /// <param name="lookupId">The lookup id.</param>
        public LookupValue(int lookupId)
        {
            this.Id = lookupId;
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
        public int Id { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value { get; private set; }

        #endregion
    }
}