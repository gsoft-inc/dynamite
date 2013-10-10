using System;
using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Sharepoint2013.Binding
{
    /// <summary>
    /// A property attribute that defines binding behavior.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is made to be inherited from.")]
    public class PropertyAttribute : Attribute
    {
        #region Fields

        private readonly string _propertyName;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class.
        /// </summary>
        public PropertyAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public PropertyAttribute(string propertyName)
        {
            this._propertyName = propertyName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the type of the converter.
        /// </summary>
        public virtual Type ConverterType { get; set; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public virtual string PropertyName
        {
            get
            {
                return this._propertyName;
            }
        }

        /// <summary>
        /// Gets or sets the type of the binding. By default, a binding is Bidirectional.
        /// </summary>
        public virtual BindingType BindingType { get; set; }

        #endregion

        #region Creates an instance of the converter

        /// <summary>
        /// Gets the custom converter.
        /// </summary>
        /// <returns>
        /// The converter.
        /// </returns>
        protected internal virtual IConverter CreateConverter()
        {
            if (this.ConverterType != null)
            {
                var instance = Activator.CreateInstance(this.ConverterType) as IConverter;
                if (instance == null)
                {
                    throw new InvalidOperationException("The custom converter associated to a property must implement the IConverter interface.");
                }

                return instance;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}