namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// The type of binding used in a bound Property.
    /// </summary>
    public enum BindingType
    {
        /// <summary>
        /// The binding is bidirectional, both when reading and writing.
        /// </summary>
        Bidirectional,

        /// <summary>
        /// The binding will only read from the source.
        /// </summary>
        ReadOnly,

        /// <summary>
        /// The binding will only write the value to the destination.
        /// </summary>
        WriteOnly,
    }

    /// <summary>
    /// The Requirement status of a field
    /// </summary>
    public enum RequiredType
    {
        /// <summary>
        /// Inherit its value from the Field Definition
        /// </summary>
        Inherit,

        /// <summary>
        /// The FieldLink in the Content type is required
        /// </summary>
        Required,

        /// <summary>
        /// The FieldLink in the Content Type is not required
        /// </summary>
        NotRequired
    }
}
