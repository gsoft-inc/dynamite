using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ValueTypes;
using GSoft.Dynamite.ValueTypes.Readers;
using GSoft.Dynamite.ValueTypes.Writers;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// An entity mapping utility for SharePoint.
    /// </summary>
    public class SharePointEntityBinder : ISharePointEntityBinder
    {
        #region Fields

        private readonly IEntitySchemaFactory entitySchemaFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="SharePointEntityBinder"/>
        /// </summary>
        /// <param name="entitySchemaFactory">The entity schema building utility</param>
        public SharePointEntityBinder(IEntitySchemaFactory entitySchemaFactory)
        {
            this.entitySchemaFactory = entitySchemaFactory;
        }

        #endregion

        #region ISharePointEntityBinder Members

        /// <summary>
        /// Extracts the values from the entity to fill the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItem">The list item.</param>
        public void FromEntity<T>(T entity, SPListItem listItem)
        {
            var schema = this.entitySchemaFactory.GetSchema(typeof(T));
            var listItemFields = listItem.Fields;

            foreach (var binding in schema.PropertyConversionDetails.ToList().Where(x => x.BindingType == BindingType.Bidirectional || x.BindingType == BindingType.WriteOnly))
            {
                var valueFromEntity = binding.EntityProperty.GetValue(entity, null);
                IBaseValueWriter writer = binding.ValueWriter;
                
                // Create a MinimalFieldInfo<TValueType> to feed into the FieldValueInfo needed to
                // interact with IBaseValueWriter
                var minimalFieldInfoType = typeof(MinimalFieldInfo<>).MakeGenericType(writer.AssociatedValueType);
                string fieldInternalName = binding.ValueKey;
                SPField itemField = listItemFields.GetFieldByInternalName(fieldInternalName);
                var minimalFieldInfo = (BaseFieldInfo)Activator.CreateInstance(minimalFieldInfoType, new object[] { fieldInternalName, itemField.Id });
                var fieldValueInfo = new FieldValueInfo(minimalFieldInfo, valueFromEntity);

                // Update the list item through the IBaseValueWriter
                writer.WriteValueToListItem(listItem, fieldValueInfo);
            }
        }

        /// <summary>
        /// Creates an entity of the specified type and fills it using the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="listItem">The list item.</param>
        /// <returns>
        /// The newly created and filled entity.
        /// </returns>
        public T Get<T>(SPListItem listItem) where T : new()
        {
            var entity = new T();

            this.ToEntity(entity, listItem);

            return entity;
        }

        /// <summary>
        /// Creates an entity of the specified type and fills it using the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="listItemVersion">The list item version.</param>
        /// <returns>
        /// The newly created and filled entity.
        /// </returns>
        public T Get<T>(SPListItemVersion listItemVersion) where T : new()
        {
            var entity = new T();

            this.ToEntity(entity, listItemVersion);

            return entity;
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="listItems">
        /// The list items.
        /// </param>
        /// <typeparam name="T"> The type of object to return
        /// </typeparam>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public IList<T> Get<T>(SPListItemCollection listItems) where T : new()
        {
            var returnList = new List<T>();

            if (listItems.Count > 0)
            {
                // Using GetDataTable is great because it eagerly fetches all
                // the data of the SPListItemCollection. Without a GetDataTable
                // each step in the SPListItemCollection enumeration will trigger
                // a database call. If you are truly careless and forgot to specify
                // your SPQuery.ViewFields, each field value access on the item will
                // also trigger a database call.
                // Lessons: 
                // 1) always use ISharePointEntityBinder.Get<T>(SPListItemCollection)
                // because it eagerly fetches all the data
                // and 
                // 2) always specify SPQuery.ViewFields to avoid per-field-access
                // database calls.
                var table = listItems.GetDataTable();
                var rows = table.AsEnumerable();

                foreach (var dataRow in rows)
                {
                    returnList.Add(this.Get<T>(dataRow, listItems.Fields));
                }
            }

            return returnList;
        }

        /// <summary>
        /// Creates an entity of the specified type and fills it using the values.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="dataRow">The data row.</param>
        /// <param name="fieldCollection">The collection of field to get</param>
        /// <returns>
        /// The newly created and filled entity.
        /// </returns>
        public T Get<T>(DataRow dataRow, SPFieldCollection fieldCollection) where T : new()
        {
            var entity = new T();

            this.ToEntity(entity, dataRow, fieldCollection);

            return entity;
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItem">The list item.</param>
        public void ToEntity<T>(T entity, SPListItem listItem)
        {
            var schema = this.entitySchemaFactory.GetSchema(typeof(T));

            foreach (var binding in schema.PropertyConversionDetails.Where(x => x.BindingType == BindingType.Bidirectional || x.BindingType == BindingType.ReadOnly))
            {
                IBaseValueReader reader = binding.ValueReader;
                var value = reader.GetType()
                    .GetMethod("ReadValueFromListItem")
                    .Invoke(reader, new object[] { listItem, binding.ValueKey });

                binding.EntityProperty.SetValue(entity, value, null);
            }
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the entity.
        /// </typeparam>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="dataRow">
        /// The data Row.
        /// </param>
        /// <param name="fieldCollection">
        /// The field Collection.
        /// </param>
        public void ToEntity<T>(T entity, DataRow dataRow, SPFieldCollection fieldCollection)   // TODO: get rid of field collection here... only useful for Write purposes..
        {
            var schema = this.entitySchemaFactory.GetSchema(typeof(T));

            foreach (var binding in schema.PropertyConversionDetails.Where(x => x.BindingType == BindingType.Bidirectional || x.BindingType == BindingType.ReadOnly))
            {
                IBaseValueReader reader = binding.ValueReader;
                var value = reader.GetType()
                    .GetMethod("ReadValueFromCamlResultDataRow")
                    .Invoke(reader, new object[] { fieldCollection.Web, dataRow, binding.ValueKey });

                binding.EntityProperty.SetValue(entity, value, null);
            }
        }

        /// <summary>
        /// Fills the entity with values taken from the values collection.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="listItemVersion">The list item version.</param>
        public void ToEntity<T>(T entity, SPListItemVersion listItemVersion)
        {
            var schema = this.entitySchemaFactory.GetSchema(typeof(T));

            foreach (var binding in schema.PropertyConversionDetails.Where(x => x.BindingType == BindingType.Bidirectional || x.BindingType == BindingType.ReadOnly))
            {
                IBaseValueReader reader = binding.ValueReader;
                var value = reader.GetType()
                    .GetMethod("ReadValueFromListItemVersion")
                    .Invoke(reader, new object[] { listItemVersion, binding.ValueKey });

                binding.EntityProperty.SetValue(entity, value, null);
            }
        }

        #endregion
    }
}