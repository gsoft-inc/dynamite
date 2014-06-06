using System.Collections.Generic;
using GSoft.Dynamite.Binding.Converters;

namespace GSoft.Dynamite.Binding
{
    using System.Web.UI.WebControls;

    using Microsoft.SharePoint;

    /// <summary>
    /// A schema for SharePoint entities.
    /// </summary>
    public class SharePointDataRowEntitySchema : EntitySchema
    {
        /// <summary>
        /// Creates the conversion arguments.
        /// </summary>
        /// <param name="bindingDetail">The binding detail.</param>
        /// <param name="values">The values.</param>
        /// <param name="fieldCollection">The item Collection.</param>
        /// <param name="web">The web.</param>
        /// <returns>
        /// The conversion arguments.
        /// </returns>
        protected internal override ConversionArguments GetConversionArguments(EntityBindingDetail bindingDetail, IDictionary<string, object> values, SPFieldCollection fieldCollection, SPWeb web)
        {
            var listItemValues = values as IDataRowValues;
            
            if (listItemValues != null)
            {
                return new DataRowConversionArguments(bindingDetail.EntityProperty.Name, bindingDetail.EntityProperty.PropertyType, bindingDetail.ValueKey, listItemValues.DataRow, fieldCollection, web, values);
            }
            else
            {
                return base.GetConversionArguments(bindingDetail, values, fieldCollection, web);
            }
        }
    }
}
