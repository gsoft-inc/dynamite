using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using Microsoft.SharePoint;
using GSoft.Dynamite.ValueTypes.Readers;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Defines the generic contract for all ValueType readers
    /// </summary>
    public abstract class BaseValueReader<T> : IBaseValueReader
    {
        /// <summary>
        /// The ValueType with which the reader is compatible
        /// </summary>
        public Type AssociatedValueType 
        { 
            get
            {
                return typeof(T);
            }
        }

        /// <summary>
        /// Reads a field value from a list item
        /// </summary>
        /// <param name="item">The list item we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        public abstract T ReadValueFromListItem(SPListItem item, string fieldInternalName);

        /// <summary>
        /// Reads a field value from a list item version
        /// </summary>
        /// <param name="itemVersion">The list item version we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        public abstract T ReadValueFromListItemVersion(SPListItemVersion itemVersion, string fieldInternalName);

        /// <summary>
        /// Reads a field value from a DataRow returned by a CAML query
        /// </summary>
        /// <param name="dataRowFromCamlResult">The CAML-query-result data row we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field among the data row cells</param>
        /// <returns>The value extracted from the data row's corresponding cell</returns>
        public abstract T ReadValueFromCamlResultDataRow(DataRow dataRowFromCamlResult, string fieldInternalName);
    }
}