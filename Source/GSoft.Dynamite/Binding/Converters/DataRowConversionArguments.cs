using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.Converters
{
    /// <summary>
    /// The arguments for a SharePoint conversion.
    /// </summary>
    public class DataRowConversionArguments : ConversionArguments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointListItemConversionArguments"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="valueKey">The value key.</param>
        /// <param name="dataRow">The data row.</param>
        /// <param name="fieldCollection">The field Collection.</param>
        /// <param name="fieldValues">The full dictionary of values being converted</param>
        public DataRowConversionArguments(string propertyName, Type propertyType, string valueKey, DataRow dataRow, SPFieldCollection fieldCollection, IDictionary<string, object> fieldValues)
            : base(propertyName, propertyType, valueKey)
        {
            this.FieldCollection = fieldCollection;
            this.DataRow = dataRow;
            this.FieldValues = fieldValues;
        }

        /// <summary>
        /// Gets the list item collection associated to the data row.
        /// </summary>
        public SPFieldCollection FieldCollection { get; private set; }

        /// <summary>
        /// Gets or sets the web.
        /// </summary>
        public SPWeb Web
        {
            get
            {
                return this.FieldCollection.Web;
            }
        }

        /// <summary>
        /// Gets the list item.
        /// </summary>
        public DataRow DataRow { get; private set; }

        /// <summary>
        /// Gets the full dictionary of values being converted
        /// </summary>
        public IDictionary<string, object> FieldValues { get; private set; }
    }
}
