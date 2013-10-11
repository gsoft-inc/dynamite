using System.Collections.Generic;
using System.Linq;

namespace GSoft.Dynamite.Sharepoint.Binding
{
    /// <summary>
    /// The schema for an entity.
    /// </summary>
    public class EntitySchema : IEntitySchema
    {
        #region Fields

        private readonly LinkedList<EntityBindingDetail> _bindingDetails = new LinkedList<EntityBindingDetail>();

        #endregion

        #region IEntitySchema Members

        /// <summary>
        /// Fills the values from the entity properties.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="values">The values.</param>
        public void FromEntity(object sourceEntity, IDictionary<string, object> values)
        {
            foreach (var binding in this._bindingDetails.Where(x => x.BindingType == BindingType.Bidirectional || x.BindingType == BindingType.WriteOnly))
            {
                var value = binding.EntityProperty.GetValue(sourceEntity, null);
                value = binding.Converter.ConvertBack(value, this.GetConversionArguments(binding, values));
                values[binding.ValueKey] = value;
            }
        }

        /// <summary>
        /// Fills the entity from the values.
        /// </summary>
        /// <param name="targetEntity">The target entity.</param>
        /// <param name="values">The values.</param>
        public void ToEntity(object targetEntity, IDictionary<string, object> values)
        {
            foreach (var binding in this._bindingDetails.Where(x => x.BindingType == BindingType.Bidirectional || x.BindingType == BindingType.ReadOnly))
            {
                object value;
                if (!values.TryGetValue(binding.ValueKey, out value))
                {
                    value = null;
                }

                value = binding.Converter.Convert(value, this.GetConversionArguments(binding, values));
                binding.EntityProperty.SetValue(targetEntity, value, null);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the conversion arguments.
        /// </summary>
        /// <param name="bindingDetail">The binding detail.</param>
        /// <param name="values">The values.</param>
        /// <returns>The conversion arguments.</returns>
        protected internal virtual ConversionArguments GetConversionArguments(EntityBindingDetail bindingDetail, IDictionary<string, object> values)
        {
            return new ConversionArguments(bindingDetail.EntityProperty.Name, bindingDetail.EntityProperty.PropertyType, bindingDetail.ValueKey);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="detail">The detail.</param>
        protected internal void AddProperty(EntityBindingDetail detail)
        {
            this._bindingDetails.AddLast(detail);
        }

        #endregion
    }
}