using System.Reflection;

namespace GSoft.Dynamite.Sharepoint2013.Binding
{
    /// <summary>
    /// Details for an entity binding.
    /// </summary>
    public class EntityBindingDetail
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBindingDetail"/> class.
        /// </summary>
        /// <param name="entityProperty">The entity property.</param>
        /// <param name="valueKey">The value key.</param>
        /// <param name="converter">The converter.</param>
        /// <param name="bindingType">Type of the binding.</param>
        public EntityBindingDetail(PropertyInfo entityProperty, string valueKey, IConverter converter, BindingType bindingType)
        {
            this.EntityProperty = entityProperty;
            this.ValueKey = valueKey;
            this.Converter = converter;
            this.BindingType = bindingType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the converter.
        /// </summary>
        public IConverter Converter { get; private set; }

        /// <summary>
        /// Gets or sets the entity property.
        /// </summary>
        public PropertyInfo EntityProperty { get; private set; }

        /// <summary>
        /// Gets or sets the value key.
        /// </summary>
        public string ValueKey { get; private set; }

        /// <summary>
        /// Gets the type of the binding.
        /// </summary>
        public BindingType BindingType { get; private set; }

        #endregion
    }
}