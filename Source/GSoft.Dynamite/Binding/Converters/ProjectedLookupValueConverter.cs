using System;
using System.Globalization;

namespace GSoft.Dynamite.Binding.Converters
{
    /// <summary>
    /// A converter for a projected Lookup value.
    /// </summary>
    public class ProjectedLookupValueConverter : SharePointListItemValueConverter
    {
        private readonly string _projectedFieldName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectedLookupValueConverter"/> class.
        /// </summary>
        /// <param name="projectedFieldName">Name of the projected field.</param>
        public ProjectedLookupValueConverter(string projectedFieldName)
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
        public override object Convert(object value, SharePointListItemConversionArguments arguments)
        {
            var lookupValue = value as SPFieldLookupValue;
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
                var lookupField = arguments.ListItem.Fields.GetFieldByInternalName(arguments.ValueKey) as SPFieldLookup;
                if (lookupField != null)
                {
                    if (lookupField.LookupWebId == Guid.Empty || lookupField.LookupWebId == arguments.ListItem.Web.ID)
                    {
                        return GetLookupFieldValue(arguments.ListItem.Web, lookupField.LookupList, this._projectedFieldName, lookupValue.LookupId);
                    }
                    else
                    {
                        using (var web = arguments.ListItem.Web.Site.OpenWeb(lookupField.LookupWebId))
                        {
                            return GetLookupFieldValue(web, lookupField.LookupList, this._projectedFieldName, lookupValue.LookupId);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "No lookup field with Internal Name '{0}' could be found.", arguments.ValueKey));
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
        public override object ConvertBack(object value, SharePointListItemConversionArguments arguments)
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
