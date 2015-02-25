using System;
using System.Collections.Generic;
using System.Data;
using GSoft.Dynamite.Fields;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Handles reading values from a SharePoint list item, from a DataRow obtained from a CAML query
    /// over list items or from a DataRow obtained from a Search query.
    /// </summary>
    public interface IFieldValueReader 
    {
        /// <summary>
        /// Reads a field value from a list item
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="item">The list item we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        T ReadValueFromListItem<T>(SPListItem item, string fieldInternalName);

        /// <summary>
        /// Reads a field value from a list item version
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="itemVersion">The list item version we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        T ReadValueFromListItemVersion<T>(SPListItemVersion itemVersion, string fieldInternalName);

        /// <summary>
        /// Reads a field value from a DataRow returned by a CAML query
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="dataRowFromCamlResult">The CAML-query-result data row we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field among the data row cells</param>
        /// <returns>The value extracted from the data row's corresponding cell</returns>
        T ReadValueFromCamlResultDataRow<T>(DataRow dataRowFromCamlResult, string fieldInternalName);

        /// <summary>
        /// Reads a field value from a DataRow returned by a Search query
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="dataRowFromCamlResult">The CAML-query-result data row we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field among the data row cells</param>
        /// <returns>The value extracted from the data row's corresponding cell</returns>
        ////T ReadValueFromSearchResultDataRow<T>(DataRow dataRowFromSearchResult, string fieldManagedPropertyName);
    }
}