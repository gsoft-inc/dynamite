using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Lists.Constants;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Readers
{
    /// <summary>
    /// Reads DateTime-based field values
    /// </summary>
    public class DateTimeValueReader : BaseValueReader<DateTime?>
    {
        /// <summary>
        /// Reads a field value from a list item
        /// </summary>
        /// <param name="item">The list item we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        public override DateTime? ReadValueFromListItem(SPListItem item, string fieldInternalName)
        {
            var fieldValue = item[fieldInternalName];

            if (fieldValue != null)
            {
                if ((int)item.ParentList.BaseTemplate == BuiltInListTemplates.Pages.ListTempateTypeId)
                {
                    // There's more than a good chance that your SPListItem comes from the result of
                    // a PublishingWeb.GetPublishingPages(SPQuery) call. This call is sneaky and 
                    // returns a UTC-time SPListItem instance linked to the PublishingPage object.
                    var dateTime = (DateTime)fieldValue;
                    return dateTime.ToLocalTime(); 
                }
                else
                {
                    return (DateTime)fieldValue;
                }
            }

            return null;
        }

        /// <summary>
        /// Reads a field value from a list item version
        /// </summary>
        /// <param name="itemVersion">The list item version we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        public override DateTime? ReadValueFromListItemVersion(SPListItemVersion itemVersion, string fieldInternalName)
        {
            var fieldValue = itemVersion[fieldInternalName];

            if (fieldValue != null)
            {
                var dateTime = (DateTime)fieldValue;
                return dateTime.ToLocalTime();  // Weird, but list item version returns the datetime objects in UTC 
                                                // (while SPListItem takes care of the UTC-to-local conversion)
            }

            return null;
        }

        /// <summary>
        /// Reads a field value from a DataRow returned by a CAML query
        /// </summary>
        /// <param name="web">The context's web</param>
        /// <param name="dataRowFromCamlResult">The CAML-query-result data row we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field among the data row cells</param>
        /// <returns>The value extracted from the data row's corresponding cell</returns>
        public override DateTime? ReadValueFromCamlResultDataRow(SPWeb web, DataRow dataRowFromCamlResult, string fieldInternalName)
        {
            var fieldValue = dataRowFromCamlResult[fieldInternalName];

            if (fieldValue != null && fieldValue != System.DBNull.Value)
            {
                return fieldValue as DateTime?;
            }

            return null;
        }
    }
}