using System;
using System.Globalization;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.Converters
{
    /// <summary>
    /// A converter for a projected Lookup value.
    /// </summary>
    public class ProjectedLookupValueDataRowConverter : DataRowValueConverter
    {
        private readonly string _projectedFieldName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectedLookupValueConverter"/> class.
        /// </summary>
        /// <param name="projectedFieldName">Name of the projected field.</param>
        public ProjectedLookupValueDataRowConverter(string projectedFieldName)
        {
            this._projectedFieldName = projectedFieldName;
        }

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public override object Convert(object value, DataRowConversionArguments arguments)
        {
            var lookupValue = value as SPFieldLookupValue;

            if (value == DBNull.Value)
            {
                return null;
            }

            if (lookupValue == null)
            {
                var stringValue = value as string;
                if (!string.IsNullOrEmpty(stringValue))
                {
                    lookupValue = new SPFieldLookupValue(stringValue);
                }
            }

            if (lookupValue != null)
            {
                var lookupField = new SPFieldLookup(arguments.FieldCollection, arguments.ValueKey);

                if (lookupField.LookupWebId == Guid.Empty || lookupField.LookupWebId == arguments.Web.ID)
                {
                    return GetLookupFieldValue(arguments.Web, lookupField.LookupList, this._projectedFieldName, lookupValue.LookupId);
                }

                using (var web = arguments.Web.Site.OpenWeb(lookupField.LookupWebId))
                {
                    return GetLookupFieldValue(web, lookupField.LookupList, this._projectedFieldName, lookupValue.LookupId);
                }
            }

            return null;
        }

        /// <summary>
        /// Converts the specified value back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public override object ConvertBack(object value, DataRowConversionArguments arguments)
        {
            throw new NotSupportedException();
        }

        private static object GetLookupFieldValue(SPWeb web, string listName, string projectedFieldInternalName, int itemId)
        {
            SPList list;

            try
            {
                list = web.Lists[new Guid(listName)];
            }
            catch (FormatException)
            {
                list = web.Lists[listName];
            }

            var item = list.GetItemById(itemId);

            return item != null ? item[projectedFieldInternalName] : null;
        }
    }
}
