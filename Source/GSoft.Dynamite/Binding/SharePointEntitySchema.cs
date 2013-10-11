using System.Collections.Generic;
using GSoft.Dynamite.Binding.Converters;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// A schema for SharePoint entities.
    /// </summary>
    public class SharePointEntitySchema : EntitySchema
    {
        /// <summary>
        /// Creates the conversion arguments.
        /// </summary>
        /// <param name="bindingDetail">The binding detail.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        /// The conversion arguments.
        /// </returns>
        protected internal override ConversionArguments GetConversionArguments(EntityBindingDetail bindingDetail, IDictionary<string, object> values)
        {
            var listItemValues = values as ISharePointListItemValues;
            if (listItemValues != null)
            {
                return new SharePointListItemConversionArguments(bindingDetail.EntityProperty.Name, bindingDetail.EntityProperty.PropertyType, bindingDetail.ValueKey, listItemValues.ListItem, values);
            }
            else
            {
                return base.GetConversionArguments(bindingDetail, values);
            }
        }
    }
}
