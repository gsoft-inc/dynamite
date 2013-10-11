namespace GSoft.Dynamite.Sharepoint.Binding
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
}
