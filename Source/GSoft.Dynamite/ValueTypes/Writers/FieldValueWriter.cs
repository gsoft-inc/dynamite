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
        /// <param name="taxonomyValueWriter">The taxonomy value writer.</param>
        /// <param name="taxonomyValueCollectionWriter">The taxonomy multi value writer.</param>
        /// <param name="lookupValueWriter">The lookup value writer.</param>
        /// <param name="principalValueWriter">The principal value writer.</param>
        /// <param name="userValueWriter">The user value writer.</param>
        /// <param name="urlValueWriter">The URL value writer.</param>
        /// <param name="imageValueWriter">The image value writer.</param>
        public FieldValueWriter(
            StringValueWriter stringValueWriter,
            TaxonomyFullValueWriter taxonomyValueWriter,
            TaxonomyFullValueCollectionWriter taxonomyValueCollectionWriter,
            LookupValueWriter lookupValueWriter,
            PrincipalValueWriter principalValueWriter,
            UserValueWriter userValueWriter,
            UrlValueWriter urlValueWriter,
            ImageValueWriter imageValueWriter)
        {
            this.AddToWritersDictionary(stringValueWriter);
            this.AddToWritersDictionary(taxonomyValueWriter);
            this.AddToWritersDictionary(taxonomyValueCollectionWriter);
            this.AddToWritersDictionary(lookupValueWriter);
            this.AddToWritersDictionary(principalValueWriter);
            this.AddToWritersDictionary(urlValueWriter);
            this.AddToWritersDictionary(imageValueWriter);
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

        public void WriteValuesToFieldDefaults(SPFieldCollection field, IList<FieldValueInfo> fieldValueInfos)
        {
            throw new NotImplementedException();
        }

        public void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            if (parentFieldCollection == null)
            {
                throw new ArgumentNullException("parentFieldCollection");
            }

            if (fieldValueInfo == null || fieldValueInfo.FieldInfo == null)
            {
                throw new ArgumentNullException("fieldValueInfo");
            }

            IBaseValueWriter valueWriter = this.GetWriter(fieldValueInfo);

            valueWriter.WriteValueToFieldDefault(parentFieldCollection, fieldValueInfo);
        }

        public void WriteValuesToFolderDefaults(SPFolder folder, IList<FieldValueInfo> fieldValueInfos)
        {
            throw new NotImplementedException();
        }

        public void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            throw new NotImplementedException();
        }

        private void AddToWritersDictionary(IBaseValueWriter writer)
        {
            this.writers.Add(writer.AssociatedValueType, writer);
        }

        private IBaseValueWriter GetWriter(FieldValueInfo fieldValueInfo)
        {
            var associatedValueType = fieldValueInfo.FieldInfo.AssociatedValueType;
            IBaseValueWriter valueWriter = null;

            if (writers.ContainsKey(associatedValueType))
            {
                valueWriter = writers[associatedValueType];
            }
            else
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    "WriteValueToListItem - Failed to find a value writer for your FieldInfo's AssociatedValueType (field={0}, valueType={1})",
                    fieldValueInfo.FieldInfo.InternalName,
                    associatedValueType.ToString()));
            }

            return valueWriter;
        }
    }
}