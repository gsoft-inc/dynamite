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

        /// <summary>
        /// Creates a new instance of <see cref="FieldValueReader"/>
        /// </summary>
        /// <param name="stringValueReader">String value reader</param>
        /// <param name="boolValueReader">Boolean value reader</param>
        /// <param name="integerValueReader">Integer value reader</param>
        /// <param name="doubleValueReader">Double value reader</param>
        /// <param name="dateTimeValueReader">DateTime value reader</param>
        /// <param name="guidValueReader">Guid value reader</param>
        /// <param name="taxonomyValueReader">Taxonomy value reader</param>
        /// <param name="taxonomyValueCollectionReader">Taxonomy value collection reader</param>
        /// <param name="lookupValueReader">Lookup value reader</param>
        /// <param name="lookupValueCollectionReader">Lookup value collection reader</param>
        /// <param name="principalValueReader">Principal value reader</param>
        /// <param name="userValueReader">User value reader</param>
        /// <param name="userValueCollectionReader">User value collection reader</param>
        /// <param name="urlValueReader">Url value reader</param>
        /// <param name="imageValueReader">Publishing image value reader</param>
        /// <param name="mediaValueReader">Nedia value reader</param>
        public FieldValueReader(
            StringValueReader stringValueReader,
            BooleanValueReader boolValueReader,
            IntegerValueReader integerValueReader,
            DoubleValueReader doubleValueReader,
            DateTimeValueReader dateTimeValueReader,
            GuidValueReader guidValueReader,
            TaxonomyValueReader taxonomyValueReader,
            TaxonomyValueCollectionReader taxonomyValueCollectionReader,
            LookupValueReader lookupValueReader,
            LookupValueCollectionReader lookupValueCollectionReader,
            PrincipalValueReader principalValueReader,
            UserValueReader userValueReader,
            UserValueCollectionReader userValueCollectionReader,
            UrlValueReader urlValueReader,
            ImageValueReader imageValueReader,
            MediaValueReader mediaValueReader)
        {
            this.AddToReadersDictionary(stringValueReader);
            this.AddToReadersDictionary(boolValueReader);
            this.AddToReadersDictionary(integerValueReader);
            this.AddToReadersDictionary(doubleValueReader);
            this.AddToReadersDictionary(dateTimeValueReader);
            this.AddToReadersDictionary(guidValueReader);
            this.AddToReadersDictionary(taxonomyValueReader);
            this.AddToReadersDictionary(taxonomyValueCollectionReader);
            this.AddToReadersDictionary(lookupValueReader);
            this.AddToReadersDictionary(lookupValueCollectionReader);
            this.AddToReadersDictionary(principalValueReader);
            this.AddToReadersDictionary(userValueReader);
            this.AddToReadersDictionary(userValueCollectionReader);
            this.AddToReadersDictionary(urlValueReader);
            this.AddToReadersDictionary(imageValueReader);
            this.AddToReadersDictionary(mediaValueReader);
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
        /// Gets the registered value reader instance for the specified type
        /// </summary>
        /// <param name="valueType">The value type we wish to read</param>
        /// <returns>The value reader that you should then cast down to a generic BaseValueReader of Type valueType</returns>
        public IBaseValueReader GetValueReaderForType(Type valueType)
        {
            Type readerTypeArgument = valueType;
            if ((valueType.IsValueType || valueType.IsPrimitive) && !valueType.Name.StartsWith("Nullable", StringComparison.OrdinalIgnoreCase))
            {
                // Readers for primitives or structs always handles the Nullable versions of those value types
                readerTypeArgument = typeof(Nullable<>).MakeGenericType(valueType);
            }

            if (this.readers.ContainsKey(readerTypeArgument))
            {
                return this.readers[readerTypeArgument];
            }

            return null;
        }

        private void AddToReadersDictionary(IBaseValueReader reader)
        {
            this.readers.Add(reader.AssociatedValueType, reader);
        }

        private IBaseValueReader GetReader(Type typeOfValueWeWantToRead)
        {
            Type readerTypeArgument = typeOfValueWeWantToRead;
            if ((typeOfValueWeWantToRead.IsValueType || typeOfValueWeWantToRead.IsPrimitive)
                && !typeOfValueWeWantToRead.Name.StartsWith("Nullable", StringComparison.OrdinalIgnoreCase))
            {
                // Readers for primitives or structs always handles the Nullable versions of those value types
                readerTypeArgument = typeof(Nullable<>).MakeGenericType(typeOfValueWeWantToRead);
            }

            if (this.readers.ContainsKey(readerTypeArgument))
            {
                return this.readers[readerTypeArgument];
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