using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Has the responsibility to write values to a SharePoint list item.
    /// </summary>
    public class FieldValueWriter : IFieldValueWriter
    {
        private readonly IDictionary<Type, IBaseValueWriter> writers = new Dictionary<Type, IBaseValueWriter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValueWriter"/> class.
        /// </summary>
        /// <param name="stringValueWriter">The base value writer.</param>
        /// <param name="boolValueWriter">The boolean value writer.</param>
        /// <param name="doubleValueWriter">The double value writer.</param>
        /// <param name="dateTimeValueWriter">The DateTime value writer.</param>
        /// <param name="guidValueWriter">The Guid value writer.</param>
        /// <param name="taxonomyValueWriter">The taxonomy value writer.</param>
        /// <param name="taxonomyValueCollectionWriter">The taxonomy multi value writer.</param>
        /// <param name="lookupValueWriter">The lookup value writer.</param>
        /// <param name="lookupValueCollectionWriter">The lookup value collection writer</param>
        /// <param name="principalValueWriter">The principal value writer.</param>
        /// <param name="userValueWriter">The user value writer.</param>
        /// <param name="userValueCollectionWriter">The user value collection writer</param>
        /// <param name="urlValueWriter">The URL value writer.</param>
        /// <param name="imageValueWriter">The image value writer.</param>
        /// <param name="mediaValueWriter">The media value writer.</param>
        public FieldValueWriter(
            StringValueWriter stringValueWriter,
            BooleanValueWriter boolValueWriter,
            DoubleValueWriter doubleValueWriter,
            DateTimeValueWriter dateTimeValueWriter,
            GuidValueWriter guidValueWriter,
            TaxonomyFullValueWriter taxonomyValueWriter,
            TaxonomyFullValueCollectionWriter taxonomyValueCollectionWriter,
            LookupValueWriter lookupValueWriter,
            LookupValueCollectionWriter lookupValueCollectionWriter,
            PrincipalValueWriter principalValueWriter,
            UserValueWriter userValueWriter,
            UserValueCollectionWriter userValueCollectionWriter,
            UrlValueWriter urlValueWriter,
            ImageValueWriter imageValueWriter,
            MediaValueWriter mediaValueWriter)
        {
            this.AddToWritersDictionary(stringValueWriter);
            this.AddToWritersDictionary(boolValueWriter);
            this.AddToWritersDictionary(doubleValueWriter);
            this.AddToWritersDictionary(dateTimeValueWriter);
            this.AddToWritersDictionary(guidValueWriter);
            this.AddToWritersDictionary(taxonomyValueWriter);
            this.AddToWritersDictionary(taxonomyValueCollectionWriter);
            this.AddToWritersDictionary(lookupValueWriter);
            this.AddToWritersDictionary(lookupValueCollectionWriter);
            this.AddToWritersDictionary(principalValueWriter);
            this.AddToWritersDictionary(userValueWriter);
            this.AddToWritersDictionary(userValueCollectionWriter);
            this.AddToWritersDictionary(urlValueWriter);
            this.AddToWritersDictionary(imageValueWriter);
            this.AddToWritersDictionary(mediaValueWriter);
        }

        /// <summary>
        /// Updates the given SPListItem with the values passed.
        /// This method does not call Update or SystemUpdate.
        /// </summary>
        /// <param name="item">The SharePoint list item to update.</param>
        /// <param name="fieldValueInfos">The value information to be updated in the SPListItem.</param>
        public void WriteValuesToListItem(SPListItem item, IList<FieldValueInfo> fieldValueInfos)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (fieldValueInfos == null)
            {
                throw new ArgumentNullException("fieldValueInfos");
            }

            foreach (var fieldValue in fieldValueInfos)
            {
                this.WriteValueToListItem(item, fieldValue);
            }
        }
        
        /// <summary>
        /// Updates the given SPListItem with the value passed.
        /// This method does not call Update or SystemUpdate.
        /// </summary>
        /// <param name="item">The SharePoint list item to update.</param>
        /// <param name="fieldValueInfo">The value information to be updated in the SPListItem.</param>
        public void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (fieldValueInfo == null || fieldValueInfo.FieldInfo == null)
            {
                throw new ArgumentNullException("fieldValueInfo");
            }

            IBaseValueWriter valueWriter = this.GetWriter(fieldValueInfo);

            valueWriter.WriteValueToListItem(item, fieldValueInfo);
        }
        
        /// <summary>
        /// Updates the specified SPField definitions with new DefaultValues
        /// </summary>
        /// <param name="parentFieldCollection">The SharePoint field collection containing the fields to update.</param>
        /// <param name="defaultFieldValueInfos">The default values to be applied as the SPFields' new defaults.</param>
        public void WriteValuesToFieldDefaults(SPFieldCollection parentFieldCollection, IList<FieldValueInfo> defaultFieldValueInfos)
        {
            if (parentFieldCollection == null)
            {
                throw new ArgumentNullException("parentFieldCollection");
            }

            if (defaultFieldValueInfos == null)
            {
                throw new ArgumentNullException("defaultFieldValueInfos");
            }

            foreach (var fieldValue in defaultFieldValueInfos)
            {
                this.WriteValueToFieldDefault(parentFieldCollection, fieldValue);
            }
        }
        
        /// <summary>
        /// Updates the specified SPField definition with new DefaultValue
        /// </summary>
        /// <param name="parentFieldCollection">The SharePoint field collection containing the field to update.</param>
        /// <param name="defaultFieldValueInfo">The default value to be applied as the SPField' new default.</param>
        public void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo defaultFieldValueInfo)
        {
            if (parentFieldCollection == null)
            {
                throw new ArgumentNullException("parentFieldCollection");
            }

            if (defaultFieldValueInfo == null || defaultFieldValueInfo.FieldInfo == null)
            {
                throw new ArgumentNullException("defaultFieldValueInfo");
            }

            IBaseValueWriter valueWriter = this.GetWriter(defaultFieldValueInfo);

            valueWriter.WriteValueToFieldDefault(parentFieldCollection, defaultFieldValueInfo);
        }
        
        /// <summary>
        /// Updates the specified SPFolder with new default field values
        /// </summary>
        /// <param name="folder">The SharePoint folder for which we want to update the metadata defaults.</param>
        /// <param name="defaultFieldValueInfos">The default values to be applied to items created within that folder.</param>
        public void WriteValuesToFolderDefaults(SPFolder folder, IList<FieldValueInfo> defaultFieldValueInfos)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            if (defaultFieldValueInfos == null)
            {
                throw new ArgumentNullException("defaultFieldValueInfos");
            }

            foreach (var fieldValue in defaultFieldValueInfos)
            {
                this.WriteValuesToFolderDefault(folder, fieldValue);
            }
        }
    
        /// <summary>
        /// Updates the specified SPFolder with new default field value
        /// </summary>
        /// <param name="folder">The SharePoint folder for which we want to update the metadata defaults.</param>
        /// <param name="defaultFieldValueInfo">The default value to be applied to items created within that folder.</param>
        public void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo defaultFieldValueInfo)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            if (defaultFieldValueInfo == null || defaultFieldValueInfo.FieldInfo == null)
            {
                throw new ArgumentNullException("defaultFieldValueInfo");
            }

            IBaseValueWriter valueWriter = this.GetWriter(defaultFieldValueInfo);

            valueWriter.WriteValueToFolderDefault(folder, defaultFieldValueInfo);
        }

        private void AddToWritersDictionary(IBaseValueWriter writer)
        {
            this.writers.Add(writer.AssociatedValueType, writer);
        }

        private IBaseValueWriter GetWriter(FieldValueInfo fieldValueInfo)
        {
            var associatedValueType = fieldValueInfo.FieldInfo.AssociatedValueType;
            IBaseValueWriter valueWriter = null;

            if (this.writers.ContainsKey(associatedValueType))
            {
                valueWriter = this.writers[associatedValueType];
            }
            else
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Failed to find a value writer for your FieldInfo's AssociatedValueType (field={0}, valueType={1})",
                    fieldValueInfo.FieldInfo.InternalName,
                    associatedValueType.ToString()));
            }

            return valueWriter;
        }
    }
}