using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes.Readers;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Handles reading values from a SharePoint list item, from a DataRow obtained from a CAML query
    /// over list items or from a DataRow obtained from a Search query.
    /// </summary>
    public class FieldValueReader : IFieldValueReader 
    {
        private readonly IDictionary<Type, IBaseValueReader> readers = new Dictionary<Type, IBaseValueReader>();

        public FieldValueReader(
            StringValueReader stringValueReader,
            BooleanValueReader booleanValueReader,
            ImageValueReader imageValueReader
            )
        {
            this.AddToReadersDictionary(stringValueReader);
            this.AddToReadersDictionary(booleanValueReader);
            this.AddToReadersDictionary(imageValueReader);
        }

        /// <summary>
        /// Reads a field value from a list item
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="item">The list item we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        public T ReadValueFromListItem<T>(SPListItem item, string fieldInternalName)
        {
            IBaseValueReader selectedReader = this.GetReader(typeof(T));
            object valueThatWasRead = selectedReader.GetType().GetMethod("ReadValueFromListItem").Invoke(selectedReader, new object[] { item, fieldInternalName });
            return (T)valueThatWasRead;
        }

        /// <summary>
        /// Reads a field value from a list item version
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="itemVersion">The list item version we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        public T ReadValueFromListItemVersion<T>(SPListItemVersion itemVersion, string fieldInternalName)
        {
            IBaseValueReader selectedReader = this.GetReader(typeof(T));
            object valueThatWasRead = selectedReader.GetType().GetMethod("ReadValueFromListItemVersion").Invoke(selectedReader, new object[] { itemVersion, fieldInternalName });
            return (T)valueThatWasRead;
        }

        /// <summary>
        /// Reads a field value from a DataRow returned by a CAML query
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="dataRowFromCamlResult">The CAML-query-result data row we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field among the data row cells</param>
        /// <returns>The value extracted from the data row's corresponding cell</returns>
        public T ReadValueFromCamlResultDataRow<T>(DataRow dataRowFromCamlResult, string fieldInternalName)
        {
            IBaseValueReader selectedReader = this.GetReader(typeof(T));
            object valueThatWasRead = selectedReader.GetType().GetMethod("ReadValueFromCamlResultDataRow").Invoke(selectedReader, new object[] { dataRowFromCamlResult, fieldInternalName });
            return (T)valueThatWasRead;
        }

        /// <summary>
        /// Reads a field value from a DataRow returned by a Search query
        /// </summary>
        /// <typeparam name="T">The field's associated value type</typeparam>
        /// <param name="dataRowFromCamlResult">The CAML-query-result data row we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field among the data row cells</param>
        /// <returns>The value extracted from the data row's corresponding cell</returns>
        ////T ReadValueFromSearchResultDataRow<T>(DataRow dataRowFromSearchResult, string fieldManagedPropertyName);
        private void AddToReadersDictionary(IBaseValueReader reader)
        {
            this.readers.Add(reader.AssociatedValueType, reader);
        }

        private IBaseValueReader GetReader(Type typeOfValueWeWantToRead)
        {
            Type readerTypeArgument = typeOfValueWeWantToRead;
            if (typeOfValueWeWantToRead.IsValueType || typeOfValueWeWantToRead.IsPrimitive)
            {
                // Readers for primitives or structs always handles the Nullable versions of those value types
                readerTypeArgument = typeof(Nullable<>).MakeGenericType(typeOfValueWeWantToRead);
            }

            if (this.readers.ContainsKey(readerTypeArgument))
            {
                return readers[readerTypeArgument];
            }
            else
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Failed to find a value reader for the specified AssociatedValueType (valueType={0})",
                    typeOfValueWeWantToRead.ToString()));
            }
        }
    }
}