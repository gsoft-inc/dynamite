using System;
using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// A property attribute that defines binding behavior.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is made to be inherited from.")]
    public class PropertyAttribute : Attribute
    {
        #region Fields

        private readonly string propertyName;

        private readonly bool isLookupId;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class.
        /// </summary>
        public PropertyAttribute()
        {
            this.isLookupId = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public PropertyAttribute(string propertyName)
        {
            this.propertyName = propertyName;
            this.isLookupId = false;
        }

        /// <summary>Initializes a new instance of the <see cref="PropertyAttribute"/> class.</summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="isLookupId">The is Lookup.</param>
        public PropertyAttribute(string propertyName, bool isLookupId)
        {
            this.propertyName = propertyName;
            this.isLookupId = isLookupId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public virtual string PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }

        /// <summary>
        /// Gets or sets the type of the binding. By default, a binding is Bidirectional.
        /// </summary>
        public virtual BindingType BindingType { get; set; }

        /// <summary>Gets a value indicating whether is lookup.</summary>
        public bool IsLookupId
        {
            get
            {
                return this.isLookupId;
            }
        }

        #endregion
    }
}