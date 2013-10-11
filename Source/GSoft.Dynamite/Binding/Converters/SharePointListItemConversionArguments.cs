using System;
using System.Collections.Generic;

namespace GSoft.Dynamite.Sharepoint.Binding.Converters
{
    /// <summary>
    /// The arguments for a SharePoint conversion.
    /// </summary>
    public class SharePointListItemConversionArguments : ConversionArguments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointListItemConversionArguments"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="valueKey">The value key.</param>
        /// <param name="listItem">The list item.</param>
        /// <param name="fieldValues">The full dictionary of values being converted</param>
        public SharePointListItemConversionArguments(string propertyName, Type propertyType, string valueKey, SPListItem listItem, IDictionary<string, object> fieldValues)
            : base(propertyName, propertyType, valueKey)
        {
            this.ListItem = listItem;
            this.FieldValues = fieldValues;
        }

        /// <summary>
        /// Gets the list item.
        /// </summary>
        public SPListItem ListItem { get; private set; }

        /// <summary>
        /// Gets the full dictionary of values being converted
        /// </summary>
        public IDictionary<string, object> FieldValues { get; private set; }
    }
}
