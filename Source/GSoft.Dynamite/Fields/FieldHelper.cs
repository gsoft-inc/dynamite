using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.ValueTypes;
using GSoft.Dynamite.ValueTypes.Writers;
using Microsoft.Office.Server.Search.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Publishing.Fields;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Helper class for managing SP Fields.
    /// </summary>
    public class FieldHelper : IFieldHelper
    {
        private readonly ITaxonomyHelper taxonomyHelper;
        private readonly IFieldSchemaHelper fieldSchemaHelper;
        private readonly IFieldValueWriter fieldValueWriter;
        private readonly ILogger log;

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="taxonomyHelper">The taxonomy helper</param>
        /// <param name="fieldSchemaHelper">Field schema builder</param>
        /// <param name="fieldValueWriter">Field value wirter</param>
        /// <param name="log">Logging utility</param>
        public FieldHelper(ITaxonomyHelper taxonomyHelper, IFieldSchemaHelper fieldSchemaHelper, IFieldValueWriter fieldValueWriter, ILogger log)
        {
            this.taxonomyHelper = taxonomyHelper;
            this.fieldSchemaHelper = fieldSchemaHelper;
            this.fieldValueWriter = fieldValueWriter;
            this.log = log;
        }

        /// <summary>
        /// Ensure a field
        /// </summary>
        /// <param name="fieldCollection">The field collection</param>
        /// <param name="fieldInfo">The field info configuration</param>
        /// <returns>The internal name of the field</returns>
        public SPField EnsureField(SPFieldCollection fieldCollection, IFieldInfo fieldInfo)
        {
            SPList parentList = null;
            bool isListField = TryGetListFromFieldCollection(fieldCollection, out parentList);
            bool alreadyExistsAsSiteColumn = fieldCollection.Web.Site.RootWeb.Fields.TryGetFieldByStaticName(fieldInfo.InternalName) != null;

            if (isListField && !alreadyExistsAsSiteColumn)
            {
                // By convention, we enfore creation of site column before using that field on a list
                this.InnerEnsureField(fieldCollection.Web.Site.RootWeb.Fields, fieldInfo);
            }

            return this.InnerEnsureField(fieldCollection, fieldInfo);
        }

        /// <summary>
        /// Ensure a collection of fields
        /// </summary>
        /// <param name="fieldCollection">The field collection</param>
        /// <param name="fieldInfos">The field info configuration</param>
        /// <returns>The internal names of the field</returns>
        public IEnumerable<SPField> EnsureField(SPFieldCollection fieldCollection, ICollection<IFieldInfo> fieldInfos)
        {
            var createdFields = new List<SPField>();

            foreach (IFieldInfo fieldInfo in fieldInfos)
            {
                createdFields.Add(this.EnsureField(fieldCollection, fieldInfo));
            }

            return createdFields;
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "refetchedField", Justification = "Variable exists just to attempt to trigger ArgumentException to handle weird edge-case.")]
        private SPField InnerEnsureField(SPFieldCollection fieldCollection, IFieldInfo fieldInfo)
        {
            SPField field = null;
            
            if (fieldInfo.GetType().Name.StartsWith("MinimalFieldInfo", StringComparison.OrdinalIgnoreCase))
            {
                // Ensuring a MinimalFieldInfo from its SchemaXML is impossible since the MinimalFieldInfo object
                // doesn't hold enough information to completely describe the field metadata.
                // Instead, we have to re-use the site column and apply it to the list.
                var existingSiteColumn = fieldCollection.Web.Site.RootWeb.Fields.TryGetFieldByStaticName(fieldInfo.InternalName);

                if (existingSiteColumn == null)
                {
                    throw new NotSupportedException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Failed to ensure MinimalFieldInfo for field {0} because the pre-requisite Site Column doesn't exist.",
                            fieldInfo.InternalName));
                }

                SPList parentList = null;
                if (!TryGetListFromFieldCollection(fieldCollection, out parentList))
                {
                    throw new NotSupportedException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Failed to ensure MinimalFieldInfo for field {0}. A MinimalFieldInfo can only be used to ensure a Field on a List's SPFieldCollection, not to re-define an OOTB site column definition.",
                            fieldInfo.InternalName));
                }

                fieldCollection.Add(existingSiteColumn);

                field = fieldCollection[existingSiteColumn.Id];
            }
            else
            {
                // We have a fully-functional/fully-detailed IFieldInfo which should support a conversion to SchemaXML: go ahead and try to add the field.
                XElement xmlSchemaForField = this.fieldSchemaHelper.SchemaForField(fieldInfo);
                field = this.fieldSchemaHelper.EnsureFieldFromSchema(fieldCollection, xmlSchemaForField);
            }

            // In some cases, the returned field will not match the one we meant to create or ensure. For example,
            // we may have defined a fieldInfo with an InternalName that clashes with an already existing field.
            // In such a case, EnsureFieldFromSchema will return us the conflicting/already existing field (not 
            // the one we mean to ensure).
            if (field.Id == fieldInfo.Id && field.InternalName == fieldInfo.InternalName)
            {
                try 
                {
                    var refetchedField = fieldCollection[field.Id];
                }
                catch (ArgumentException)
                { 
                    // in a sneaky edge case, we're dealing with a sub-web's fieldCollection,
                    // and we actually ensured the column on the root web (instead of on the sub-web).
                    fieldCollection = fieldCollection.Web.Site.RootWeb.Fields;
                }

                // Set the field visibility
                this.UpdateFieldVisibility(field, fieldInfo);

                // Set miscellaneous properties
                this.UpdateFieldTypeSpecificProperties(fieldCollection, field, fieldInfo);

                // Tiny bit of ugly reflection here: we assume that all implementations of IFieldInfo will 
                // derive from FieldInfo<T>, which in turn lets us assume a DefaultValue property will always
                // be there for us to create our FieldValueInfo (which simply needs an untyped object as value).
                FieldValueInfo defaultValueFieldInfo = new FieldValueInfo(fieldInfo, fieldInfo.GetType().GetProperty("DefaultValue").GetValue(fieldInfo));
                this.fieldValueWriter.WriteValueToFieldDefault(fieldCollection, defaultValueFieldInfo);

                if (!string.IsNullOrEmpty(fieldInfo.DefaultFormula))
                {
                    if (!string.IsNullOrEmpty(field.DefaultValue))
                    {
                        // A default value was already specified, so setting a Formula makes no sense.
                        throw new InvalidOperationException("Failed to ensure field " + fieldInfo.InternalName + " in its entirety because both DefaultFormula and DefaultValue properties were specified. Please only set Formula OR DefaultValue, not both. Also don't forget to clean up the partially created field " + fieldInfo.InternalName + ".");
                    }

                    // Setting the DefaultFormula through the SchemaXML doesn't work,
                    // so let's force it here.
                    field.DefaultFormula = fieldInfo.DefaultFormula;
                }

                field.Update();
            }

            return field;
        }

        private void ApplyTaxonomyTermStoreMapping(SPFieldCollection fieldCollection, SPField field, TaxonomyFieldInfo taxonomyFieldInfo)
        {
            // Apply the term set mapping (taxonomy picker selection context) for the column
            if (taxonomyFieldInfo.TermStoreMapping != null)
            {
                SPList fieldCollectionParentList = null;
                if (TryGetListFromFieldCollection(fieldCollection, out fieldCollectionParentList))
                {
                    this.taxonomyHelper.AssignTermStoreMappingToField(
                        fieldCollectionParentList.ParentWeb.Site,
                        field,
                        taxonomyFieldInfo.TermStoreMapping);
                }
                else
                {
                    this.taxonomyHelper.AssignTermStoreMappingToField(
                        fieldCollection.Web.Site,
                        field,
                        taxonomyFieldInfo.TermStoreMapping);
                }
            }
            else
            {
                // the term store mapping is null, we should make sure the field is unmapped
                ClearTermStoreMapping(fieldCollection, taxonomyFieldInfo);
            }
        }

        private static void ClearTermStoreMapping(SPFieldCollection fieldCollection, IFieldInfo taxonomyFieldInfo)
        {
            var taxoField = (TaxonomyField)fieldCollection[taxonomyFieldInfo.Id];
            taxoField.AnchorId = Guid.Empty;
            taxoField.TermSetId = Guid.Empty;
            taxoField.SspId = Guid.Empty;
            taxoField.Update();
        }

        private void ApplyTaxonomyMultiTermStoreMapping(SPFieldCollection fieldCollection, SPField field, TaxonomyMultiFieldInfo taxonomyMultiFieldInfo)
        {
            // Apply the term set mapping (taxonomy picker selection context) for the column
            if (taxonomyMultiFieldInfo.TermStoreMapping != null)
            {
                SPList fieldCollectionParentList = null;
                if (TryGetListFromFieldCollection(fieldCollection, out fieldCollectionParentList))
                {
                    this.taxonomyHelper.AssignTermStoreMappingToField(
                        fieldCollectionParentList.ParentWeb.Site, 
                        field,
                        taxonomyMultiFieldInfo.TermStoreMapping);
                }
                else
                {
                    this.taxonomyHelper.AssignTermStoreMappingToField(
                        fieldCollection.Web.Site, 
                        field,
                        taxonomyMultiFieldInfo.TermStoreMapping);
                }
            }
            else
            {
                // the term store mapping is null, we should make sure the field is unmapped
                ClearTermStoreMapping(fieldCollection, taxonomyMultiFieldInfo);
            }
        }

        private void UpdateFieldVisibility(SPField field, IFieldInfo fieldInfo)
        {
            field.ShowInListSettings = !fieldInfo.IsHiddenInListSettings;
            field.ShowInDisplayForm = !fieldInfo.IsHiddenInDisplayForm;
            field.ShowInEditForm = !fieldInfo.IsHiddenInEditForm;
            field.ShowInNewForm = !fieldInfo.IsHiddenInNewForm;

            // Apply Hidden here again (even through it's already set through the schema XML),
            // because otherwise updates to Hidden will not work.
            if (!field.CanToggleHidden)
            {
                bool before = field.Hidden;

                // Use reflection to get around the CanToggleHidden constraint. Keep in mind that 
                // there may be some unintended consequenced from hiding/showing and previously
                // shown/hidden field (hence the logged warning).
                Type type = field.GetType();
                MethodInfo mi = type.GetMethod("SetFieldBoolValue", BindingFlags.NonPublic | BindingFlags.Instance); 
                mi.Invoke(field, new object[] { "CanToggleHidden", true });
                field.Hidden = fieldInfo.IsHidden;
                mi.Invoke(field, new object[] { "CanToggleHidden", false });

                this.log.Warn(
                    string.Format(
                        CultureInfo.InvariantCulture, 
                        "FieldHelper.EnsureField - Forced field (id={0}, name={1}) from Hidden={2} to Hidden={3} even though it should've been impossible because CanToggleHidden=false.",
                        field.Id,
                        field.InternalName,
                        before,
                        fieldInfo.IsHidden));
            }
            else
            {
                // No need to use reflection before being able to set the Hidden property
                field.Hidden = fieldInfo.IsHidden;
            }   
        }

        private void UpdateFieldTypeSpecificProperties(SPFieldCollection parentFieldCollection, SPField field, IFieldInfo fieldInfo)
        {
            // Set field properties
            var asTaxonomyFieldInfo = fieldInfo as TaxonomyFieldInfo;
            var asTaxonomyMultiFieldInfo = fieldInfo as TaxonomyMultiFieldInfo;
            var asCurrencyFieldInfo = fieldInfo as CurrencyFieldInfo;

            if (fieldInfo is TextFieldInfo
                || fieldInfo is NoteFieldInfo
                || fieldInfo is HtmlFieldInfo)
            {
                field.EnforceUniqueValues = fieldInfo.EnforceUniqueValues;
            }
            else if (asTaxonomyFieldInfo != null)
            {
                var taxonomyField = field as TaxonomyField;
                if (taxonomyField != null)
                {
                    taxonomyField.CreateValuesInEditForm = asTaxonomyFieldInfo.CreateValuesInEditForm;
                    taxonomyField.Open = asTaxonomyFieldInfo.CreateValuesInEditForm;                 
                    taxonomyField.IsPathRendered = asTaxonomyFieldInfo.IsPathRendered;

                    field = taxonomyField;

                    // this call will take care of calling Update() on field
                    this.ApplyTaxonomyTermStoreMapping(parentFieldCollection, field, asTaxonomyFieldInfo);
                }
            }
            else if (asTaxonomyMultiFieldInfo != null)
            {
                var taxonomyField = field as TaxonomyField;
                if (taxonomyField != null)
                {
                    taxonomyField.CreateValuesInEditForm = asTaxonomyMultiFieldInfo.CreateValuesInEditForm;
                    taxonomyField.Open = asTaxonomyMultiFieldInfo.CreateValuesInEditForm;
                    taxonomyField.IsPathRendered = asTaxonomyMultiFieldInfo.IsPathRendered;

                    field = taxonomyField;

                    // this call will take care of calling Update() on field
                    this.ApplyTaxonomyMultiTermStoreMapping(parentFieldCollection, field, asTaxonomyMultiFieldInfo);
                }              
            }
            else if (asCurrencyFieldInfo != null)
            {
                // gotta set locale here because it doesn't get persisted through schema XML
                ((SPFieldCurrency)field).CurrencyLocaleId = asCurrencyFieldInfo.LocaleId;        
            }
        }

        private static bool TryGetListFromFieldCollection(SPFieldCollection collection, out SPList list)
        {
            if (collection.Count > 0)
            {
                SPField first = collection[0];
                if (first != null)
                {
                    if (first.ParentList != null)
                    {
                        list = first.ParentList;
                        return true;
                    }
                }
            }

            list = null;
            return false;
        }
    }
}
