using System;
using GSoft.Dynamite.Sharepoint.Binding.Converters;

namespace GSoft.Dynamite.Sharepoint.Binding
{
    /// <summary>
    /// A projected property from a Lookup value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ProjectedPropertyAttribute : PropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectedPropertyAttribute"/> class.
        /// </summary>
        /// <param name="projectedFieldName">Name of the projected field.</param>
        public ProjectedPropertyAttribute(string projectedFieldName)
        {
            if (string.IsNullOrEmpty(this.ProjectedFieldName))
            {
                throw new ArgumentNullException("projectedFieldName");
            }

            this.ProjectedFieldName = projectedFieldName;
            base.BindingType = BindingType.ReadOnly;
        }

        /// <summary>
        /// Gets or sets the type of the binding. Only Read-Only bindings are supported for a projected property.
        /// </summary>
        public override BindingType BindingType
        {
            get
            {
                return base.BindingType;
            }

            set
            {
                if (value != BindingType.ReadOnly)
                {
                    throw new ArgumentException("Projected properties must be read-only");
                }

                base.BindingType = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the projected field.
        /// </summary>
        /// <value>
        /// The name of the projected field.
        /// </value>
        public string ProjectedFieldName { get; private set; }

        /// <summary>
        /// Gets the custom converter.
        /// </summary>
        /// <returns>
        /// The converter.
        /// </returns>
        protected internal override IConverter CreateConverter()
        {
            return new ProjectedLookupValueConverter(this.ProjectedFieldName);
        }
    }
}
