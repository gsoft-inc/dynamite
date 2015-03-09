using System.Reflection;
using GSoft.Dynamite.ValueTypes.Readers;
using GSoft.Dynamite.ValueTypes.Writers;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// Details for an entity binding.
    /// </summary>
    public class EntityPropertyConversionDetail
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBindingDetail"/> class.
        /// </summary>
        /// <param name="entityProperty">The entity property.</param>
        /// <param name="valueKey">The value key.</param>
        /// <param name="bindingType">Type of the binding.</param>
        /// <param name="valueWriter">Value writer for the associated value type</param>
        /// <param name="valueReader">Value reader for the associated value type</param>
        public EntityPropertyConversionDetail(PropertyInfo entityProperty, string valueKey, BindingType bindingType, IBaseValueWriter valueWriter, IBaseValueReader valueReader)
        {
            this.EntityProperty = entityProperty;
            this.ValueKey = valueKey;
            this.BindingType = bindingType;

            this.ValueWriter = valueWriter;
            this.ValueReader = valueReader;
        }

        #endregion

        #region Properties

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

        /// <summary>
        /// Gets the writer instance that knows how to write the property's value type to SharePoint elements
        /// </summary>
        public IBaseValueWriter ValueWriter { get; private set; }

        /// <summary>
        /// Gets the re instance that knows how to write the property's value type to SharePoint elements
        /// </summary>
        public IBaseValueReader ValueReader { get; private set; }

        #endregion
    }
}