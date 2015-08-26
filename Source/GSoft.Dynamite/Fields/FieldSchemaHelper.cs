using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Helps in formatted SharePoint field schema XML
    /// </summary>
    public class FieldSchemaHelper : IFieldSchemaHelper
    {
        private IResourceLocator resourceLocator;
        private ILogger logger;
        private IFieldLocator fieldLocator;

        /// <summary>
        /// Initializes a new <see cref="FieldChemaHelper"/> instance
        /// </summary>
        /// <param name="resourceLocator">The resource locator</param>
        /// <param name="logger">The logging utility</param>
        /// <param name="fieldLocator">Field finder</param>
        public FieldSchemaHelper(IResourceLocator resourceLocator, ILogger logger, IFieldLocator fieldLocator)
        {
            this.resourceLocator = resourceLocator;
            this.logger = logger;
            this.fieldLocator = fieldLocator;
        }

        /// <summary>
        /// Generates the Field XML for a site column definition
        /// </summary>
        /// <param name="fieldInfo">The field definition for which we want to print out the full XML schema</param>
        /// <returns>The XML schema of the field</returns>
        public XElement SchemaForField(BaseFieldInfo fieldInfo)
        {
            var schema = new XElement(
                "Field",
                new XAttribute("Name", fieldInfo.InternalName),
                new XAttribute("Type", fieldInfo.FieldType),
                new XAttribute("ID", "{" + fieldInfo.Id + "}"),
                new XAttribute("StaticName", fieldInfo.InternalName),
                new XAttribute("DisplayName", this.resourceLocator.GetResourceString(fieldInfo.ResourceFileName, fieldInfo.DisplayNameResourceKey)),
                new XAttribute("Group", this.resourceLocator.GetResourceString(fieldInfo.ResourceFileName, fieldInfo.GroupResourceKey)));

            // Check if description is defined
            if (!string.IsNullOrEmpty(fieldInfo.DescriptionResourceKey))
            {
                schema.Add(new XAttribute(
                    "Description", 
                    this.resourceLocator.GetResourceString(fieldInfo.ResourceFileName, fieldInfo.DescriptionResourceKey)));
            }

            // Check the Required type
            if (fieldInfo.Required == RequiredType.Required)
            {
                schema.Add(new XAttribute("Required", "TRUE"));
            }

            if (fieldInfo.Required == RequiredType.NotRequired)
            {
                schema.Add(new XAttribute("Required", "FALSE"));
            }

            // Hidden state
            if (fieldInfo.IsHidden)
            {
                schema.Add(new XAttribute("Hidden", "TRUE"));
            }

            // Show in Display Form
            if (fieldInfo.IsHiddenInDisplayForm)
            {
                schema.Add(new XAttribute("ShowInDisplayForm", "FALSE"));
            }

            // Show in Edit Form
            if (fieldInfo.IsHiddenInEditForm)
            {
                schema.Add(new XAttribute("ShowInEditForm", "FALSE"));
            }

            // Show in new Form
            if (fieldInfo.IsHiddenInNewForm)
            {
                schema.Add(new XAttribute("ShowInNewForm", "FALSE"));
            }

            // Show in List settings
            if (fieldInfo.IsHiddenInListSettings)
            {
                schema.Add(new XAttribute("ShowInListSettings", "FALSE"));
            }
            else
            {
                schema.Add(new XAttribute("ShowInListSettings", "TRUE"));
            }

            // Extend the basic field scheme (everything listed above here) with the specific field type's extra attributes
            return fieldInfo.Schema(schema);
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
        public SPField EnsureFieldFromSchema(SPFieldCollection fieldCollection, XElement fieldXml)
        {
            if (fieldCollection == null)
            {
                throw new ArgumentNullException("fieldCollection");
            }

            if (fieldXml == null)
            {
                throw new ArgumentNullException("fieldXml");
            }

            // Gotta use reflection to figure out if the field collection comes from
            // a content type (and thus should be read-only)
            bool isContentTypeFieldCollection = (bool)typeof(SPFieldCollection)
                    .GetField("m_FromContentType", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(fieldCollection);
            if (isContentTypeFieldCollection)
            {
                throw new ArgumentException("EnsureField - Impossible to ensure field on Content Type's field collection - it should be considered as read-only. Use IContentTypeHelper.EnsureContentType instead (which will update the content type's FieldRefs collection).");
            }

            this.logger.Info("Start method 'EnsureField'");

            Guid id = Guid.Empty;
            string displayName = string.Empty;
            string internalName = string.Empty;
            string typeName = string.Empty;

            // Validate the xml of the field and get its properties
            if (this.IsFieldXmlValid(fieldXml, out id, out displayName, out internalName, out typeName))
            {
                // If its a lookup we need to fix up the xml.
                if (this.IsLookup(fieldXml))
                {
                    fieldXml = this.FixLookupFieldXml(fieldCollection.Web, fieldXml);
                }

                // Check if the field already exists. Skip the creation if so.
                if (!this.FieldExists(fieldCollection, internalName, id))
                {
                    // We want to create the field: if we're trying to add field on a list field collection,
                    // then chances are the field is already defined on the parent root web (we actually enforce
                    // this in the calling FieldHelper). In such a case, we need to re-use the existing field definition,
                    // because using .AddFieldAsXml directly on the list field collection would cause a field
                    // with an InternalName==ParentRootWebFieldDisplayName (weird bug, really - using AddFieldAsXml
                    // on a list's SPFieldCollection is just a bad idea: use the already provisioned site column 
                    // whenever possible). 
                    string addedInternalName = string.Empty;
                    SPField parentRootWebExistingField = null;
                    SPWeb rootWeb = fieldCollection.Web.Site.RootWeb;
                    SPFieldCollection rootWebFields = rootWeb.Fields;
                    if (!this.FieldExists(rootWebFields, internalName, id))
                    {
                        if (fieldCollection.List == null && fieldCollection.Web.ID == rootWeb.ID)
                        {
                            rootWebFields = fieldCollection;
                        }

                        // Add on Root Web if it wasn't done before
                        addedInternalName = rootWebFields.AddFieldAsXml(fieldXml.ToString(), false, SPAddFieldOptions.Default);
                    }

                    // Re-use the parent field definition
                    parentRootWebExistingField = rootWebFields[id];

                    // Make sure we're adding the field on a List or on a different web than the root web:
                    // we don't want to ensure the field twice on the root web
                    if (fieldCollection.List != null)
                    {
                        if (parentRootWebExistingField.EnforceUniqueValues)
                        {
                            // Before we ensure the site column on a list, gotta make sure
                            // that, on a list, EnforceUniqueValues=TRUE IF-AND-ONLY-IF Indexed=TRUE
                            parentRootWebExistingField.Indexed = true;
                        }

                        addedInternalName = fieldCollection.Add(parentRootWebExistingField);

                        // Then update the list column with the new list-specific or web-specfic definition
                        var createdField = this.fieldLocator.GetFieldById(fieldCollection, id);
                        createdField.SchemaXml = fieldXml.ToString();   // TODO: we should probably do a more granular update (property by property, 
                                                                        // only when needed) instead of brutally overwriting the schema XML like this.
                        createdField.Update();
                    }
                    else if (fieldCollection.Web.ID != rootWeb.ID)
                    {
                        if (!string.IsNullOrEmpty(addedInternalName))
                        {
                            // We just added the site column on the RootWeb moments ago. Adding the same field on any sub-web will
                            // be impossible (will trigger "Same internal name already exist" kindof error).
                            // This may be a bit misleading but really enforces the "always provision site columns on RootWeb" motto
                            this.logger.Warn("EnsureFieldFromSchema - Field was ensured on RootWeb instead of sub-web. Best practice: keep your site column definitions on the root web / avoid scattering them across your sub-webs.");
                        }
                        else
                        {
                            // User was trying to define a custom field definition on a subweb, while that site column
                            // is already defined on root web - which is impossible.
                            throw new InvalidOperationException("EnsureFieldFromSchema - Cannot ensure field on sub-web if site column already exists at root web-level. Best practive: create all your site columns on the RootWeb only.");
                        }
                    }

                    if (internalName != addedInternalName)
                    {
                        // Internal name changed abruptly! (probably ended up being set as DisplayName)
                        // This can happen when .AddFieldAsXml is used directly on a list field collection.
                        // Some code above tried to detect the situation and act accordingly.
                        // It can be surprising, when this happens: so better to have it explode violently.
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.InvariantCulture, 
                                "Tried to add field with internal name {0}. Final field was created with internal name {1}.",
                                internalName,
                                addedInternalName));
                    }

                    this.logger.Info("End method 'EnsureField'. Added field with internal name '{0}'", addedInternalName);
                }
                else
                { 
                    var alreadyCreatedField = this.fieldLocator.GetFieldById(fieldCollection, id);

                    if (alreadyCreatedField != null && alreadyCreatedField.InternalName == internalName && alreadyCreatedField.TypeAsString == typeName)
                    {
                        // Only try updating if we managed to find the field by its ID and if 
                        // the existing field has the same internal name (changing the internal
                        // name should be impossible).
                        alreadyCreatedField.SchemaXml = fieldXml.ToString();    // TODO: we should probably do a more granular update (property by property, 
                                                                                // only when needed) instead of brutally overwriting the schema XML like this.
                        alreadyCreatedField.Update();
                        this.logger.Info("End method 'EnsureField'. Field with id '{0}', display name '{1}' and internal name '{2}' was not added because it already exists in the collection.", id, displayName, internalName);
                    }
                }
            }
            else
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "Unable to create field. Invalid xml. id: '{0}' DisplayName: '{1}' Name: '{2}'", id, displayName, internalName);
                throw new FormatException(msg);
            }

            // Return the newly created or the existing field
            var existingField = this.fieldLocator.GetFieldById(fieldCollection, id);

            if (existingField == null)
            {
                try
                {
                    // Guid match failed. A field may already exist with the same internal name but a different Guid.
                    existingField = fieldCollection.GetFieldByInternalName(internalName);
                }
                catch (ArgumentException)
                {
                    // We're probably on a sub-web's field collection, and we just sneakily created
                    // the site column on the RootWeb instead of on the web the user actually wanted.
                    existingField = fieldCollection.Web.Site.RootWeb.Fields[id];

                    if (existingField == null)
                    {
                        existingField = fieldCollection.Web.Site.RootWeb.Fields.GetFieldByInternalName(internalName);
                    }
                }
            }

            return existingField;
        }

        private bool IsFieldXmlValid(XElement fieldXml, out Guid id, out string displayName, out string internalName, out string fieldTypeName)
        {
            id = Guid.Empty;
            displayName = string.Empty;
            internalName = string.Empty;
            fieldTypeName = string.Empty;

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
            
            // Validate internal name
            fieldTypeName = GetAttributeValue(fieldXml, "Type");
            if (string.IsNullOrEmpty(fieldTypeName))
            {
                this.logger.Fatal("Attribute 'Type' is required for field with id: '{0}'.", id);
                return false;
            }

            // Everything is valid.
            return true;
        }

        private bool IsLookup(XElement fieldXml)
        {
            string fieldType = GetAttributeValue(fieldXml, "Type");
            this.logger.Info("Field is of type '{0}'", fieldType);
            return string.Compare(fieldType, "Lookup", StringComparison.OrdinalIgnoreCase) == 0;
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

        private bool FieldExists(SPFieldCollection fieldCollection, string internalName, Guid fieldId)
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
                field = fieldCollection.GetFieldByInternalName(internalName);
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
                this.logger.Warn("Field with display name '{0}' is already in the collection.", internalName);
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
            SPList lookupList = null;
            Guid listGuid = Guid.Empty;
            if (Guid.TryParse(list, out listGuid))
            {
                if (Guid.Empty == listGuid)
                {
                    throw new ArgumentException("You need to specify a non-empty list Guid for your lookup field definition.");
                }

                // Fetch list by its ID
                lookupList = web.Lists[listGuid];
            }
            else
            {
                // Fetch list by its web-relative URL
                string listPath = SPUtility.ConcatUrls(web.ServerRelativeUrl, list);
                lookupList = web.GetList(listPath);
            }

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
    }
}
