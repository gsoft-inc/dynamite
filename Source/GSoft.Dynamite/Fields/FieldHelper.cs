using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Taxonomy;
using Microsoft.Office.Server.Search.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
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
        private readonly ILogger log;

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="taxonomyHelper">The taxonomy helper</param>
        /// <param name="fieldSchemaHelper">Field schema builder</param>
        /// <param name="log">Logging utility</param>
        public FieldHelper(ITaxonomyHelper taxonomyHelper, IFieldSchemaHelper fieldSchemaHelper, ILogger log)
        {
            this.taxonomyHelper = taxonomyHelper;
            this.fieldSchemaHelper = fieldSchemaHelper;
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

        private SPField InnerEnsureField(SPFieldCollection fieldCollection, IFieldInfo fieldInfo)
        {
            SPField field = this.fieldSchemaHelper.EnsureFieldFromSchema(fieldCollection, this.fieldSchemaHelper.SchemaForField(fieldInfo));

            // Set the field visibility
            field = this.UpdateFieldVisibility(field, fieldInfo);

            // Set miscellaneous proeprties
            field = SetFieldMiscProperties(field, fieldInfo);

            // Set default value if any, ensure other FieldType-specific properties
            this.ApplyFieldTypeSpecificValuesAndUpdate(fieldCollection, field, fieldInfo);

            // Refetch latest version of field, because right now the SPField object
            // doesn't hold the TermStore mapping information (see how TaxonomyHelper.AssignTermSetToColumn
            // always re-fetches the SPField itself... TODO: this should be reworked)
            field = this.RefetchFieldToGetLatestVersionAndAvoidUpdateConflicts(fieldCollection, fieldInfo);

            return field;
        }

        private SPField RefetchFieldToGetLatestVersionAndAvoidUpdateConflicts(SPFieldCollection fieldCollection, IFieldInfo fieldInfo)
        {
            SPField field = null;

            if (fieldCollection.List != null)
            {
                fieldCollection = fieldCollection.List.Fields;
            }
            else
            {
                fieldCollection = fieldCollection.Web.Fields;
            }

            try
            {
                field = fieldCollection[fieldInfo.Id];
            }
            catch (ArgumentException)
            {
                field = fieldCollection.Cast<SPField>().FirstOrDefault(f => f.InternalName == fieldInfo.InternalName);

                if (field == null)
                {
                    try
                    {
                        // maybe we're in the sub-web scenario, where we sneakily created the site column
                        // on the root web instead
                        field = fieldCollection.Web.Site.RootWeb.Fields[fieldInfo.Id];
                    }
                    catch (ArgumentException)
                    {
                        field = fieldCollection.Web.Site.RootWeb.Fields.Cast<SPField>().FirstOrDefault(f => f.InternalName == fieldInfo.InternalName);
                    }
                }
            }

            return field;
        }

        private void ApplyFieldTypeSpecificValuesAndUpdate(SPFieldCollection fieldCollection, SPField field, IFieldInfo fieldInfo)
        {
            var asTaxonomyFieldInfo = fieldInfo as TaxonomyFieldInfo;
            var asTaxonomyMultiFieldInfo = fieldInfo as TaxonomyMultiFieldInfo;

            if (fieldInfo is NumberFieldInfo)
            {
                FieldInfo<double?> doubleBasedField = fieldInfo as FieldInfo<double?>;

                if (doubleBasedField.DefaultValue.HasValue)
                {
                    field.DefaultValue = doubleBasedField.DefaultValue.ToString();
                }

                field.Update();
            }
            else if (fieldInfo is TextFieldInfo
                || fieldInfo is NoteFieldInfo
                || fieldInfo is HtmlFieldInfo)
            {
                FieldInfo<string> stringBasedField = fieldInfo as FieldInfo<string>;

                if (!string.IsNullOrEmpty(stringBasedField.DefaultValue))
                {
                    field.DefaultValue = stringBasedField.DefaultValue;
                }

                // don't forget to persist changes
                field.Update();
            }
            else if (asTaxonomyFieldInfo != null)
            {
                // this call will take care of calling Update() on field
                this.ApplyTaxonomyFieldValues(fieldCollection, field, asTaxonomyFieldInfo);
            }
            else if (asTaxonomyMultiFieldInfo != null)
            {
                // this call will take care of calling Update() on field
                this.ApplyTaxonomyMultiFieldValues(fieldCollection, field, asTaxonomyMultiFieldInfo);
            }
            else if (fieldInfo is DateTimeFieldInfo)
            {
                FieldInfo<DateTime?> doubleBasedField = fieldInfo as FieldInfo<DateTime?>;

                if (doubleBasedField.DefaultValue.HasValue)
                {
                    field.DefaultValue = SPUtility.CreateISO8601DateTimeFromSystemDateTime(doubleBasedField.DefaultValue.Value);
                }

                field.Update();
            }
            else
            {
                // Some preceding changed be need to be persisted
                field.Update();
            }

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
                field.Update();
            }

            // TODO: support other field types (DateTimeFieldInfo, UrlFieldInfo, ImageFieldInfo, etc.)
        }

        private void ApplyTaxonomyFieldValues(SPFieldCollection fieldCollection, SPField field, TaxonomyFieldInfo taxonomyFieldInfo)
        {
            // Apply the term set mapping (taxonomy picker selection context) for the column
            if (taxonomyFieldInfo.TermStoreMapping != null)
            {
                this.ApplyTermStoreMapping(fieldCollection, taxonomyFieldInfo, taxonomyFieldInfo.TermStoreMapping);
            }
            else
            {
                // the term store mapping is null, we should make sure the field is unmapped
                ClearTermStoreMapping(fieldCollection, taxonomyFieldInfo);
            }

            // Set the default value for the field
            if (taxonomyFieldInfo.DefaultValue != null)
            {
                // If term store mapping was applied, the field instance is now stale (the field definition got updated 
                // through another instance of the same SPField). We need to re-fetch the field to get the very latest.
                field = this.RefetchFieldToGetLatestVersionAndAvoidUpdateConflicts(fieldCollection, taxonomyFieldInfo);
                this.taxonomyHelper.SetDefaultTaxonomyFieldValue(fieldCollection.Web, field as TaxonomyField, taxonomyFieldInfo.DefaultValue);
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

        private void ApplyTaxonomyMultiFieldValues(SPFieldCollection fieldCollection, SPField field, TaxonomyMultiFieldInfo taxonomyMultiFieldInfo)
        {
            // Apply the term set mapping (taxonomy picker selection context) for the column
            if (taxonomyMultiFieldInfo.TermStoreMapping != null)
            {
                this.ApplyTermStoreMapping(fieldCollection, taxonomyMultiFieldInfo, taxonomyMultiFieldInfo.TermStoreMapping);
            }
            else
            {
                // the term store mapping is null, we should make sure the field is unmapped
                ClearTermStoreMapping(fieldCollection, taxonomyMultiFieldInfo);
            }

            // Set the default value for the field
            if (taxonomyMultiFieldInfo.DefaultValue != null)
            {
                // If term store mapping was applied, the field instance is now stale (the field definition got updated 
                // through another instance of the same SPField). We need to re-fetch the field to get the very latest.
                field = this.RefetchFieldToGetLatestVersionAndAvoidUpdateConflicts(fieldCollection, taxonomyMultiFieldInfo);
                this.taxonomyHelper.SetDefaultTaxonomyFieldMultiValue(fieldCollection.Web, field as TaxonomyField, taxonomyMultiFieldInfo.DefaultValue);
            }
        }

        private void ApplyTermStoreMapping(SPFieldCollection fieldCollection, IFieldInfo fieldInfo, TaxonomyContext taxonomyMappingContext)
        {
            string termSubsetName = string.Empty;
            if (taxonomyMappingContext.TermSubset != null)
            {
                termSubsetName = taxonomyMappingContext.TermSubset.Label;
            }

            // Metadata mapping configuration
            SPList parentList = null;

            // Try to see if we're playing with a List-field collection or a Web-field collection context
            if (TryGetListFromFieldCollection(fieldCollection, out parentList))
            {
                // Ensure this term set mapping on the List-specific field only
                if (taxonomyMappingContext.Group != null)
                {
                    // Term set mapping on a global farm-wide term set.
                    this.taxonomyHelper.AssignTermSetToListColumn(
                        parentList,
                        fieldInfo.Id,
                        taxonomyMappingContext.Group.Name,
                        taxonomyMappingContext.TermSet.Labels[fieldCollection.Web.UICulture],
                        termSubsetName);
                }
                else
                {
                    // Term set mapping on a local site-collection-specific term set.
                    this.taxonomyHelper.AssignTermSetToListColumn(
                        parentList,
                        fieldInfo.Id,
                        taxonomyMappingContext.TermSet.Labels[fieldCollection.Web.UICulture],
                        termSubsetName);
                }
            }
            else 
            {
                // Ensure this field accross the web (i.e. site column + all usages of the field accross all the web's lists)
                if (taxonomyMappingContext.Group != null)
                {
                    // Term set mapping on a global farm-wide term set.
                    this.taxonomyHelper.AssignTermSetToSiteColumn(
                        fieldCollection.Web,
                        fieldInfo.Id,
                        taxonomyMappingContext.Group.Name,
                        taxonomyMappingContext.TermSet.Labels[fieldCollection.Web.UICulture],
                        termSubsetName);
                }
                else
                {
                    // Term set mapping on a local site-collection-specific term set.
                    this.taxonomyHelper.AssignTermSetToSiteColumn(
                        fieldCollection.Web,
                        fieldInfo.Id,
                        taxonomyMappingContext.TermSet.Labels[fieldCollection.Web.UICulture],
                        termSubsetName);
                }
            }
        }

        private SPField UpdateFieldVisibility(SPField field, IFieldInfo fieldInfo)
        {
            if (field != null)
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

                // TODO: call Update() from outside, maybe? not sure how we should be handling our commits...
                field.Update();     
            }

            return field;
        }

        private static SPField SetFieldMiscProperties(SPField field, IFieldInfo fieldInfo)
        {
            // Set field properties
            var asTaxonomyFieldInfo = fieldInfo as TaxonomyFieldInfo;
            var asTaxonomyMultiFieldInfo = fieldInfo as TaxonomyMultiFieldInfo;

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
                }              
            }

            field.Update();

            return field;
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
