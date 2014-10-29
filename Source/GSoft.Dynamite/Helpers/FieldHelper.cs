using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Xml.Linq;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebPartPages.Communication;
using IFieldInfo = GSoft.Dynamite.Definitions.IFieldInfo;
using GSoft.Dynamite.ValueTypes;
using GSoft.Dynamite.Taxonomy;

namespace GSoft.Dynamite.Helpers
{
    /// <summary>
    /// Helper class for managing SP Fields.
    /// </summary>
    public class FieldHelper : IFieldHelper
    {
        private readonly ILogger logger;
        private readonly ITaxonomyHelper taxonomyHelper;

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="taxonomyHelper">The taxonomy helper</param>
        public FieldHelper(ILogger logger, ITaxonomyHelper taxonomyHelper)
        {
            this.logger = logger;
            this.taxonomyHelper = taxonomyHelper;
        }

        /// <summary>
        /// Sets the lookup field to a list.
        /// </summary>
        /// <param name="web">The web the field and list will be in.</param>
        /// <param name="fieldId">The lookup field id.</param>
        /// <param name="listUrl">The list URL of the list we want to get the information from.</param>
        /// <exception cref="System.ArgumentNullException">All null parameters.</exception>
        /// <exception cref="System.ArgumentException">Unable to find the lookup field.;lookupField</exception>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "The GetList method for SP requires a string url.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        [Obsolete("Use method 'SetLookupToList' with SPFieldCollection as first parameter.")]
        public void SetLookupToList(SPWeb web, Guid fieldId, string listUrl)
        {
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }

            if (fieldId == null)
            {
                throw new ArgumentNullException("fieldId");
            }

            if (string.IsNullOrEmpty(listUrl))
            {
                throw new ArgumentNullException("listUrl");
            }

            this.logger.Info("Start method 'SetLookupToList' for field id: '{0}'", fieldId);
            
            // Get the field.
            SPFieldLookup lookupField = this.GetFieldById(web.Fields, fieldId) as SPFieldLookup;
            if (lookupField == null)
            {
                throw new ArgumentException("Unable to find the lookup field.", "fieldId");
            }

            // Get the list
            SPList lookupList = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, listUrl));

            // Configure the lookup field.
            this.SetLookupToList(lookupField, lookupList);

            this.logger.Info("End method 'SetLookupToList'.");
        }

        /// <summary>
        /// Sets the lookup to a list.
        /// </summary>
        /// <param name="fieldCollection">The field collection.</param>
        /// <param name="fieldId">The field identifier of the lookup field.</param>
        /// <param name="lookupList">The lookup list.</param>
        /// <exception cref="System.ArgumentNullException">
        /// fieldCollection
        /// or
        /// fieldId
        /// or
        /// lookupList
        /// </exception>
        /// <exception cref="System.ArgumentException">Unable to find the lookup field.;fieldId</exception>
        public void SetLookupToList(SPFieldCollection fieldCollection, Guid fieldId, SPList lookupList)
        {
            if (fieldCollection == null)
            {
                throw new ArgumentNullException("fieldCollection");
            }

            if (fieldId == null)
            {
                throw new ArgumentNullException("fieldId");
            }

            if (lookupList == null)
            {
                throw new ArgumentNullException("lookupList");
            }

            this.logger.Info("Start method 'SetLookupToList' for field id: '{0}'", fieldId);

            // Get the field.
            SPFieldLookup lookupField = this.GetFieldById(fieldCollection, fieldId) as SPFieldLookup;
            if (lookupField == null)
            {
                throw new ArgumentException("Unable to find the lookup field.", "fieldId");
            }

            // Configure the lookup field.
            this.SetLookupToList(lookupField, lookupList);

            this.logger.Info("End method 'SetLookupToList'.");
        }

        /// <summary>
        /// Sets the lookup to a list.
        /// </summary>
        /// <param name="lookupField">The lookup field.</param>
        /// <param name="lookupList">The lookup list.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The parameter 'lookupField' cannot be null.;lookupField
        /// or
        /// The parameter 'lookupList' cannot be null.;lookupList
        /// </exception>
        public void SetLookupToList(SPFieldLookup lookupField, SPList lookupList)
        {
            if (lookupField == null)
            {
                throw new ArgumentNullException("The parameter 'lookupField' cannot be null.", "lookupField");
            }

            if (lookupList == null)
            {
                throw new ArgumentNullException("The parameter 'lookupList' cannot be null.", "lookupList");
            }

            this.logger.Info("Start method 'SetLookupToList' for field with id '{0}'", lookupField.Id);

            // Get the fields schema xml.
            XDocument fieldSchema = XDocument.Parse(lookupField.SchemaXml);
            XElement root = fieldSchema.Root;

            // Reset the attributes list and source id.
            root.SetAttributeValue("List", lookupList.ID);
            root.SetAttributeValue("SourceID", lookupList.ParentWeb.ID);

            // Update the lookup field.
            lookupField.SchemaXml = fieldSchema.ToString();

            this.logger.Info("End method 'SetLookupToList'.");
        }

        /// <summary>
        /// Gets the field by identifier.
        /// Returns null if the field is not found in the collection.
        /// </summary>
        /// <param name="fieldCollection">The field collection.</param>
        /// <param name="fieldId">The field identifier.</param>
        /// <returns>The SPField.</returns>
        public SPField GetFieldById(SPFieldCollection fieldCollection, Guid fieldId)
        {
            if (fieldCollection == null)
            {
                throw new ArgumentNullException("fieldCollection");
            }

            if (fieldId == null)
            {
                throw new ArgumentNullException("fieldId");
            }

            SPField field = null;
            if (fieldCollection.Contains(fieldId))
            {
                field = fieldCollection[fieldId] as SPField;
            }

            return field;
        }

        /// <summary>
        /// Adds a collection of fields defined in xml to a collection of fields.
        /// </summary>
        /// <param name="fieldCollection">The SPField collection.</param>
        /// <param name="fieldsXml">The field schema XMLs.</param>
        /// <returns>A collection of strings that contain the internal name of the new fields.</returns>
        /// <exception cref="System.ArgumentNullException">Null fieldsXml parameter</exception>
        public IList<string> EnsureField(SPFieldCollection fieldCollection, XDocument fieldsXml)
        {
            if (fieldsXml == null)
            {
                throw new ArgumentNullException("fieldsXml");
            }

            this.logger.Info("Start method 'EnsureFields'");

            IList<string> internalNames = new List<string>();

            // Get all the field declerations in the XmlDocument.
            var fields = fieldsXml.Root.Elements("Field");

            this.logger.Info("Found '{0}' fields to add.", fields.Count());

            foreach (XElement field in fields)
            {
                // Add the field to the collection.
                string internalname = this.EnsureField(fieldCollection, field);
                if (!string.IsNullOrEmpty(internalname))
                {
                    internalNames.Add(internalname);
                }
            }

            this.logger.Info("End method 'EnsureFields'. Returning '{0}' internal names.", internalNames.Count);

            // Return a list of the fields that where created.
            return internalNames;
        }

        /// <summary>
        /// Adds a field defined in xml to a collection of fields.
        /// </summary>
        /// <param name="fieldCollection">The SPField collection.</param>
        /// <param name="fieldXml">The field XML schema.</param>
        /// <returns>
        /// A string that contains the internal name of the new field.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// fieldCollection
        /// or
        /// fieldXml
        /// </exception>
        /// <exception cref="System.FormatException">Invalid xml.</exception>
        public string EnsureField(SPFieldCollection fieldCollection, XElement fieldXml)
        {
            if (fieldCollection == null)
            {
                throw new ArgumentNullException("fieldCollection");
            }

            if (fieldXml == null)
            {
                throw new ArgumentNullException("fieldXml");
            }

            this.logger.Info("Start method 'EnsureField'");

            Guid id = Guid.Empty;
            string displayName = string.Empty;
            string internalName = string.Empty;

            // Validate the xml of the field and get its 
            if (this.IsFieldXmlValid(fieldXml, out id, out displayName, out internalName))
            {
                // Check if the field already exists. Skip the creation if so.
                if (!this.FieldExists(fieldCollection, displayName, id))
                {
                    // If its a lookup we need to fix up the xml.
                    if (this.IsLookup(fieldXml))
                    {
                        fieldXml = this.FixLookupFieldXml(fieldCollection.Web, fieldXml);
                    }

                    string addedInternalName = fieldCollection.AddFieldAsXml(fieldXml.ToString(), false, SPAddFieldOptions.Default);

                    this.logger.Info("End method 'EnsureField'. Added field with internal name '{0}'", addedInternalName);

                    return addedInternalName;
                }
                else
                {
                    this.logger.Warn("End method 'EnsureField'. Field with id '{0}' and display name '{1}' was not added because it already exists in the collection.", id, displayName);
                    return string.Empty;
                }
            }
            else
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "Unable to create field. Invalid xml. id: '{0}' DisplayName: '{1}' Name: '{2}'", id, displayName, internalName);
                throw new FormatException(msg);
            }
        }

        /// <summary>
        /// Ensure a field
        /// </summary>
        /// <param name="fieldCollection">The field collection</param>
        /// <param name="fieldInfo">The field info configuration</param>
        /// <returns>The internal name of the field</returns>
        public string EnsureField(SPFieldCollection fieldCollection, IFieldInfo fieldInfo)
        {
            string field;

            if (fieldInfo.GetType() == typeof(TaxonomyFieldInfo))
            {
                field = this.EnsureField(fieldCollection, fieldInfo as TaxonomyFieldInfo);
            }
            else
            {
                field = this.EnsureField(fieldCollection, fieldInfo.Schema);
            }

            // Gets the created field
            var createdField = fieldCollection.GetFieldByInternalName(fieldInfo.InternalName);

            // Updates the visibility of the field
            UpdateFieldVisibility(createdField, fieldInfo);

            return field;
        }
        
        /// <summary>
        /// Ensure a taxonomy field
        /// </summary>
        /// <param name="fieldCollection">The field collection</param>
        /// <param name="fieldInfo">The field info configuration</param>
        /// <returns>The internal name of the field</returns>
        public string EnsureField(SPFieldCollection fieldCollection, TaxonomyFieldInfo fieldInfo)
        {
            var field = this.EnsureField(fieldCollection, fieldInfo.Schema);

            // Gets the created field
            var createdField = fieldCollection.GetFieldByInternalName(fieldInfo.InternalName);

            // Updates the visibility of the field
            UpdateFieldVisibility(createdField, fieldInfo);

            // Get the term store default language for term set name
            var termStoreDefaultLanguageLcid = this.taxonomyHelper.GetTermStoreDefaultLanguage(fieldCollection.Web.Site);

            if (fieldInfo.TermStoreMapping != null)
            {
                TaxonomyContext taxContext = fieldInfo.TermStoreMapping;
                string termSubsetName = string.Empty;
                if (taxContext.TermSubset != null)
                {
                    termSubsetName = taxContext.TermSubset.Label;
                }

                // TODO: DefaultValue shouldn't be used for this. Use TaxonomyContext object on TaxonomyFieldInfo instead. DefaultValue should be used for the TermSet-mapped (thx to Context) field.

                // Metadata mapping configuration
                this.taxonomyHelper.AssignTermSetToSiteColumn(
                            fieldCollection.Web,
                            fieldInfo.Id,
                            taxContext.Group.Name,
                            taxContext.TermSet.Labels[new CultureInfo(termStoreDefaultLanguageLcid)],
                            termSubsetName);
            }

            if (fieldInfo.DefaultValue != null)
            {
                SPField newlyCreatedField = fieldCollection[fieldInfo.Id];

                // TODO: create a IFieldValueWriter<ValueType> utility to re-use the proper
                // taxonomy setter logic which is locked up in the TaxonomyValueConverter

                //newlyCreatedField.DefaultValueTyped = 

                //TaxonomyFullValue defaultValue = fieldInfo.DefaultValue;

            }

            return field;
        }

        /// <summary>
        /// Ensure a collection of fields
        /// </summary>
        /// <param name="fieldCollection">The field collection</param>
        /// <param name="fieldInfos">The field info configuration</param>
        /// <returns>The internal names of the field</returns>
        public IEnumerable<string> EnsureField(SPFieldCollection fieldCollection, ICollection<IFieldInfo> fieldInfos)
        {
            var fieldNames = new List<string>();

            foreach (IFieldInfo fieldInfo in fieldInfos)
            {
                fieldNames.Add(this.EnsureField(fieldCollection, fieldInfo));
            }

            return fieldNames;
        }

        private SPField UpdateFieldVisibility(SPField field, IFieldInfo fieldInfo)
        {
            if (field != null)
            {
                field.ShowInListSettings = !fieldInfo.IsHiddenInListSettings;
                field.ShowInDisplayForm = !fieldInfo.IsHiddenInDisplayForm;
                field.ShowInEditForm = !fieldInfo.IsHiddenInEditForm;
                field.ShowInNewForm = !fieldInfo.IsHiddenInNewForm;
                field.Update(true);
            }
            return field;
        }

        private static string GetAttributeValue(XElement fieldXml, string key)
        {
            XAttribute attribute = fieldXml.Attribute(key);
            if (attribute != null)
            {
                return attribute.Value;
            }
            else
            {
                return string.Empty;
            }
        }

        private bool FieldExists(SPFieldCollection fieldCollection, string displayName, Guid fieldId)
        {
            if (fieldCollection.Contains(fieldId))
            {
                // If Id is found in the collection.
                this.logger.Warn("Field with id '{0}' is already in the collection.", fieldId);
                return true;
            }

            SPField field;
            try
            {
                // Throws argument exception if not in collection.
                field = fieldCollection.GetField(displayName);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (field == null)
            {
                // Still can't find the field in the collection
                return false;
            }
            else
            {
                // We found it!
                this.logger.Warn("Field with display name '{0}' is already in the collection.", displayName);
                return true;
            }
        }

        private XElement FixLookupFieldXml(SPWeb web, XElement fieldXml)
        {
            this.logger.Info("Fixing up lookup field xml.");

            // Validate the list attribute is present.
            string list = GetAttributeValue(fieldXml, "List");
            if (string.IsNullOrEmpty(list))
            {
                string displayName = GetAttributeValue(fieldXml, "DisplayName");
                string msg = string.Format(CultureInfo.InvariantCulture, "Unable to create Lookup Field '{0}' because it is missing the 'List' attribute.", displayName);
                throw new ArgumentException(msg);
            }

            // Get the lookup list in the current web.
            string listPath = SPUtility.ConcatUrls(web.ServerRelativeUrl, list);
            SPList lookupList = web.GetList(listPath);

            // Get the required attribute.
            bool required;
            if (!bool.TryParse(GetAttributeValue(fieldXml, "Required"), out required))
            {
                required = false;
            }

            // prepare xml values the same way SharePoint does it...
            string listValue = lookupList.ID.ToString("B").ToUpper(CultureInfo.InvariantCulture);
            string webIdValue = web.ID.ToString();
            string requiredValue = required ? "TRUE" : "FALSE";

            this.logger.Info("Setting field xml attributes, List: '{0}' WebId: '{1}' Required: '{2}'", listValue, webIdValue, requiredValue);

            // Update the xml.
            fieldXml.SetAttributeValue("List", listValue);
            fieldXml.SetAttributeValue("WebId", webIdValue);
            fieldXml.SetAttributeValue("Required", requiredValue);

            // Return the modified xml.
            return fieldXml;
        }

        private bool IsLookup(XElement fieldXml)
        {
            string fieldType = GetAttributeValue(fieldXml, "Type");
            this.logger.Info("Field is of type '{0}'", fieldType);
            return string.Compare(fieldType, "Lookup", true, CultureInfo.InvariantCulture) == 0;
        }

        private bool IsFieldXmlValid(XElement fieldXml, out Guid id, out string displayName, out string internalName)
        {
            id = Guid.Empty;
            displayName = string.Empty;
            internalName = string.Empty;

            // Validate the ID attribute
            string strId = GetAttributeValue(fieldXml, "ID");
            if (string.IsNullOrEmpty(strId))
            {
                this.logger.Fatal("Attribute 'ID' is required.");
                return false;
            }
            else
            {
                try
                {
                    id = new Guid(strId);
                }
                catch (FormatException)
                {
                    this.logger.Fatal("Attribute ID: '{0}' needs to be a guid.", strId);
                    return false;
                }
                catch (OverflowException)
                {
                    this.logger.Fatal("Attribute ID: '{0}' needs to be a guid.", strId);
                    return false;
                }
            }

            // Validate display Name
            displayName = GetAttributeValue(fieldXml, "DisplayName");
            if (string.IsNullOrEmpty(displayName))
            {
                this.logger.Fatal("Attribute 'DisplayName' is required for field with id: '{0}'.", id);
                return false;
            }

            // Validate internal name
            internalName = GetAttributeValue(fieldXml, "Name");
            if (string.IsNullOrEmpty(internalName))
            {
                this.logger.Fatal("Attribute 'Name' is required for field with id: '{0}'.", id);
                return false;
            }

            // Everything is valid.
            return true;
        }
    }
}
