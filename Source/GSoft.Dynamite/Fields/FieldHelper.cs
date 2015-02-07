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

        /// <summary>
        /// This ugly method is meant solely as a workaround: the TaxonomyHelper.AssignTermSetToSite/ListColumn forces
        /// us to re-fetch both the field and its parent fieldCollection if we want to keep updating them (since TaxonomyHelper
        /// it-self re-fetched both and updated them, rendering our current SPField and SPFieldCollection instance "stale" - i.e.
        /// not updated to the latest changes, and headed for a "Concurrent change"-exception at the next .Update() call).
        /// </summary>
        /// <param name="inputFieldCollection">The parent field collection of the field we want to refetch</param>
        /// <param name="fieldInfo">The metadata of the field we want to refetch from its parent Web or List</param>
        /// <param name="outputFieldCollection">The "refreshed" parent SPFieldCollection</param>
        /// <returns>The "refreshed" SPField instance</returns>
        private static SPField RefetchFieldToGetLatestVersionAndAvoidUpdateConflicts(
            SPFieldCollection inputFieldCollection, 
            IFieldInfo fieldInfo, 
            out SPFieldCollection outputFieldCollection)
        {
            SPField field = null;

            if (inputFieldCollection.List != null)
            {
                outputFieldCollection = inputFieldCollection.List.Fields;
            }
            else
            {
                outputFieldCollection = inputFieldCollection.Web.Fields;
            }

            try
            {
                field = outputFieldCollection[fieldInfo.Id];
            }
            catch (ArgumentException)
            {
                field = outputFieldCollection.Cast<SPField>().FirstOrDefault(f => f.InternalName == fieldInfo.InternalName);

                if (field == null)
                {
                    // TODO: oh my god nested try-catch blocks... 
                    try
                    {
                        // maybe we're in the sub-web scenario, where we sneakily created the site column
                        // on the root web instead
                        field = outputFieldCollection.Web.Site.RootWeb.Fields[fieldInfo.Id];
                    }
                    catch (ArgumentException)
                    {
                        field = outputFieldCollection.Web.Site.RootWeb.Fields.Cast<SPField>().FirstOrDefault(f => f.InternalName == fieldInfo.InternalName);
                    }
                }
            }

            return field;
        }

        private SPField InnerEnsureField(SPFieldCollection fieldCollection, IFieldInfo fieldInfo)
        {
            SPField field = this.fieldSchemaHelper.EnsureFieldFromSchema(fieldCollection, this.fieldSchemaHelper.SchemaForField(fieldInfo));

            // Set the field visibility
            this.UpdateFieldVisibility(field, fieldInfo);

            // Set miscellaneous proeprties
            SetFieldMiscProperties(field, fieldInfo);

            
            // Set default value if any, ensure other FieldType-specific properties
            this.ApplyFieldTypeSpecificValuesAndUpdate(fieldCollection, field, fieldInfo);

            // Refetch latest version of field, because right now the SPField object
            // doesn't hold the TermStore mapping information (see how TaxonomyHelper.AssignTermSetToColumn
            // always re-fetches the SPField itself... TODO: this should be reworked)
            //field = RefetchFieldToGetLatestVersionAndAvoidUpdateConflicts(fieldCollection, fieldInfo);


            return field;
        }

        // TODO: consolidate this DefaultValue setter logic with the normal setter logic in Binding.Writer utilities
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Currency and Number field handling should stay separate, even though they both can be cast to FieldInfo<double?>")]
        private void ApplyFieldTypeSpecificValuesAndUpdate(SPFieldCollection fieldCollection, SPField field, IFieldInfo fieldInfo)
        {
            var asTaxonomyFieldInfo = fieldInfo as TaxonomyFieldInfo;
            var asTaxonomyMultiFieldInfo = fieldInfo as TaxonomyMultiFieldInfo;

            //if (fieldInfo is NumberFieldInfo)
            //{
            //    FieldInfo<double?> doubleBasedField = fieldInfo as FieldInfo<double?>;

            //    if (doubleBasedField.DefaultValue.HasValue)
            //    {
            //        field.DefaultValue = doubleBasedField.DefaultValue.ToString();
            //    }

            //    field.Update();
            //}
            //else if (fieldInfo is CurrencyFieldInfo)
            //{
            //    FieldInfo<double?> doubleBasedField = fieldInfo as FieldInfo<double?>;

            //    if (doubleBasedField.DefaultValue.HasValue)
            //    {
            //        field.DefaultValue = doubleBasedField.DefaultValue.ToString();
            //    }

            //    ((SPFieldCurrency)field).CurrencyLocaleId = ((CurrencyFieldInfo)fieldInfo).LocaleId;        // gotta set locale here because it doesn't get persisted through schema XML

            //    field.Update();
            //}
            //else if (fieldInfo is BooleanFieldInfo)
            //{
            //    FieldInfo<bool?> booleanBasedField = fieldInfo as FieldInfo<bool?>;

            //    if (booleanBasedField.DefaultValue.HasValue)
            //    {
            //        field.DefaultValue = booleanBasedField.DefaultValue.ToString();

            //        this.log.Warn(
            //            "Default value ({0}) set on field {1} with type Boolean. SharePoint does not support default values on Boolean fields. "
            //            + "Only list items created programmatically will get the default value properly set. Setting a Boolean-field default value will not be "
            //            + "respected in your lists' NewForm.aspx item creation form.",
            //            field.DefaultValue,
            //            field.InternalName);
            //    }

            //    field.Update();
            //}
            //else if (fieldInfo is GuidFieldInfo)
            //{
            //    FieldInfo<Guid?> guidBasedField = fieldInfo as FieldInfo<Guid?>;

            //    if (guidBasedField.DefaultValue.HasValue)
            //    {
            //        field.DefaultValue = guidBasedField.DefaultValue.Value.ToString();
            //    }

            //    field.Update();
            //}
            //else if (fieldInfo is TextFieldInfo
            //    || fieldInfo is NoteFieldInfo
            //    || fieldInfo is HtmlFieldInfo)
            //{
            //    FieldInfo<string> stringBasedField = fieldInfo as FieldInfo<string>;

            //    if (!string.IsNullOrEmpty(stringBasedField.DefaultValue))
            //    {
            //        field.DefaultValue = stringBasedField.DefaultValue;
            //    }

            //    // don't forget to persist changes
            //    field.Update();
            //}
            //else if (fieldInfo is ImageFieldInfo)
            //{
            //    FieldInfo<ImageValue> imageBasedField = fieldInfo as FieldInfo<ImageValue>;

            //    if (imageBasedField.DefaultValue != null)
            //    {
            //        var imageValue = imageBasedField.DefaultValue;
            //        var fieldImageValue = new ImageFieldValue()
            //        {
            //            Alignment = imageValue.Alignment,
            //            AlternateText = imageValue.AlternateText,
            //            BorderWidth = imageValue.BorderWidth,
            //            Height = imageValue.Height,
            //            HorizontalSpacing = imageValue.HorizontalSpacing,
            //            Hyperlink = imageValue.Hyperlink,
            //            ImageUrl = imageValue.ImageUrl,
            //            OpenHyperlinkInNewWindow = imageValue.OpenHyperlinkInNewWindow,
            //            VerticalSpacing = imageValue.VerticalSpacing,
            //            Width = imageValue.Width,
            //        };

            //        field.DefaultValue = fieldImageValue.ToString();
            //    }

            //    // don't forget to persist changes
            //    field.Update();
            //}
            //else if (fieldInfo is UrlFieldInfo)
            //{
            //    FieldInfo<UrlValue> urlBasedField = fieldInfo as FieldInfo<UrlValue>;

            //    if (urlBasedField.DefaultValue != null)
            //    {
            //        var urlValue = urlBasedField.DefaultValue;

            //        var newUrlValue = new SPFieldUrlValue { Url = urlValue.Url, Description = urlValue.Description };

            //        // Avoid setting the Description as well, otherwise all
            //        // new items created with that field will have both the URL
            //        // and Description in their URL and Description fields (weird lack
            //        // of OOTB support for Url default values).
            //        field.DefaultValue = newUrlValue.Url;

            //        if (!string.IsNullOrEmpty(urlValue.Description))
            //        {
            //            this.log.Warn("Skipped initialization of Description property (val={0}) on Url field value (urlval={1}). A SPFieldUrlValue cannot support more than a simple URL string as default value.", urlValue.Description, urlValue.Url);
            //        }
            //    }

            //    // don't forget to persist changes
            //    field.Update();
            //}
            if (asTaxonomyFieldInfo != null)
            {
                // this call will take care of calling Update() on field
                this.ApplyTaxonomyFieldValues(fieldCollection, field, asTaxonomyFieldInfo);
            }
            else if (asTaxonomyMultiFieldInfo != null)
            {
                // this call will take care of calling Update() on field
                this.ApplyTaxonomyMultiFieldValues(fieldCollection, field, asTaxonomyMultiFieldInfo);
            }
            //else if (fieldInfo is DateTimeFieldInfo)
            //{
            //    FieldInfo<DateTime?> doubleBasedField = fieldInfo as FieldInfo<DateTime?>;

            //    if (doubleBasedField.DefaultValue.HasValue)
            //    {
            //        field.DefaultValue = SPUtility.CreateISO8601DateTimeFromSystemDateTime(doubleBasedField.DefaultValue.Value);
            //    }

            //    field.Update();
            //}
            //else if (fieldInfo is LookupFieldInfo)
            //{
            //    FieldInfo<LookupValue> lookupBasedField = fieldInfo as FieldInfo<LookupValue>;

            //    if (lookupBasedField.DefaultValue != null)
            //    {
            //        field.DefaultValue = new SPFieldLookupValue(lookupBasedField.DefaultValue.Id, lookupBasedField.DefaultValue.Value).ToString();

            //        this.log.Warn(
            //            "Default value ({0}) set on field {1} with type Lookup. SharePoint does not support default values on Lookup fields. Only list items created programmatically will get the default value properly set. Setting a Lookup-field default value will not be respected by your lists' NewForm.aspx item creation form.",
            //            field.DefaultValue,
            //            field.InternalName);
            //    }

            //    field.Update();
            //}
            //else if (fieldInfo is LookupMultiFieldInfo)
            //{
            //    FieldInfo<LookupValueCollection> lookupCollectionBasedField = fieldInfo as FieldInfo<LookupValueCollection>;

            //    if (lookupCollectionBasedField.DefaultValue != null)
            //    {
            //        LookupValueCollection defaultCollection = lookupCollectionBasedField.DefaultValue;
            //        SPFieldLookupValueCollection tempSharePointCollection = new SPFieldLookupValueCollection();

            //        foreach (LookupValue defaultVal in defaultCollection)
            //        {
            //            tempSharePointCollection.Add(new SPFieldLookupValue(defaultVal.Id, defaultVal.Value));
            //        }

            //        field.DefaultValue = tempSharePointCollection.ToString();

            //        this.log.Warn(
            //            "Default value ({0}) set on field {1} with type LookupMulti. SharePoint does not support default values on Lookup fields. Only list items created programmatically will get the default value properly set. Setting a Lookup-field default value will not be respected by your lists' NewForm.aspx item creation form.",
            //            field.DefaultValue,
            //            field.InternalName);
            //    }

            //    field.Update();
            //}
            //else if (fieldInfo is UserFieldInfo)
            //{
            //    FieldInfo<UserValue> userBasedField = fieldInfo as FieldInfo<UserValue>;

            //    if (userBasedField.DefaultValue != null)
            //    {
            //        field.DefaultValue = new SPFieldUserValue(fieldCollection.Web, userBasedField.DefaultValue.Id, HttpUtility.HtmlEncode(userBasedField.DefaultValue.DisplayName)).ToString();

            //        this.log.Warn(
            //            "Default value ({0}) set on field {1} with type User. SharePoint does not support default values on User fields. Only list items created programmatically will get the default value properly set. Setting a User-field default value may break your lists' NewForm.aspx people pickers. User folder metadata defaults for User default values instead.",
            //            field.DefaultValue,
            //            field.InternalName);
            //    }

            //    field.Update();
            //}
            //else if (fieldInfo is UserMultiFieldInfo)
            //{
            //    FieldInfo<UserValueCollection> userlookupCollectionBasedField = fieldInfo as FieldInfo<UserValueCollection>;

            //    if (userlookupCollectionBasedField.DefaultValue != null)
            //    {
            //        UserValueCollection defaultCollection = userlookupCollectionBasedField.DefaultValue;
            //        SPFieldUserValueCollection tempSharePointCollection = new SPFieldUserValueCollection();

            //        foreach (UserValue defaultVal in defaultCollection)
            //        {
            //            tempSharePointCollection.Add(new SPFieldUserValue(fieldCollection.Web, defaultVal.Id, HttpUtility.HtmlEncode(defaultVal.DisplayName)));
            //        }

            //        field.DefaultValue = tempSharePointCollection.ToString();

            //        this.log.Warn(
            //            "Default value ({0}) set on field {1} with type UserMulti. SharePoint does not support default values on User fields. Only list items created programmatically will get the default value properly set. Setting a User-field default value may break your lists' NewForm.aspx people pickers. User folder metadata defaults for User default values instead.",
            //            field.DefaultValue,
            //            field.InternalName);
            //    }

            //    field.Update();
            //}
            //else if (fieldInfo is MediaFieldInfo)
            //{
            //    FieldInfo<MediaValue> mediaBasedField = fieldInfo as FieldInfo<MediaValue>;

            //    if (mediaBasedField.DefaultValue != null)
            //    {
            //        var defaultVal = mediaBasedField.DefaultValue;

            //        var fieldValue = new MediaFieldValue()
            //        {
            //            Title = defaultVal.Title,
            //            MediaSource = defaultVal.Url,
            //            PreviewImageSource = defaultVal.PreviewImageUrl,
            //            DisplayMode = defaultVal.DisplayMode,
            //            TemplateSource = defaultVal.XamlTemplateUrl,
            //            InlineHeight = defaultVal.InlineHeight,
            //            InlineWidth = defaultVal.InlineWidth,
            //            AutoPlay = defaultVal.IsAutoPlay,
            //            Loop = defaultVal.IsLoop
            //        };

            //        field.DefaultValue = fieldValue.ToString();
            //    }

            //    field.Update();
            //}
            //else
            //{
            //    // Some preceding changed be need to be persisted
            //    field.Update();
            //}

            //field.Update();

            //field = RefetchFieldToGetLatestVersionAndAvoidUpdateConflicts(fieldCollection, fieldInfo);

            // Tiny bit of ugly reflection here: we assume that all implementations of IFieldInfo will 
            // derive from FieldInfo<T>, which in turn lets us assume a DefaultValue property will always
            // be there for us to create our FieldValueInfo (which simply needs an untyped object as value).
            FieldValueInfo defaultValue = new FieldValueInfo(fieldInfo, fieldInfo.GetType().GetProperty("DefaultValue").GetValue(fieldInfo));

            if (defaultValue != null)
            {
                this.fieldValueWriter.WriteValueToFieldDefault(fieldCollection, defaultValue);
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
        }

        private void ApplyTaxonomyFieldValues(SPFieldCollection fieldCollection, SPField field, TaxonomyFieldInfo taxonomyFieldInfo)
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

                //this.ApplyTermStoreMapping(fieldCollection, taxonomyFieldInfo, taxonomyFieldInfo.TermStoreMapping);
            }
            else
            {
                // the term store mapping is null, we should make sure the field is unmapped
                ClearTermStoreMapping(fieldCollection, taxonomyFieldInfo);
            }

            // Set the default value for the field
            //if (taxonomyFieldInfo.DefaultValue != null)
            //{
            //    // If term store mapping was applied, the field instance is now stale (the field definition got updated 
            //    // through another instance of the same SPField). We need to re-fetch the field to get the very latest.
            //    field = RefetchFieldToGetLatestVersionAndAvoidUpdateConflicts(fieldCollection, taxonomyFieldInfo);
            //    this.taxonomyHelper.SetDefaultTaxonomyFieldValue(fieldCollection.Web, field as TaxonomyField, taxonomyFieldInfo.DefaultValue);
            //}
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

                //this.taxonomyHelper.AssignTermStoreMappingToField(field.)
                //this.ApplyTermStoreMapping(fieldCollection, taxonomyMultiFieldInfo, taxonomyMultiFieldInfo.TermStoreMapping);
            }
            else
            {
                // the term store mapping is null, we should make sure the field is unmapped
                ClearTermStoreMapping(fieldCollection, taxonomyMultiFieldInfo);
            }

            // Set the default value for the field
            //if (taxonomyMultiFieldInfo.DefaultValue != null)
            //{
            //    // If term store mapping was applied, the field instance is now stale (the field definition got updated 
            //    // through another instance of the same SPField). We need to re-fetch the field to get the very latest.
            //    field = RefetchFieldToGetLatestVersionAndAvoidUpdateConflicts(fieldCollection, taxonomyMultiFieldInfo);
            //    this.taxonomyHelper.SetDefaultTaxonomyFieldMultiValue(fieldCollection.Web, field as TaxonomyField, taxonomyMultiFieldInfo.DefaultValue);
            //}
        }

        //private void ApplyTermStoreMapping(SPFieldCollection fieldCollection, IFieldInfo fieldInfo, TaxonomyContext taxonomyMappingContext)
        //{
        //    string termSubsetName = string.Empty;
        //    if (taxonomyMappingContext.TermSubset != null)
        //    {
        //        termSubsetName = taxonomyMappingContext.TermSubset.Label;
        //    }

        //    // Metadata mapping configuration
        //    SPList parentList = null;

        //    // Try to see if we're playing with a List-field collection or a Web-field collection context
        //    if (TryGetListFromFieldCollection(fieldCollection, out parentList))
        //    {
        //        // Ensure this term set mapping on the List-specific field only
        //        if (taxonomyMappingContext.Group != null)
        //        {
        //            // Term set mapping on a global farm-wide term set.
        //            this.taxonomyHelper.AssignTermSetToListColumn(
        //                parentList,
        //                fieldInfo.Id,
        //                taxonomyMappingContext.Group.Name,
        //                taxonomyMappingContext.TermSet.Labels[fieldCollection.Web.UICulture],
        //                termSubsetName);
        //        }
        //        else
        //        {
        //            // Term set mapping on a local site-collection-specific term set.
        //            this.taxonomyHelper.AssignTermSetToListColumn(
        //                parentList,
        //                fieldInfo.Id,
        //                taxonomyMappingContext.TermSet.Labels[fieldCollection.Web.UICulture],
        //                termSubsetName);
        //        }
        //    }
        //    else 
        //    {
        //        // Ensure this field accross the web (i.e. site column + all usages of the field accross all the web's lists)
        //        if (taxonomyMappingContext.Group != null)
        //        {
        //            // Term set mapping on a global farm-wide term set.
        //            this.taxonomyHelper.AssignTermSetToSiteColumn(
        //                fieldCollection.Web,
        //                fieldInfo.Id,
        //                taxonomyMappingContext.Group.Name,
        //                taxonomyMappingContext.TermSet.Labels[fieldCollection.Web.UICulture],
        //                termSubsetName);
        //        }
        //        else
        //        {
        //            // Term set mapping on a local site-collection-specific term set.
        //            this.taxonomyHelper.AssignTermSetToSiteColumn(
        //                fieldCollection.Web,
        //                fieldInfo.Id,
        //                taxonomyMappingContext.TermSet.Labels[fieldCollection.Web.UICulture],
        //                termSubsetName);
        //        }
        //    }
        //}

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

        private static void SetFieldMiscProperties(SPField field, IFieldInfo fieldInfo)
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
