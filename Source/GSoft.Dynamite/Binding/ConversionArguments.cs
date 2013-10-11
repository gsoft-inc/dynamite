using System;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// Arguments for a conversion.
    /// </summary>
    public class ConversionArguments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConversionArguments"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="valueKey">Name of the target.</param>
        public ConversionArguments(string propertyName, Type propertyType, string valueKey)
        {
            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
            this.ValueKey = valueKey;
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// Gets the name of the target.
        /// </summary>
        public string ValueKey { get; private set; }
    }
}
